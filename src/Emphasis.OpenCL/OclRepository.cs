using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL
{
	public interface IOclRepository
	{

	}

	public class OclRepository : IOclRepository, IDisposable
	{
		

		public nint CreateProgram(nint platformId, string source)
		{
			var api = OclApi.Value;

			var propgram = api.CreateProgramWithSource();


			Span<nint> kernelIds = stackalloc nint[256];
				api.CreateKernelsInProgram(programId, )
		}

		public async Task<nint> BuildProgram(nint deviceId, nint programId, string options = null)
		{
			var api = OclApi.Value;

			int Build()
			{
				ReadOnlySpan<nint> deviceIds = stackalloc nint[1];
				return api.BuildProgram(programId, 1, deviceIds, options, null, Span<byte>.Empty);
			}

			var errBuild = await Task.Run(Build);
			if (errBuild != (int) CLEnum.Success)
			{
				var errLog = GetLog(programId, deviceId, out var buildLog);
				if (errLog != (int) CLEnum.Success)
					throw new Exception($"Build failed (OpenCL: {errBuild}). Unable to obtain build log (OpenCL: {errLog}).");

				throw new Exception($"Build failed (OpenCL: {errBuild}).");
			}
			
			

			return 
		}

		public async Task<nint> GetKernel(nint programId, string name)
		{
			var api = OclApi.Value;

			var kernelId = api.CreateKernel(programId, name, out var err);
			if (err != (int) CLEnum.Success)
				throw new Exception($"Unable to create kernel {name} (OpenCL: {err}).");


		}

		public void Dispose()
		{
			
		}
	}
}
