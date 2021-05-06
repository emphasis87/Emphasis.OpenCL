using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenCL;

namespace Emphasis.OpenCL
{
	public static class OclHelper
	{
		internal static Lazy<CL> OclApi = new(CL.GetApi);

		internal static nuint Size<T>(int count) => (nuint)(Marshal.SizeOf<T>() * count);

		public static nint[] GetPlatforms()
		{
			var api = OclApi.Value;

			Span<nint> platformIds = stackalloc nint[32];
			Span<uint> platformCounts = stackalloc uint[1];
			var errPlatforms = api.GetPlatformIDs(32, platformIds, platformCounts);
			if (errPlatforms != (int)CLEnum.Success)
				throw new Exception($"Unable to get platform ids (OpenCL: {errPlatforms}).");

			var platformCount = (int)platformCounts[0];
			if (platformCount > 32)
			{
				platformIds = platformCount < 1024 ? stackalloc nint[platformCount] : new nint[platformCount];
				errPlatforms = api.GetPlatformIDs(32, platformIds, platformCounts);
				if (errPlatforms != (int)CLEnum.Success)
					throw new Exception($"Unable to get platform ids (OpenCL: {errPlatforms}).");
			}
			platformCount = (int)platformCounts[0];
			return platformIds.Slice(0, platformCount).ToArray();
		}

		private static readonly ConcurrentDictionary<nint, Lazy<string>> PlatformNames = new();
		public static string GetPlatformName(nint platformId)
		{
			var api = OclApi.Value;

			string GetName()
			{
				Span<nuint> nameCounts = stackalloc nuint[1];
				Span<byte> name = stackalloc byte[2048];
				var errInfo = api.GetPlatformInfo(platformId, (uint) CLEnum.PlatformName, 2048, name, nameCounts);
				if (errInfo != 0)
					throw new Exception($"Unable to get platform name (OpenCL: {errInfo}).");

				var nameCount = (int) nameCounts[0];
				if (nameCount > 2048)
				{
					name = new byte[nameCount];
					errInfo = api.GetPlatformInfo(platformId, (uint) CLEnum.PlatformName, (nuint) nameCount, name, nameCounts);
					if (errInfo != 0)
						throw new Exception($"Unable to get platform name (OpenCL: {errInfo}).");
				}

				var result = Encoding.ASCII.GetString(name.Slice(0, nameCount - 1));
				return result;
			}

			var nameLazy = PlatformNames.GetOrAdd(platformId, new Lazy<string>(GetName));
			return nameLazy.Value;
		}

		private static readonly ConcurrentDictionary<nint, Lazy<string>> PlatformExtensions = new();
		public static string GetPlatformExtensions(nint platformId)
		{
			var api = OclApi.Value;

			string GetExtensions()
			{
				Span<nuint> extensionCounts = stackalloc nuint[1];
				Span<byte> extensions = stackalloc byte[2048];
				var errInfo = api.GetPlatformInfo(platformId, (uint) CLEnum.PlatformExtensions, 2048, extensions, extensionCounts);
				if (errInfo != 0)
					throw new Exception($"Unable to get platform extensions (OpenCL: {errInfo}).");

				var extensionsCount = (int) extensionCounts[0];
				if (extensionsCount > 2048)
				{
					extensions = new byte[extensionsCount];
					errInfo = api.GetPlatformInfo(platformId, (uint) CLEnum.PlatformExtensions, (nuint) extensionsCount, extensions, extensionCounts);
					if (errInfo != 0)
						throw new Exception($"Unable to get platform extensions (OpenCL: {errInfo}).");
				}

				var result = Encoding.ASCII.GetString(extensions.Slice(0, extensionsCount - 1));
				return result;
			}

			var extensionsLazy = PlatformExtensions.GetOrAdd(platformId, new Lazy<string>(GetExtensions));
			return extensionsLazy.Value;
		}

