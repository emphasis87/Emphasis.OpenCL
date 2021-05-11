using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL.Tests
{
	public class OclProgramRepositoryTests
	{
		private static readonly Lazy<CL> OclApi = new(CL.GetApi);

		[Test]
		public async Task Test()
		{
			var api = OclApi.Value;

			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			var deviceId = GetDevicesFromContext(contextId).First();

			using var repository = new OclProgramRepository();

			var multiplyI32 = await repository.GetProgram(new OclProgram(contextId, deviceId, Kernels.multiply, "-DTDepth=int"));
			var multiplyI16 = await repository.GetProgram(new OclProgram(contextId, deviceId, Kernels.multiply, "-DTDepth=short"));

			var kernelM16 = CreateKernel(multiplyI16, "multiply");
			var kernelM32 = CreateKernel(multiplyI32, "multiply");

			var queueId = CreateCommandQueue(contextId, deviceId, (int) CLEnum.QueueOutOfOrderExecModeEnable);

			api.ReleaseKernel(kernelM16);
			api.ReleaseKernel(kernelM32);
			api.ReleaseCommandQueue(queueId);
			api.ReleaseContext(contextId);
		}
	}
}
