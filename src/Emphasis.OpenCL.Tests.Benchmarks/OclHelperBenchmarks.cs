using System.Linq;
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

		public nint[] PlatformIds;
		public nint[] DeviceIds;
		public string Extensions;

		[GlobalSetup]
		public void Setup()
		{
			_platformId = OclHelper.GetPlatforms().First();
			_deviceId = OclHelper.GetDevicesForPlatform(_platformId).First();
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
	}
}