		private static readonly ConcurrentDictionary<nint, Lazy<string>> DeviceNames = new();
		public static string GetDeviceName(nint deviceId)
		{
			var api = OclApi.Value;

			string GetName()
			{
				Span<nuint> nameCounts = stackalloc nuint[1];
				Span<byte> name = stackalloc byte[2048];
				var errInfo = api.GetDeviceInfo(deviceId, (uint) CLEnum.DeviceName, 2048, name, nameCounts);
				if (errInfo != 0)
					throw new Exception($"Unable to get device name (OpenCL: {errInfo}).");

				var nameCount = (int) nameCounts[0];
				if (nameCount > 2048)
				{
					name = new byte[nameCount];
					errInfo = api.GetDeviceInfo(deviceId, (uint) CLEnum.DeviceName, (nuint) nameCount, name, nameCounts);
					if (errInfo != 0)
						throw new Exception($"Unable to get device name (OpenCL: {errInfo}).");
				}

				var result = Encoding.ASCII.GetString(name.Slice(0, nameCount - 1));
				return result;
			}

			var nameLazy = DeviceNames.GetOrAdd(deviceId, new Lazy<string>(GetName));
			return nameLazy.Value;
		}

		private static readonly ConcurrentDictionary<nint, Lazy<string>> DeviceExtensions = new();
		public static string GetDeviceExtensions(nint deviceId)
		{
			var api = OclApi.Value;

			string GetExtensions()
			{
				Span<nuint> extensionCounts = stackalloc nuint[1];
				Span<byte> extensions = stackalloc byte[2048];
				var errInfo = api.GetDeviceInfo(deviceId, (uint) CLEnum.DeviceExtensions, 2048, extensions, extensionCounts);
				if (errInfo != 0)
					throw new Exception($"Unable to get device extensions (OpenCL: {errInfo}).");

				var extensionsCount = (int) extensionCounts[0];
				if (extensionsCount > 2048)
				{
					extensions = new byte[extensionsCount];
					errInfo = api.GetDeviceInfo(deviceId, (uint) CLEnum.DeviceExtensions, (nuint) extensionsCount, extensions, extensionCounts);
					if (errInfo != 0)
						throw new Exception($"Unable to get device extensions (OpenCL: {errInfo}).");
				}

				var result = Encoding.ASCII.GetString(extensions.Slice(0, extensionsCount - 1));
				return result;
			}

			var extensionsLazy = DeviceExtensions.GetOrAdd(deviceId, new Lazy<string>(GetExtensions));
			return extensionsLazy.Value;
		}
		
		public static nint[] GetDevicesFromContext(nint contextId)
		{
			var api = OclApi.Value;

			Span<nuint> deviceCount = stackalloc nuint[1];
			var errDeviceNum = api.GetContextInfo(contextId, (uint) CLEnum.ContextNumDevices, Size<nuint>(1), deviceCount, Span<UIntPtr>.Empty);
			if (errDeviceNum != (int) CLEnum.Success)
				throw new Exception($"Unable to get context info {CLEnum.ContextNumDevices} (OpenCL: {errDeviceNum}).");

			var devCount = (int)deviceCount[0];
			Span<nint> deviceIds = stackalloc nint[devCount];
			var errDevices = api.GetContextInfo(contextId, (uint)CLEnum.ContextDevices, Size<nint>(devCount), deviceIds, Span<UIntPtr>.Empty);
			if (errDevices != (int)CLEnum.Success)
				throw new Exception($"Unable to get context info {CLEnum.ContextDevices} (OpenCL: {errDevices}).");

			return deviceIds.ToArray();
		}

		public static nint[] GetDevicesForPlatform(nint platformId, int deviceType = 0)
		{
			var api = OclApi.Value;

			if (deviceType <= 0)
				deviceType = (int) CLEnum.DeviceTypeAll;

			Span<nint> deviceIds = stackalloc nint[32];
			Span<uint> deviceCounts = stackalloc uint[1];
			var errDevices = api.GetDeviceIDs(platformId, (CLEnum) deviceType, 32, deviceIds, deviceCounts);
			if (errDevices != (int)CLEnum.Success)
				throw new Exception($"Unable to get devices for platform (OpenCL: {errDevices}).");

			var deviceCount = (int)deviceCounts[0];
			if (deviceCount > 32)
			{
				deviceIds = deviceCount < 1024 ? stackalloc nint[deviceCount] : new nint[deviceCount];
				errDevices = api.GetDeviceIDs(platformId, (CLEnum)deviceType, (uint) deviceCount, deviceIds, deviceCounts);
				if (errDevices != (int)CLEnum.Success)
					throw new Exception($"Unable to get platform ids (OpenCL: {errDevices}).");
			}
			deviceCount = (int)deviceCounts[0];
			return deviceIds.Slice(0, deviceCount).ToArray();
		}
		
