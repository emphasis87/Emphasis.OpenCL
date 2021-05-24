using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Emphasis.OpenCL.Tests
{
	public class OclMemoryPoolTests
	{
		[Test]
		public void Test()
		{
			var list = new SortedList<(int,long), string>();
			list.Add((1, DateTime.Now.Ticks), "a");
			list.Add((1, DateTime.Now.Ticks), "b");

			foreach (var v in list)
			{
				Console.WriteLine($"{v.Key} = {v.Value}");
			}
		}
	}
}
