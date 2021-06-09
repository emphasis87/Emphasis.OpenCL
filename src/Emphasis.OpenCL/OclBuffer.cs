namespace Emphasis.OpenCL
{
	public class OclBuffer : OclEntity
	{
		public OclBuffer(nint bufferId)
			: base(bufferId)
		{
			
		}

		public static implicit operator nint(OclBuffer buffer) => buffer.Id;

		public OclBuffer<T> Cast<T>() where T : unmanaged => new(Id);
	}

	public class OclBuffer<T> : OclBuffer
		where T : unmanaged
	{
		public OclBuffer(nint bufferId) : base(bufferId)
		{

		}
	}
}