		public static nint CreateContext(nint platformId, nint[] deviceIds = null, nint[] properties = null, int deviceType = 0)
		{
			var api = OclApi.Value;

			deviceIds ??= GetDevicesForPlatform(platformId, deviceType);

			Span<int> errs = stackalloc int[1];
			Span<nint> props = properties ?? stackalloc nint[] { (nint)CLEnum.ContextPlatform, platformId, 0 };
			var contextId = api.CreateContext(props, (uint)deviceIds.Length, deviceIds.AsSpan(), null, Span<byte>.Empty, errs);
			var errContext = errs[0];
			if (errContext != (int)CLEnum.Success)
				throw new Exception($"Unable to create context (OpenCL: {errContext}).");

			return contextId;
		}

		public static nint CreateProgram(nint contextId, string source)
		{
			var api = OclApi.Value;
			
			var lengths = (nuint)source.Length;
			var programId = api.CreateProgramWithSource(contextId, 1, new[] { source }, lengths, out var errProgram);
			if (errProgram != (int)CLEnum.Success)
				throw new Exception($"Unable to create a program with source (OpenCL: {errProgram}).");

			return programId;
		}

		public static nint GetContextForProgram(nint programId)
		{
			var api = OclApi.Value;

			Span<nuint> contextsCounts = stackalloc nuint[1];
			Span<nint> contextIds = stackalloc nint[1];
			var errInfo = api.GetProgramInfo(programId, (uint) CLEnum.ProgramContext, Size<nint>(1), contextIds, contextsCounts);
			if (errInfo != (int) CLEnum.Success)
				throw new Exception($"Unable to get program context (OpenCL: {errInfo}).");

			var contextId = contextIds[0];
			return contextId;
		}
		
		public static async Task BuildProgram(nint programId, nint deviceId, string options = null)
		{
			var api = OclApi.Value;

			void Build()
			{
				Span<nint> deviceIds = stackalloc nint[1] {deviceId};
				var errBuild = api.BuildProgram(programId, 1, deviceIds, options, null, Span<byte>.Empty);
				if (errBuild == (int) CLEnum.Success) 
					return;

				var errLog = TryGetBuildLog(programId, deviceId, out var buildLog);
				if (errLog != (int)CLEnum.Success)
					throw new Exception($"Build failed (OpenCL: {errBuild}). Unable to obtain build log (OpenCL: {errLog}).");

				throw new Exception($"Build failed (OpenCL: {errBuild}).\n{buildLog}");
			}

			await Task.Run(Build);
		}

		public static int TryGetBuildLog(nint programId, nint deviceId, out string buildLog)
		{
			var api = OclApi.Value;
			
			Span<nuint> resultCounts = stackalloc nuint[1];
			Span<byte> result = stackalloc byte[2048];
			var errInfo = api.GetProgramBuildInfo(programId, deviceId, (uint)CLEnum.ProgramBuildLog,2048, result, resultCounts);
			var resultCount = (int)resultCounts[0];
			if (errInfo != (int)CLEnum.Success)
			{
				if (errInfo == (int)CLEnum.InvalidValue && resultCount > 2048)
				{
					result = new byte[resultCount];
					errInfo = api.GetProgramBuildInfo(programId, deviceId, (uint)CLEnum.ProgramBuildLog, (uint)resultCount, result, resultCounts);
					resultCount = (int)resultCounts[0];
					if (errInfo != (int)CLEnum.Success)
					{
						buildLog = "";
						return errInfo;
					}
				}
				else
				{
					buildLog = "";
					return errInfo;
				}
			}

			buildLog = Encoding.ASCII.GetString(result.Slice(0, resultCount - 1));
			return errInfo;
		}
	}
}
