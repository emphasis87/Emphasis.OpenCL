﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Emphasis.OpenCL.Tests.Benchmarks
{
	[MarkdownExporter]
	[ShortRunJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclHelperBenchmarks_WaitOnEvents
	{
		private nint _platformId;
		private nint _deviceId;
		private nint _contextId;
		private nint _queueId;
		private nint _programId;
		private nint _kernelId;
		private nint _eventId;

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

			var memA = OclHelper.CopyBuffer(_contextId, stackalloc int[5] { 1, 2, 3, 4, 5 });
			var memB = OclHelper.CreateBuffer<int>(_contextId, 5);

			OclHelper.SetKernelArg(_kernelId, 0, memA);
			OclHelper.SetKernelArg(_kernelId, 1, memB);
			OclHelper.SetKernelArg(_kernelId, 2, 2);
			
			_eventId = OclHelper.EnqueueNDRangeKernel(_queueId, _kernelId, globalWorkSize: stackalloc nuint[] { 5 });

			OclHelper.Finish(_queueId);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			OclHelper.ReleaseCommandQueue(_queueId);
			OclHelper.ReleaseContext(_contextId);
		}

		[Benchmark]
		public void WaitForEvents()
		{
			OclHelper.WaitForEvents(_eventId);
		}

		[Benchmark]
		public async Task WaitForEventsTask()
		{
			await Task.Run(() => OclHelper.WaitForEvents(_eventId));
		}
	}
}
