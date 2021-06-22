using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Emphasis.OpenCL.Tests.Benchmarks
{
	[MarkdownExporter]
	[ShortRunJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclHelperBenchmarks_EnqueueFillBuffer
	{
		private nint _platformId;
		private nint _deviceId;
		private nint _contextId;
		private nint _queueId;
		private nint _bufferId;
		private int[] _pattern;

		[GlobalSetup]
		public void Setup()
		{
			_platformId = OclHelper.GetPlatforms().First();
			_deviceId = OclHelper.GetDevicesForPlatform(_platformId).First();
			_contextId = OclHelper.CreateContext(_platformId, new[] {_deviceId});
			_queueId = OclHelper.CreateCommandQueue(_contextId, _deviceId);
			_bufferId = OclHelper.CreateBuffer<int>(_contextId, 1200 * 1920);
			_pattern = new int[1] {13};
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			OclHelper.ReleaseMemObject(_bufferId);
			OclHelper.ReleaseCommandQueue(_queueId);
			OclHelper.ReleaseContext(_contextId);
		}
		
		[Benchmark]
		public void EnqueueFillBuffer()
		{
			var fillEventId = OclHelper.EnqueueFillBuffer<int>(_queueId, _bufferId, _pattern.AsSpan());

			OclHelper.Finish(_queueId);
			OclHelper.ReleaseEvent(fillEventId);
		}
	}
}
