using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;
using Timer = System.Timers.Timer;

namespace Emphasis.OpenCL
{
	public interface IOclMemoryPool
	{
		nint RentBuffer<T>(nint contextId, int memoryFlags, int minSize) where T : unmanaged;
		void ReturnBuffer(nint bufferId);
	}

	public class OclMemoryPool : IOclMemoryPool
	{
		public static OclMemoryPool Default { get; } = new OclMemoryPool();

		private readonly ConcurrentDictionary<(nint contextId, int memoryFlags), ContextMemoryFlagsBucket> _contextBuckets = new();
		private readonly Timer _gcTimer;

		public OclMemoryPool()
		{
			_gcTimer = new Timer();
			_gcTimer.Elapsed += (sender, args) => TrimExcess();
			_gcTimer.Interval = TimeSpan.FromSeconds(1).Milliseconds;
			_gcTimer.Start();
		}

		public nint RentBuffer<T>(nint contextId, int memoryFlags, int minSize) where T : unmanaged
		{
			var api = OclApi.Value;

			if ((memoryFlags & (int) CLEnum.MemCopyHostPtr) != 0)
				throw new Exception("Unable to use CL_MEM_COPY_HOST_PTR as memory flags within OpenCL buffer memory pool.");
			
			if (!_contextBuckets.TryGetValue((contextId, memoryFlags), out var contextBucket))
			{
				contextBucket = _contextBuckets.GetOrAdd((contextId, memoryFlags), new ContextMemoryFlagsBucket(contextId, memoryFlags));
			}

			if (!TryRentBuffer(contextBucket, minSize, out var bufferBucket))
			{

			}
			
			var createdBufferId = CreateBuffer<T>(contextId, minSize, memoryFlags);
			return createdBufferId;
		}

		private bool TryRentBuffer(ContextMemoryFlagsBucket contextBucket, int minSize, out BufferBucket bufferBucket)
		{
			bufferBucket = default;
			var bufferBuckets = contextBucket.BufferBuckets;

			int n0, n1;
			var rwLock = contextBucket.ReaderWriterLock;
			rwLock.EnterReadLock();
			try
			{
				var count = bufferBuckets.Count;
				if (count == 0)
					return false;
				
				n1 = bufferBuckets.Count - 1;
				var max = bufferBuckets.Keys[n1];
				if (max < minSize)
					return false;

				n0 = 0;
				while (n0 != n1)
				{
					var min = bufferBuckets.Keys[n0];
					if (min < minSize)
						n0 = (n0 + n1) / 2;
				}

				if (bufferBuckets.Keys[n0] > 2 * minSize)
					return false;
			}
			finally
			{
				rwLock.ExitReadLock();
			}
			
			return false;
		}

		public void ReturnBuffer(nint bufferId)
		{
			throw new NotImplementedException();
		}

		public void TrimExcess()
		{
			
		}

		internal class ContextMemoryFlagsBucket
		{
			public nint ContextId { get; }
			public int MemoryFlags { get; }

			public ReaderWriterLockSlim ReaderWriterLock = new();
			public readonly SortedList<int, BufferBucket> BufferBuckets = new();

			public ContextMemoryFlagsBucket(nint contextId, int memoryFlags)
			{
				ContextId = contextId;
				MemoryFlags = memoryFlags;
			}
		}

		internal class BufferBucket
		{
			
		}


		internal readonly struct OclBuffer : IComparable<OclBuffer>
		{
			public nint BufferId { get; }
			public int Size { get; }
			
			public OclBuffer(nint bufferId, int size)
			{
				BufferId = bufferId;
				Size = size;
			}

			public int CompareTo(OclBuffer other)
			{
				return Size.CompareTo(other.Size);
			}
		}
	}
}
