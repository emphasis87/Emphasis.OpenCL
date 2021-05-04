using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenCL;

namespace Emphasis.OpenCL
{
	public static class OclHelper
	{
		internal static Lazy<CL> OclApi = new(CL.GetApi);

		internal static nuint Size<T>(int count) => (nuint)(Marshal.SizeOf<T>() * count);

		public static nint[] GetDeviceIds(nint contextId)
		{
			var api = OclApi.Value;

			Span<nuint> deviceCount = stackalloc nuint[1];
			var errDeviceNum = api.GetContextInfo(contextId, (uint) CLEnum.ContextNumDevices, Size<nuint>(1), deviceCount, Span<UIntPtr>.Empty);
			if (errDeviceNum != (int) CLEnum.Success)
				throw new Exception($"Unable to get context info {CLEnum.ContextNumDevices} (OpenCL: {errDeviceNum}).");

			var devCount = (int)deviceCount[0];
			Span<nint> deviceIds = stackalloc nint[devCount];
			var errDevices = api.GetContextInfo(contextId, (uint)CLEnum.ContextDevices, Size<nint>(devCount), deviceIds, Span<UIntPtr>.Empty);
			if (errDevices != (int)CLEnum.Success)
				throw new Exception($"Unable to get context info {CLEnum.ContextDevices} (OpenCL: {errDevices}).");

			return deviceIds.ToArray();
		}

		public static nint CreateProgram(nint contextId, string source)
		{
			var api = OclApi.Value;
			
			var lengths = (nuint)source.Length;
			var programId = api.CreateProgramWithSource(contextId, 1, new[] { source }, lengths, out var errProgram);
			if (errProgram != (int)CLEnum.Success)
				throw new Exception($"Unable to create a program with source (OpenCL: {errProgram}).");

			return programId;
		}

		public static async Task BuildProgram(nint programId, nint deviceId, string options = null)
		{
			var api = OclApi.Value;

			void Build()
			{
				Span<nint> deviceIds = stackalloc nint[] { deviceId };
				var errBuild = api.BuildProgram(programId, 1, deviceIds, options, null, Span<byte>.Empty);
				if (errBuild == (int) CLEnum.Success) 
					return;

				var errLog = TryGetBuildLog(programId, deviceId, out var buildLog);
				if (errLog != (int)CLEnum.Success)
					throw new Exception($"Build failed (OpenCL: {errBuild}). Unable to obtain build log (OpenCL: {errLog}).");

				throw new Exception($"Build failed (OpenCL: {errBuild}).\n{buildLog}");
			}

			await Task.Run(Build);
		}

		public static int TryGetBuildLog(nint programId, nint deviceId, out string buildLog)
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
