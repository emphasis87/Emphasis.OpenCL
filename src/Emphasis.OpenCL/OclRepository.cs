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


		Span<nint> CreateKernels(nint programId, out Span<nint> r)
		{
			var api = OclApi.Value;

			Span<nint> kernelIds = stackalloc nint[256];
			Span<uint> kernelCount = stackalloc uint[1];
			var errKernel = api.CreateKernelsInProgram(programId, 256, kernelIds, kernelCount);
			if (errKernel != (int)CLEnum.Success)
			{
				if (errKernel == (int)CLEnum.InvalidValue && kernelCount[0] > 256)
				{
					errKernel = api.CreateKernelsInProgram(programId, 256, kernelIds, kernelCount);
					if (errKernel != (int)CLEnum.Success)
					{
						throw new Exception($"Unable to create kernels in program (OpenCL: {errKernel}).");
					}
				}
				else
				{
					throw new Exception($"Unable to create kernels in program (OpenCL: {errKernel}).");
				}
			}

			r = kernelIds;
			return kernelIds;
		}

		public unsafe nint CreateProgram(nint contextId, string source)
		{
			var api = OclApi.Value;

			var lengths = stackalloc nuint[] {(nuint)source.Length};
			var programId = api.CreateProgramWithSource(contextId, 1, new[] {source}, lengths, out var errProgram);
			if (errProgram != (int) CLEnum.Success)
				throw new Exception($"Unable to create a program with source (OpenCL: {errProgram}).");

			

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
				var errLog = GetBuildLog(programId, deviceId, out var buildLog);
				if (errLog != (int) CLEnum.Success)
					throw new Exception($"Build failed (OpenCL: {errBuild}). Unable to obtain build log (OpenCL: {errLog}).");

				throw new Exception($"Build failed (OpenCL: {errBuild}).\n{buildLog}");
			}

			return errBuild;
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
