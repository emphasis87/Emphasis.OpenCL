using System;
using System.Diagnostics;
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
					Console.WriteLine($"Device type: {GetDeviceTypeName(deviceId)}");
				}

				Console.WriteLine();
			}
		}

		[Test]
		public void ContextTest()
		{
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
					Console.WriteLine($"Device type: {GetDeviceTypeName(deviceId)}");
				}

				ReleaseContext(contextId);

				Console.WriteLine();
			}
		}

		[Test]
		public async Task ProgramTest()
		{
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
				
				var kernelId = CreateKernel(programId, "multiply");
				foreach (var deviceId in deviceIds)
				{
					Console.WriteLine($"Device name: {GetDeviceName(deviceId)}");

					var queueId = CreateCommandQueue(contextId, deviceId);

					Multiply(contextId, queueId, kernelId, deviceId);

					ReleaseKernel(kernelId);
					ReleaseCommandQueue(queueId);
				}

				ReleaseProgram(programId);
				ReleaseContext(contextId);

				Console.WriteLine();
			}

			void Multiply(nint contextId, nint queueId, nint kernelId, nint deviceId)
			{
				var memA = CopyBuffer(contextId, stackalloc int[5] { 1, 2, 3, 4, 5 });
				var memB = CreateBuffer<int>(contextId, 5);

				SetKernelArg(kernelId, 0, memA);
				SetKernelArg(kernelId, 1, memB);
				SetKernelArg(kernelId, 2, 2);
				
				EnqueueNDRangeKernel(queueId, kernelId, globalWorkSize: stackalloc nuint[] { 5 });

				Finish(queueId);

				Span<int> bufferB = stackalloc int[5];
				EnqueueReadBuffer(queueId, memB, true, 0, 5, bufferB);

				bufferB.ToArray().Should().Equal(2, 4, 6, 8, 10);

				Console.WriteLine(string.Join(", ", bufferB.ToArray()));

				ReleaseMemObject(memA);
				ReleaseMemObject(memB);
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

				ReleaseProgram(programId);
				ReleaseContext(contextId);

				Console.WriteLine();
			}
		}

		[Test]
		public void Can_add_same_program_multiple_times()
		{
			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			var programId1 = CreateProgram(contextId, Kernels.multiply);
			var programId2 = CreateProgram(contextId, Kernels.multiply);

			programId1.Should().NotBe(programId2);

			ReleaseProgram(programId1);
			ReleaseProgram(programId2);
			ReleaseContext(contextId);
		}

		[Test]
		public void Can_get_queue_info()
		{
			// Arrange:
			var platformId = GetPlatforms().First();
			var deviceId = GetDevicesForPlatform(platformId).First();
			var contextId = CreateContext(platformId);
			var queueId = CreateCommandQueue(contextId, deviceId);

			// Act:
			var contextId2 = GetCommandQueueContext(queueId);
			var deviceId2 = GetCommandQueueDevice(queueId);

			// Assert:
			contextId2.Should().Be(contextId);
			deviceId2.Should().Be(deviceId);

			ReleaseContext(contextId);
		}

		private int _contextDestructorCalled = 0;

		[Test]
		public void Can_set_context_destructor_callback()
		{
			// Arrange:
			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			
			// Act:
			// Requires OpenCL 3.0
			AddContextDestructorCallback(contextId, () => _contextDestructorCalled++);
			ReleaseContext(contextId);

			// Assert:
			_contextDestructorCalled.Should().Be(1);
		}

		[Test]
		public async Task Can_get_kernel_work_group_size()
		{
			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			var programId = CreateProgram(contextId, Kernels.multiply);

			var deviceIds = GetContextDevices(contextId);
			foreach (var deviceId in deviceIds)
			{
				await BuildProgram(programId, deviceId);
			}

			var kernelId = CreateKernel(programId, "multiply");

			foreach (var deviceId in deviceIds)
			{
				var size = GetKernelWorkGroupSize(kernelId, deviceId);
				var sizeMultiple = GetKernelPreferredWorkGroupSizeMultiple(kernelId, deviceId);
				Console.WriteLine($"{GetDeviceName(deviceId)}: Work group size: {size}");
				Console.WriteLine($"{GetDeviceName(deviceId)}: Preferred work group size multiple: {sizeMultiple}");
			}
		}

		[Test]
		public async Task Can_wait_for_events_async()
		{
			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			var eventId = CreateUserEvent(contextId);

			async Task Fire()
			{
				await Task.Delay(1000);
				SetUserEventCompleted(eventId);
			}

			var sw = new Stopwatch();
			sw.Start();

			_ = Task.Run(Fire);

			// Act:
			await WaitForEventsAsync(eventId);

			// Assert:
			sw.Stop();
			sw.ElapsedMilliseconds.Should().BeInRange(1000, 1100);
		}

		[Test]
		public void Can_copy_buffer()
		{
			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] {deviceId});
			var queueId = CreateCommandQueue(contextId, deviceId);

			var src = new int[1024];
			for (var i = 0; i < src.Length; i++)
			{
				src[i] = i;
			}

			// Act:
			var bufferId = CopyBuffer(contextId, src.AsSpan());

			// Assert:
			var dst = new int[src.Length];
			EnqueueReadBuffer(queueId, bufferId, true, 0, src.Length, dst.AsSpan());

			for (var i = 0; i < src.Length; i++)
			{
				src[i].Should().Be(dst[i]);
			}
		}
	}
}
