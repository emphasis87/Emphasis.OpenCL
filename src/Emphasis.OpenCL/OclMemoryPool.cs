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

		private bool TryRentBuffer(ContextMemoryFlagsBucket contextBucket, int minSize, out nint bufferId)
		{
			bufferId = default;

			var buffers = contextBucket.Buffers;
			if (buffers.Count == 0)
				return false;

			var keys = buffers.Keys;

			var count = buffers.Count;
				
				
			n1 = buffers.Count - 1;
			var max = buffers.Keys[n1];
			if (max < minSize)
				return false;

			n0 = 0;
			while (n0 != n1)
			{
				var min = buffers.Keys[n0];
				if (min < minSize)
					n0 = (n0 + n1) / 2;
			}

			if (buffers.Keys[n0] > 2 * minSize)
				return false;
			
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
			
			public readonly SortedList<(int size, long ticks), nint> Buffers = new();

			public ContextMemoryFlagsBucket(nint contextId, int memoryFlags)
			{
				ContextId = contextId;
				MemoryFlags = memoryFlags;
			}
		}
	}
}
