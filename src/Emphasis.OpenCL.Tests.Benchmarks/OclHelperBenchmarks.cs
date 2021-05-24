using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL.Tests.Benchmarks
{
	[MarkdownExporter]
	[ShortRunJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclHelperBenchmarks
	{
		private nint _platformId;
		private nint _deviceId;

		public nint[] _platformIds;
		public nint[] _deviceIds;
		public string _extensions;

		[GlobalSetup]
		public void Setup()
		{
			_platformId = OclHelper.GetPlatforms().First();
			_deviceId = OclHelper.GetDevicesForPlatform(_platformId).First();
		}

		[Benchmark]
		public void GetPlatforms()
		{
			_platformIds = OclHelper.GetPlatforms();
		}

		[Benchmark]
		public void GetPlatformExtensions()
		{
			_extensions = OclHelper.GetPlatformExtensions(_platformId);
		}

		[Benchmark]
		public void GetDeviceExtensions()
		{
			_extensions = OclHelper.GetDeviceExtensions(_deviceId);
		}

		[Benchmark]
		public void GetDevicesForPlatform()
		{
			_deviceIds = OclHelper.GetDevicesForPlatform(_platformId);
		}
	}
}
