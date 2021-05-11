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

			var repository = new OclProgramRepository();
			var multiply = repository.AddSource(Kernels.multiply);
			var optInt32 = repository.AddOptions("-DTDepth=int");
			var optInt16 = repository.AddOptions("-DTDepth=short");

			var multiplyInt32 = await repository.AddProgram(contextId, multiply, optInt32);
		}
	}
}
