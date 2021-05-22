using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL
{
	public interface IOclMemoryPool
	{
		nint RentBuffer<T>(nint contextId, int minSize, int memFlags) where T : unmanaged;
		void ReturnBuffer(nint bufferId);
	}

	public class OclMemoryPool : IOclMemoryPool
	{
		public static OclMemoryPool Default { get; } = new OclMemoryPool();

		private ConcurrentDictionary<(nint contextId, int memFlags), (SortedSet<>>

		public nint RentBuffer<T>(nint contextId, int minSize, int memFlags) where T : unmanaged
		{
			var api = OclApi.Value;

			if ((memFlags & (int) CLEnum.MemCopyHostPtr) != 0)
				throw new Exception("Unable to use CL_MEM_COPY_HOST_PTR as memory flags within OpenCL buffer memory pool.");

			var bufferId = CreateBuffer<T>()
		}

		public void ReturnBuffer(nint bufferId)
		{
			throw new NotImplementedException();
		}
	}
}
