using System;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.OpenCL;

namespace Emphasis.OpenCL
{
	public static class OclHelper
	{
		internal static Lazy<CL> OclApi = new(CL.GetApi);
		
		public static unsafe int GetBuildLog(nint programId, nint deviceId, out string buildLog)
		{
			var api = OclApi.Value;

			var size = 2048;
			Span<byte> result = stackalloc byte[size];
			Span<nuint> resultSize = stackalloc nuint[1];
			var err = api.GetProgramBuildInfo(programId, deviceId, (uint)CLEnum.ProgramBuildLog,(uint) size, result, resultSize);

			if (err != (int)CLEnum.Success)
			{
				if (err == (int)CLEnum.InvalidValue && (int)resultSize[0] > size)
				{
					size = (int) resultSize[0];
					result = new byte[size];
					err = api.GetProgramBuildInfo(programId, deviceId, (uint)CLEnum.ProgramBuildLog, (uint)size, result, resultSize);

					if (err != (int)CLEnum.Success)
					{
						buildLog = "";
						return err;
					}
				}
				else
				{
					buildLog = "";
					return err;
				}
			}

			buildLog = Encoding.ASCII.GetString(result[..-1]);
			return err;
		}
	}
}
