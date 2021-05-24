using BenchmarkDotNet.Running;

namespace Emphasis.OpenCL.Tests.Benchmarks
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
		}
	}
}
