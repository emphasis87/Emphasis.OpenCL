using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;

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

			lock (contextBucket)
			{
				if (TryRentBuffer(contextBucket, out var rentedBufferId))
					return rentedBufferId;
			}
			
			var createdBufferId = CreateBuffer<T>(contextId, minSize, memoryFlags);
			return createdBufferId;
		}

		private bool TryRentBuffer(ContextMemoryFlagsBucket contextBucket, out nint bufferId)
		{

			var bufferBuckets = contextBucket.BufferBuckets;
			var count = bufferBuckets.Count;
			if (count == 0)
			{
				bufferId = default;
				return false;
			}

			var n0 = 0;
			var n1 = bufferBuckets.Count - 1;
			while (n0 != n1)
			{

			}

			bufferId = default;
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
