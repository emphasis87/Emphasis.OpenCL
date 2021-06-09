namespace Emphasis.OpenCL
{
	public class OclBuffer : OclEntity
	{
		public OclBuffer(nint bufferId)
			: base(bufferId)
		{
			
		}

		public static implicit operator nint(OclBuffer buffer) => buffer.NativeId;

		public OclBuffer<T> Cast<T>() where T : unmanaged => new(NativeId);
	}

	public class OclTypedBuffer : OclBuffer
	{
		public string NativeType { get; }

		public OclTypedBuffer(nint bufferNativeId, string nativeType) 
			: base(bufferNativeId)
		{
			NativeType = nativeType;
		}
	}

	public class OclBuffer<T> : OclTypedBuffer
		where T : unmanaged
	{
		public OclBuffer(nint bufferId) 
			: base(bufferId, GetNativeType<T>())
		{
			
		}
	}
}
