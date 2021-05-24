using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;
using Timer = System.Timers.Timer;

namespace Emphasis.OpenCL
{
	public interface IOclMemoryPool : ICancelable
	{
		nint RentBuffer<T>(nint contextId, int memoryFlags, long minSize) where T : unmanaged;
		void ReturnBuffer(nint bufferId);
	}

	public class OclMemoryPool : IOclMemoryPool
	{
		public static OclMemoryPool Shared { get; } = new OclMemoryPool();

		private readonly ConcurrentDictionary<(nint contextId, int memoryFlags), ContextMemoryFlagsBucket> _contextBuckets = new();
		private readonly Timer _gcTimer;
		private readonly CompositeDisposable _disposable;

		public TimeSpan TrimmingInterval { get; } = TimeSpan.FromSeconds(1);
		public TimeSpan ReleaseInterval { get; } = TimeSpan.FromSeconds(5);

		public OclMemoryPool()
		{
			_gcTimer = new Timer();
			_gcTimer.Elapsed += (_, _) => TrimExcess();
			_gcTimer.Interval = TrimmingInterval.Milliseconds;
			_gcTimer.Start();

			_disposable = new CompositeDisposable(_gcTimer);
		}

		public nint RentBuffer<T>(nint contextId, int memoryFlags, long minSize) where T : unmanaged
		{
			if ((memoryFlags & (int) CLEnum.MemCopyHostPtr) != 0)
				throw new Exception("Unable to use CL_MEM_COPY_HOST_PTR as memory flags within OpenCL buffer memory pool.");
			
			if (!_contextBuckets.TryGetValue((contextId, memoryFlags), out var contextBucket))
				contextBucket = _contextBuckets.GetOrAdd((contextId, memoryFlags), new ContextMemoryFlagsBucket(contextId, memoryFlags));

			if (TryRentBuffer(contextBucket, minSize, out var bufferId))
				return bufferId;
			
			var createdBufferId = CreateBuffer<T>(contextId, minSize, memoryFlags);
			return createdBufferId;
		}

		private bool TryRentBuffer(ContextMemoryFlagsBucket contextBucket, long minSize, out nint bufferId)
		{
			bufferId = default;
			
			lock (contextBucket)
			{
				var buffers = contextBucket.Buffers;
				var bufferIds = contextBucket.BufferIds;

				var count = buffers.Count;
				if (count == 0)
					return false;

				var n0 = 0;
				var n1 = count - 1;
				var keys = buffers.Keys;
				while (n0 != n1)
				{
					var (size0, _) = keys[n0];
					if (size0 < minSize)
					{
						n0 = (n0 + n1) / 2;
						continue;
					}

					var (size1, _) = keys[n1];
					if (size1 >= minSize)
					{
						n1 = (n0 + n1) / 2;
					}
				}

				var (size, _) = keys[n0];
				if (size < minSize || size > 2 * minSize)
					return false;

				bufferId = buffers.Values[n0];
				bufferIds.Remove(bufferId);
				buffers.RemoveAt(n0);
				return true;
			}
		}

		public void ReturnBuffer(nint bufferId)
		{
			var memoryFlags = GetMemObjectMemoryFlags(bufferId);
			if ((memoryFlags & (int)CLEnum.MemCopyHostPtr) != 0)
				throw new Exception("Unable to use CL_MEM_COPY_HOST_PTR as memory flags within OpenCL buffer memory pool.");

			var contextId = GetMemObjectContext(bufferId);
			if (!_contextBuckets.TryGetValue((contextId, memoryFlags), out var contextBucket))
				contextBucket = _contextBuckets.GetOrAdd((contextId, memoryFlags), new ContextMemoryFlagsBucket(contextId, memoryFlags));

			var size = GetMemObjectSize(bufferId);
			lock (contextBucket)
			{
				var buffers = contextBucket.Buffers;
				var bufferIds = contextBucket.BufferIds;

				if (!bufferIds.Add(bufferId))
					return;

				buffers.Add((size, DateTime.Now.Ticks), bufferId);
			}
		}

		public void TrimExcess()
		{
			foreach (var pair in _contextBuckets)
			{
				var bucket = pair.Value;
				lock (bucket)
				{
					var buffers = bucket.Buffers;
					var bufferIds = bucket.BufferIds;

					var count = buffers.Count;
					if (count == 0)
						return;

					var releaseLimit = DateTime.Now.Ticks - ReleaseInterval.Ticks;
					for (var i = 0; i < buffers.Count;)
					{
						var (_, ticks) = buffers.Keys[i];
						if (ticks < releaseLimit)
						{
							var bufferId = buffers.Values[i];
							bufferIds.Remove(bufferId);
							buffers.RemoveAt(i);
							continue;
						}
						i++;
					}
				}
			}
		}

		internal class ContextMemoryFlagsBucket
		{
			public nint ContextId { get; }
			public int MemoryFlags { get; }
			
			public readonly SortedList<(long size, long ticks), nint> Buffers = new();
			public readonly HashSet<nint> BufferIds = new();

			public ContextMemoryFlagsBucket(nint contextId, int memoryFlags)
			{
				ContextId = contextId;
				MemoryFlags = memoryFlags;
			}
		}

		public bool IsDisposed => _disposable.IsDisposed;

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}
}
