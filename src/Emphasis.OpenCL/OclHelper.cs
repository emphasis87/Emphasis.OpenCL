using System;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.OpenCL;

namespace Emphasis.OpenCL
{
	public static class OclHelper
	{
		internal static Lazy<CL> OclApi = new(CL.GetApi);

		//public static unsafe int GetListInfo<T>()

		public delegate int HandleList<T>(Span<T> span);

		public static unsafe Span<T> GetListInfo<T>(HandleList<T> handler) where T:struct
		{
			var api = OclApi.Value;

			const int logSize = 2048;
			Span<T> smallLog = stackalloc T[logSize];
			ReadOnlySpan<byte> log = smallLog;
			Span<nuint> smallLogSize = stackalloc nuint[1];
			var err = handler(smallLog);
			if (err != (int)CLEnum.Success)
			{
				if (err == (int)CLEnum.InvalidValue && (int)smallLogSize[0] <= logSize)
				{
					var bigLogSize = smallLogSize[0];
					var bigLog = new byte[bigLogSize];
					var handle = GCHandle.Alloc(bigLog, GCHandleType.Pinned);
					try
					{
						err = api.GetProgramBuildInfo(programId, deviceId, (uint)CLEnum.ProgramBuildLog, bigLogSize, (void*)handle.AddrOfPinnedObject(), out _);
					}
					finally
					{
						handle.Free();
					}

					if (err != (int)CLEnum.Success)
					{
						buildLog = "";
						return err;
					}

					log = bigLog.AsSpan();
				}
			}
		}

		public static unsafe int GetLog(nint programId, nint deviceId, out string buildLog)
		{
			var api = OclApi.Value;

			const int logSize = 2048;
			Span<byte> smallLog = stackalloc byte[logSize];
			ReadOnlySpan<byte> log = smallLog;
			Span<nuint> smallLogSize = stackalloc nuint[1];
			var err = api.GetProgramBuildInfo(programId, deviceId, (uint)CLEnum.ProgramBuildLog, 2048, smallLog, smallLogSize);
			if (err != (int) CLEnum.Success)
			{
				if (err == (int) CLEnum.InvalidValue && (int) smallLogSize[0] <= logSize)
				{
					var bigLogSize = smallLogSize[0];
					var bigLog = new byte[bigLogSize];
					var handle = GCHandle.Alloc(bigLog, GCHandleType.Pinned);
					try
					{
						err = api.GetProgramBuildInfo(programId, deviceId, (uint) CLEnum.ProgramBuildLog, bigLogSize, (void*) handle.AddrOfPinnedObject(), out _);
					}
					finally
					{
						handle.Free();
					}

					if (err != (int) CLEnum.Success)
					{
						buildLog = "";
						return err;
					}

					log = bigLog.AsSpan();
				}
			}

			buildLog = Encoding.ASCII.GetString(log[..-1]);
			return err;
		}
	}
}
