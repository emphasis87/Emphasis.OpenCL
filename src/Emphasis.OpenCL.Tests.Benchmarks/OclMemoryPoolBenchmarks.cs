using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL.Tests.Benchmarks
{
	[MarkdownExporter]
	[ShortRunJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclMemoryPoolBenchmarks_RentHit
	{
		private nint _platformId;
		private nint _contextId;
		private OclMemoryPool _poolRent;
		private OclMemoryPool _poolReturn;
		private nint _bufferId;

		[GlobalSetup]
		public void Setup()
		{
			_platformId = GetPlatforms().First();
			_contextId = CreateContext(_platformId);

			_poolRent = new OclMemoryPool();
			_poolReturn = new OclMemoryPool();
			
			_bufferId = CreateBuffer<int>(_contextId, 1024);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_poolRent.Dispose();
			_poolReturn.Dispose();

			ReleaseMemObject(_bufferId);
		}

		[Benchmark(Baseline = true)]
		public void Rent_miss()
		{
			var bufferId = _poolRent.RentBuffer<int>(_contextId, 1024);
			ReleaseMemObject(bufferId);
		}

		[Benchmark]
		public void Return_and_Rent()
		{
			_poolReturn.ReturnBuffer(_bufferId);
			_poolReturn.RentBuffer<int>(_contextId, 1024);
		}
	}
}
