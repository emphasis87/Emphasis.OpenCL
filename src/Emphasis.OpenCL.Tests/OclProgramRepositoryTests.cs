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

			var repository = new OclProgramRepository();

			var multiplyI32 = await repository.GetProgram(contextId, deviceId, Kernels.multiply, "-DTDepth=int");
			var multiplyI16 = await repository.GetProgram(contextId, deviceId, Kernels.multiply, "-DTDepth=short");

			api.CreateKernel(multiplyI32, "multiply", out var err);
		}
	}
}
