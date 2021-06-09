using System;

namespace Emphasis.OpenCL
{
	public class OclEntity
	{
		public nint NativeId { get; }

		public OclEntity(nint nativeId)
		{
			NativeId = nativeId;
		}

		internal static string GetNativeType<T>(T value = default)
			where T : unmanaged
		{
			return value switch
			{
				sbyte _ => "char",
				byte _ => "uchar",
				short _ => "short",
				ushort _ => "ushort",
				int _ => "int",
				uint _ => "uint",
				long _ => "long",
				ulong _ => "ulong",
				float _ => "float",
				double _ => "double",
				_ => throw new NotSupportedException($"The managed type {typeof(T)} is not supported.")
			};
		}
	}
}
