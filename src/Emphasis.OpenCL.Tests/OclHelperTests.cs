using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL.Tests
{
	public class OclHelperTests
	{
		internal static nuint Size<T>(int count) => (nuint) (Marshal.SizeOf<T>() * count);
		private static readonly Lazy<CL> OclApi = new(CL.GetApi);

		[Test]
		public void PlatformsTest()
		{
			var platformIds = GetPlatforms();
			foreach (var platformId in platformIds)
			{
				Console.WriteLine($"Platform name: {GetPlatformName(platformId)}");
				Console.WriteLine($"Platform extensions: {GetPlatformExtensions(platformId)}");

				var deviceIds = GetDevicesForPlatform(platformId);
				foreach (var deviceId in deviceIds)
				{
					Console.WriteLine($"Device name: {GetDeviceName(deviceId)}");
					Console.WriteLine($"Device extensions: {GetDeviceExtensions(deviceId)}");
				}

				Console.WriteLine();
			}
		}

		[Test]
		public void ContextTest()
		{
			var api = OclApi.Value;

			var platformIds = GetPlatforms();
			foreach (var platformId in platformIds)
			{
				Console.WriteLine($"Platform name: {GetPlatformName(platformId)}");
				Console.WriteLine($"Platform extensions: {GetPlatformExtensions(platformId)}");

				var contextId = CreateContext(platformId);
				var deviceIds = GetDevicesFromContext(contextId);
				foreach (var deviceId in deviceIds)
				{
					Console.WriteLine($"Device name: {GetDeviceName(deviceId)}");
					Console.WriteLine($"Device extensions: {GetDeviceExtensions(deviceId)}");
				}

				api.ReleaseContext(contextId);

				Console.WriteLine();
			}
		}

		[Test]
		public async Task ProgramTest()
		{
			var api = OclApi.Value;

			var platformIds = GetPlatforms();
			foreach (var platformId in platformIds)
			{
				Console.WriteLine($"Platform name: {GetPlatformName(platformId)}");

				var contextId = CreateContext(platformId);
				var programId = CreateProgram(contextId, Kernels.multiply);

				var deviceIds = GetDevicesFromContext(contextId);
				foreach (var deviceId in deviceIds)
				{
					await BuildProgram(programId, deviceId);
				}

				var kernelId = api.CreateKernel(programId, "multiply", out var err);
				if (err != (int) CLEnum.Success)
					throw new Exception("Unable to create a kernel.");

				foreach (var deviceId in deviceIds)
				{
					Console.WriteLine($"Device name: {GetDeviceName(deviceId)}");

					var queueId = api.CreateCommandQueue(contextId, deviceId, default, out var errQueue);
					if (errQueue != (int) CLEnum.Success)
						throw new Exception("Unable to create command queue.");

					Multiply(contextId, queueId, kernelId, deviceId);

					api.ReleaseKernel(kernelId);
					api.ReleaseCommandQueue(queueId);
				}

				api.ReleaseProgram(programId);
				api.ReleaseContext(contextId);

				Console.WriteLine();
			}

			void Multiply(nint contextId, nint queueId, nint kernelId, nint deviceId)
			{
				Span<int> bufferA = stackalloc int[5] {1, 2, 3, 4, 5};
				Span<int> bufferB = stackalloc int[5];
				Span<int> errs = stackalloc int[1];
				var a = api.CreateBuffer(contextId, CLEnum.MemCopyHostPtr, Size<int>(5), bufferA, errs);
				if (errs[0] != (int) CLEnum.Success)
					throw new Exception("Unable to create a buffer.");

				var b = api.CreateBuffer(contextId, CLEnum.MemHostReadOnly, Size<int>(5), Span<int>.Empty, errs);
				if (errs[0] != (int) CLEnum.Success)
					throw new Exception("Unable to create a buffer.");

				var errArg0 = api.SetKernelArg(kernelId, 0, Size<nint>(1), a);
				var errArg1 = api.SetKernelArg(kernelId, 1, Size<nint>(1), b);
				var errArg2 = api.SetKernelArg(kernelId, 2, Size<int>(1), 2);
				if (errArg0 != (int) CLEnum.Success)
					throw new Exception($"Unable to set a kernel argument (OpenCL: {errArg0}).");
				if (errArg1 != (int) CLEnum.Success)
					throw new Exception($"Unable to set a kernel argument (OpenCL: {errArg1}).");
				if (errArg2 != (int) CLEnum.Success)
					throw new Exception($"Unable to set a kernel argument (OpenCL: {errArg2}).");

				Span<nuint> globalOffset = stackalloc nuint[] {0};
				Span<nuint> globalDim = stackalloc nuint[] {5};
				Span<nint> events = stackalloc nint[1];
				var errEnqueue = api.EnqueueNdrangeKernel(queueId, kernelId, 1, globalOffset, globalDim,
					Span<nuint>.Empty, 0, Span<nint>.Empty, events);
				if (errEnqueue != (int) CLEnum.Success)
					throw new Exception("Unable to enqueue kernel.");

				var errFinish = api.Finish(queueId);
				if (errFinish != (int) CLEnum.Success)
					throw new Exception("Unable to finish a command queue.");

				var errRead = api.EnqueueReadBuffer(queueId, b, true, 0, Size<int>(5), bufferB, 0, Span<nint>.Empty,
					events);
				if (errRead != (int) CLEnum.Success)
					throw new Exception("Unable to read a buffer.");

				bufferB.ToArray().Should().Equal(2, 4, 6, 8, 10);

				Console.WriteLine(string.Join(", ", bufferB.ToArray()));

				api.ReleaseMemObject(a);
				api.ReleaseMemObject(b);
			}
		}

		[Test]
		public async Task BuildTest_on_error_should_throw_build_log()
		{
			var api = OclApi.Value;

			var platformIds = GetPlatforms();
			foreach (var platformId in platformIds)
			{
				Console.WriteLine($"Platform name: {GetPlatformName(platformId)}");

				var contextId = CreateContext(platformId);
				var programId = CreateProgram(contextId, "error");

				var deviceIds = GetDevicesFromContext(contextId);
				foreach (var deviceId in deviceIds)
				{
					Exception exception = null;
					try
					{ 
						await BuildProgram(programId, deviceId);
					}
					catch (Exception ex)
					{
						exception = ex;
					}

					exception.Should().NotBeNull();

					Console.WriteLine(exception.ToString());
				}

				api.ReleaseProgram(programId);
				api.ReleaseContext(contextId);

				Console.WriteLine();
			}
		}

		[Test]
		public void Can_add_same_program_multiple_times()
		{
			var api = OclApi.Value;

			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			var programId1 = CreateProgram(contextId, Kernels.multiply);
			var programId2 = CreateProgram(contextId, Kernels.multiply);

			programId1.Should().NotBe(programId2);

			api.ReleaseProgram(programId1);
			api.ReleaseProgram(programId2);
			api.ReleaseContext(contextId);
		}
	}
}
