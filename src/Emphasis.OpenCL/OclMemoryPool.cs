using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

		private readonly ConcurrentDictionary<(nint contextId, int memFlags), SortedSet<OclBuffer>> _buffers = new();

		public nint RentBuffer<T>(nint contextId, int memoryFlags, int minSize) where T : unmanaged
		{
			var api = OclApi.Value;

			if ((memoryFlags & (int) CLEnum.MemCopyHostPtr) != 0)
				throw new Exception("Unable to use CL_MEM_COPY_HOST_PTR as memory flags within OpenCL buffer memory pool.");
			
			if (!_buffers.TryGetValue((contextId, memoryFlags), out var buffers))
				buffers = _buffers.GetOrAdd((contextId, memoryFlags), new SortedSet<OclBuffer>());
			
			lock (buffers)
			{
				var range = buffers.GetViewBetween(new OclBuffer(default, minSize), new OclBuffer(default, int.MaxValue));
				if (!range.TryGetValue(range.Min, out var buffer))
				{
					range.Remove(buffer);
					return buffer.BufferId;
				}
			}

			var bufferId = CreateBuffer<T>(contextId, minSize, memoryFlags);
			return bufferId;
		}

		public void ReturnBuffer(nint bufferId)
		{
			throw new NotImplementedException();
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
