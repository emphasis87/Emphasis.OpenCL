using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Emphasis.OpenCL.Tests.Benchmarks
{
	[MarkdownExporter]
	[ShortRunJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclMemoryPoolBenchmarks
	{
		[GlobalSetup]
		public void Setup()
		{
		
		}

		//[Benchmark]
		public void RentBuffer_miss()
		{
			
		}

		//[Benchmark]
		public void RentBuffer_hit()
		{

		}

		//[Benchmark]
		public void ReturnBuffer()
		{

		}

		//[Benchmark]
		public void TrimExcess()
		{

		}
	}
}
