using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Emphasis.OpenCL.Tests.Benchmarks
{
	[MarkdownExporter]
	[ShortRunJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclHelperBenchmarks
	{
		private nint _platformId;
		private nint _deviceId;
		private nint _contextId;
		private nint _queueId;
		private nint _programId;
		private nint _kernelId;

		public nint[] PlatformIds;
		public nint[] DeviceIds;
		public nint DeviceId;
		public nint ContextId;
		public string Extensions;
		public uint WorkGroupSize;

		[GlobalSetup]
		public async Task Setup()
		{
			_platformId = OclHelper.GetPlatforms().First();
			_deviceId = OclHelper.GetDevicesForPlatform(_platformId).First();
			_contextId = OclHelper.CreateContext(_platformId, new[] {_deviceId});
			_queueId = OclHelper.CreateCommandQueue(_contextId, _deviceId);
			_programId = OclHelper.CreateProgram(_contextId, Kernels.multiply);
			
			await OclHelper.BuildProgram(_programId, _deviceId);
			_kernelId = OclHelper.CreateKernel(_programId, "multiply");
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			OclHelper.ReleaseContext(_contextId);
			OclHelper.ReleaseCommandQueue(_queueId);
		}

		[Benchmark]
		public void GetPlatforms()
		{
			PlatformIds = OclHelper.GetPlatforms();
		}

		[Benchmark]
		public void GetPlatformExtensions()
		{
			Extensions = OclHelper.GetPlatformExtensions(_platformId);
		}

		[Benchmark]
		public void GetDeviceExtensions()
		{
			Extensions = OclHelper.GetDeviceExtensions(_deviceId);
		}

		[Benchmark]
		public void GetDevicesForPlatform()
		{
			DeviceIds = OclHelper.GetDevicesForPlatform(_platformId);
		}

		[Benchmark]
		public void GetCommandQueueContext()
		{
			ContextId = OclHelper.GetCommandQueueContext(_queueId);
		}

		[Benchmark]
		public void GetCommandQueueDevice()
		{
			DeviceId = OclHelper.GetCommandQueueDevice(_queueId);
		}

		[Benchmark]
		public void GetKernelWorkGroupSize()
		{
			WorkGroupSize = OclHelper.GetKernelWorkGroupSize(_kernelId, _deviceId);
		}
	}
}
