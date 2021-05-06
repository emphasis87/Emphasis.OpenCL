using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL.Tests
{
	public class OclHelperTests
	{
		[Test]
		public void PlatformsTest()
		{
			var platformIds = GetPlatforms();
			foreach (var platformId in platformIds)
			{
				Console.WriteLine($"Platform name: {GetPlatformName(platformId)}");
				Console.WriteLine($"Platform extensions: {GetPlatformExtensions(platformId)}");

				var deviceIds = GetDevicesForPlatform(platformId);
				foreach (var deviceId in deviceIds)
				{
					Console.WriteLine($"Device name: {GetDeviceName(deviceId)}");
					Console.WriteLine($"Device extensions: {GetDeviceExtensions(deviceId)}");
				}
			}
		}

		[Test]
		public void ContextTest()
		{
			var platformIds = GetPlatforms();
			foreach (var platformId in platformIds)
			{
				Console.WriteLine($"Platform name: {GetPlatformName(platformId)}");
				Console.WriteLine($"Platform extensions: {GetPlatformExtensions(platformId)}");

				var contextId = CreateContext(platformId);
				var deviceIds = GetDevicesFromContext(contextId);
				foreach (var deviceId in deviceIds)
				{
					Console.WriteLine($"Device name: {GetDeviceName(deviceId)}");
					Console.WriteLine($"Device extensions: {GetDeviceExtensions(deviceId)}");
				}
			}
		}
	}
}
