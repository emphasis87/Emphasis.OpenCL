using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL.Tests
{
	public class OclMemoryPoolTests
	{
		[Test]
		public void Create()
		{
			using var pool = new OclMemoryPool();
		}

		[Test]
		public void Can_Return()
		{
			using var pool = new OclMemoryPool();

			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			var bufferId = CreateBuffer<int>(contextId, 1024);

			// Act:
			pool.ReturnBuffer(bufferId);

			ReleaseMemObject(bufferId);
		}

		[Test]
		public void Can_Rent_miss()
		{
			using var pool = new OclMemoryPool();

			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			
			// Act:
			var bufferId = pool.RentBuffer<int>(contextId, 1024);
		
			ReleaseMemObject(bufferId);
		}

		[Test]
		public void Can_Rent_hit()
		{
			using var pool = new OclMemoryPool();

			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			var bufferId = CreateBuffer<int>(contextId, 1024);
			pool.ReturnBuffer(bufferId);

			// Act:
			var rentedId = pool.RentBuffer<int>(contextId, 1024);

			rentedId.Should().Be(bufferId);

			ReleaseMemObject(bufferId);
		}

		[Test]
		public void Can_TrimExcess()
		{
			using var pool = new OclMemoryPool();

			var platformId = GetPlatforms().First();
			var contextId = CreateContext(platformId);
			var bufferId = CreateBuffer<int>(contextId, 1024);
			pool.ReturnBuffer(bufferId);

			// Act:
			pool.TrimExcess(TimeSpan.Zero);
			var rentedId = pool.RentBuffer<int>(contextId, 1024);

			rentedId.Should().NotBe(bufferId);

			ReleaseMemObject(bufferId);
		}
	}
}
