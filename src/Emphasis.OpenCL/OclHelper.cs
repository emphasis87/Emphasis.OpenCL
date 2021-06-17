using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Silk.NET.OpenCL;

namespace Emphasis.OpenCL
{
	public static class OclHelper
	{
		internal static Lazy<CL> OclApi = new(CL.GetApi);

		internal static nuint Size<T>(int count) => (nuint)(Marshal.SizeOf<T>() * count);
		internal static nuint Size<T>(long count) => (nuint)(Marshal.SizeOf<T>() * count);

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
			return platformIds[..platformCount].ToArray();
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

				var result = Encoding.ASCII.GetString(name[..(nameCount - 1)]);
				return result;
			}

			if (PlatformNames.TryGetValue(platformId, out var nameLazy))
				return nameLazy.Value;

			nameLazy = PlatformNames.GetOrAdd(platformId, new Lazy<string>(GetName));
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

				var result = Encoding.ASCII.GetString(extensions[..(extensionsCount - 1)]);
				return result;
			}

			if (PlatformExtensions.TryGetValue(platformId, out var extensionsLazy))
				return extensionsLazy.Value;

			extensionsLazy = PlatformExtensions.GetOrAdd(platformId, new Lazy<string>(GetExtensions));
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

				var result = Encoding.ASCII.GetString(name[..(nameCount - 1)]);
				return result;
			}

			if (DeviceNames.TryGetValue(deviceId, out var nameLazy))
				return nameLazy.Value;

			nameLazy = DeviceNames.GetOrAdd(deviceId, new Lazy<string>(GetName));
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

				var result = Encoding.ASCII.GetString(extensions[..(extensionsCount - 1)]);
				return result;
			}

			if (DeviceExtensions.TryGetValue(deviceId, out var extensionsLazy))
				return extensionsLazy.Value;

			extensionsLazy = DeviceExtensions.GetOrAdd(deviceId, new Lazy<string>(GetExtensions));
			return extensionsLazy.Value;
		}

		private static readonly ConcurrentDictionary<nint, Lazy<nuint>> DeviceTypes = new();
		public static int GetDeviceType(nint deviceId)
		{
			var api = OclApi.Value;

			nuint GetDevType()
			{
				Span<nuint> deviceInfo = stackalloc nuint[1];
				Span<nuint> deviceInfoSize = stackalloc nuint[1];
				var errInfo = api.GetDeviceInfo(deviceId, (uint) CLEnum.DeviceType, Size<nuint>(1), deviceInfo, deviceInfoSize);
				if (errInfo != (int) CLEnum.Success)
					throw new Exception($"Unable to get device type (OpenCL: {errInfo}).");
				
				return deviceInfo[0];
			}

			if (DeviceTypes.TryGetValue(deviceId, out var deviceTypeLazy))
				return (int)deviceTypeLazy.Value;

			deviceTypeLazy = DeviceTypes.GetOrAdd(deviceId, new Lazy<nuint>(GetDevType));
			return (int) deviceTypeLazy.Value;
		}

		public static string GetDeviceTypeName(nint deviceId)
		{
			return GetDeviceTypeName(GetDeviceType(deviceId));
		}

		public static string GetDeviceTypeName(int deviceType)
		{
			return (CLEnum) deviceType switch
			{
				CLEnum.DeviceTypeDefault => "Default",
				CLEnum.DeviceTypeCpu => "Cpu",
				CLEnum.DeviceTypeGpu => "Gpu",
				CLEnum.DeviceTypeAccelerator => "Accelerator",
				CLEnum.DeviceTypeCustom => "Custom",
				_ => throw new ArgumentOutOfRangeException($"Unknown device type {deviceType}."),
			};
		}

		public static nint[] GetContextDevices(nint contextId)
		{
			return GetDevicesFromContext(contextId);
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

		public static nint[] GetPlatformDevices(nint platformId, int deviceType = 0)
		{
			return GetDevicesForPlatform(platformId, deviceType);
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
			return deviceIds[..deviceCount].ToArray();
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

		public static nint CreateKernel(nint programId, string name)
		{
			var api = OclApi.Value;
			var kernelId = api.CreateKernel(programId, name, out var errKernel);
			if (errKernel != (int) CLEnum.Success)
				throw new Exception($"Unable to create a kernel {name} (OpenCL: {errKernel}).");

			return kernelId;
		}

		public static uint GetKernelWorkGroupSize(nint kernelId, nint deviceId)
		{
			var api = OclApi.Value;
			Span<nuint> kernelInfo = stackalloc nuint[1];
			Span<nuint> kernelInfoSize = stackalloc nuint[1];
			var errInfo = api.GetKernelWorkGroupInfo(kernelId, deviceId, (uint)CLEnum.KernelWorkGroupSize, Size<nuint>(1), kernelInfo, kernelInfoSize);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get kernel work group size (OpenCL: {errInfo}).");

			return (uint) kernelInfo[0];
		}

		public static uint GetKernelPreferredWorkGroupSizeMultiple(nint kernelId, nint deviceId)
		{
			var api = OclApi.Value;
			Span<nuint> kernelInfo = stackalloc nuint[1];
			Span<nuint> kernelInfoSize = stackalloc nuint[1];
			var errInfo = api.GetKernelWorkGroupInfo(kernelId, deviceId, (uint)CLEnum.KernelPreferredWorkGroupSizeMultiple, Size<nuint>(1), kernelInfo, kernelInfoSize);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get kernel preferred work group size multiple (OpenCL: {errInfo}).");

			return (uint) kernelInfo[0];
		}

		public static void SetKernelArg<T>(nint kernelId, int index, T arg)
			where T : unmanaged
		{
			var api = OclApi.Value;
			var errArg = api.SetKernelArg(kernelId, (uint)index, Size<T>(1), arg);
			if (errArg != (int)CLEnum.Success)
				throw new Exception($"Unable to set a kernel argument {index} (OpenCL: {errArg}).");
		}

		public static void SetKernelArgSize<T>(nint kernelId, int index, int count)
			where T : unmanaged
		{
			var api = OclApi.Value;
			var errArg = api.SetKernelArg(kernelId, (uint)index, (uint)Size<T>(count), ReadOnlySpan<T>.Empty);
			if (errArg != (int)CLEnum.Success)
				throw new Exception($"Unable to set a kernel argument {index} (OpenCL: {errArg}).");
		}

		public static nint EnqueueNDRangeKernel(nint queueId, nint kernelId, 
			ReadOnlySpan<nuint> globalWorkSize,
			ReadOnlySpan<nuint> globalOffset= default,
			ReadOnlySpan<nuint> localWorkSize = default,
			ReadOnlySpan<nint> waitOnEvents = default
			)
		{
			var api = OclApi.Value;
			
			Span<nint> onCompletedEvents = stackalloc nint[1];
			var errEnqueue = api.EnqueueNdrangeKernel(queueId, kernelId,
				(uint) globalWorkSize.Length,
				globalOffset,
				globalWorkSize,
				localWorkSize,
				(uint)waitOnEvents.Length, 
				waitOnEvents, 
				onCompletedEvents);

			if (errEnqueue != (int)CLEnum.Success)
				throw new Exception($"Unable to enqueue a kernel (OpenCL: {errEnqueue}).");

			return onCompletedEvents[0];
		}

		public static nint CreateCommandQueue(nint contextId, nint deviceId, int properties = default)
		{
			var api = OclApi.Value;
			var queueId = api.CreateCommandQueue(contextId, deviceId, (CLEnum)properties, out var errQueue);
			if (errQueue != (int)CLEnum.Success)
				throw new Exception($"Unable to create a command queue (OpenCL: {errQueue}).");

			return queueId;
		}

		public static nint GetCommandQueueContext(nint queueId)
		{
			var api = OclApi.Value;

			Span<nuint> contextsCounts = stackalloc nuint[1];
			Span<nint> contextIds = stackalloc nint[1];
			var errInfo = api.GetCommandQueueInfo(queueId, (uint)CLEnum.QueueContext, Size<nint>(1), contextIds, contextsCounts);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get command queue context (OpenCL: {errInfo}).");

			var contextId = contextIds[0];
			return contextId;
		}

		public static nint GetCommandQueueDevice(nint queueId)
		{
			var api = OclApi.Value;

			Span<nuint> deviceCounts = stackalloc nuint[1];
			Span<nint> deviceIds = stackalloc nint[1];
			var errInfo = api.GetCommandQueueInfo(queueId, (uint)CLEnum.QueueDevice, Size<nint>(1), deviceIds, deviceCounts);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get command queue device (OpenCL: {errInfo}).");

			var deviceId = deviceIds[0];
			return deviceId;
		}

		public static void OnEventStatusChanged(nint eventId, int status, Action action)
		{
			var api = OclApi.Value;
			var errCallback = api.SetEventCallback(eventId, status, delegate { action?.Invoke(); }, Span<byte>.Empty);
			if (errCallback != (int)CLEnum.Success)
				throw new Exception($"Unable to set event callback (OpenCL: {errCallback}).");
		}

		public static void OnEventCompleted(nint eventId, Action action)
		{
			OnEventStatusChanged(eventId, (int) CLEnum.Complete, action);
		}

		public static int GetEventStatus(nint eventId)
		{
			var api = OclApi.Value;

			Span<nuint> infoCount = stackalloc nuint[1];
			Span<nint> info = stackalloc nint[1];
			var errInfo = api.GetEventInfo(eventId, (uint)CLEnum.EventCommandExecutionStatus, Size<nint>(1), info, infoCount);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get event command execution status (OpenCL: {errInfo}).");

			var status = (int)info[0];
			return status;
		}

		public static nint GetEventCommandQueue(nint eventId)
		{
			var api = OclApi.Value;

			Span<nuint> infoCount = stackalloc nuint[1];
			Span<nint> info = stackalloc nint[1];
			var errInfo = api.GetEventInfo(eventId, (uint)CLEnum.EventCommandQueue, Size<nint>(1), info, infoCount);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get event command queue (OpenCL: {errInfo}).");

			var queueId = info[0];
			return queueId;
		}

		public static nint GetEventContext(nint eventId)
		{
			var api = OclApi.Value;

			Span<nuint> infoCount = stackalloc nuint[1];
			Span<nint> info = stackalloc nint[1];
			var errInfo = api.GetEventInfo(eventId, (uint)CLEnum.EventContext, Size<nint>(1), info, infoCount);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get event context (OpenCL: {errInfo}).");

			var contextId = info[0];
			return contextId;
		}

		public static uint GetEventReferenceCount(nint eventId)
		{
			var api = OclApi.Value;

			Span<nuint> infoCount = stackalloc nuint[1];
			Span<nuint> info = stackalloc nuint[1];
			var errInfo = api.GetEventInfo(eventId, (uint)CLEnum.EventReferenceCount, Size<nuint>(1), info, infoCount);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get event reference count (OpenCL: {errInfo}).");

			var referenceCount = (uint)info[0];
			return referenceCount;
		}

		public static void Finish(nint queueId)
		{
			var api = OclApi.Value;
			var errFinish = api.Finish(queueId);
			if (errFinish != (int)CLEnum.Success)
				throw new Exception($"Unable to finish a command queue (OpenCL: {errFinish}).");
		}

		public static void Flush(nint queueId)
		{
			var api = OclApi.Value;

			var errFlush = api.Flush(queueId);
			if (errFlush != (int)CLEnum.Success)
				throw new Exception($"Unable to flush a command queue (OpenCL: {errFlush}).");
		}

		public static nint CopyBuffer<T>(nint contextId, Span<T> source, int flags = default)
			where T : unmanaged
		{
			var api = OclApi.Value;

			Span<int> errs = stackalloc int[1];
			var bufferId = api.CreateBuffer(contextId,  (CLEnum)flags | CLEnum.MemCopyHostPtr, Size<T>(source.Length), source, errs);
			var errBuffer = errs[0];
			if (errBuffer != (int) CLEnum.Success)
				throw new Exception($"Unable to create a buffer (OpenCL: {errBuffer}).");

			return bufferId;
		}

		public static nint CreateBuffer<T>(nint contextId, long size, int memoryFlags = default)
			where T : unmanaged
		{
			var api = OclApi.Value;
			
			Span<int> errs = stackalloc int[1];
			var bufferId = api.CreateBuffer(contextId, (CLEnum)memoryFlags, Size<T>(size), Span<T>.Empty, errs);
			var errBuffer = errs[0];
			if (errBuffer != (int)CLEnum.Success)
				throw new Exception($"Unable to create a buffer (OpenCL: {errBuffer}).");

			return bufferId;
		}

		public static nint EnqueueReadBuffer<T>(nint queueId, nint bufferId, bool blocking, int offset, int size, Span<T> destination, 
			ReadOnlySpan<nint> waitOnEvents = default)
			where T: unmanaged
		{
			var api = OclApi.Value;
			
			Span<nint> onCompletedEvents = stackalloc nint[1];
			var errRead = api.EnqueueReadBuffer(queueId, bufferId, blocking, Size<T>(offset), Size<T>(size), destination,
				(uint)waitOnEvents.Length,
				waitOnEvents, 
				onCompletedEvents);
			if (errRead != (int)CLEnum.Success)
				throw new Exception($"Unable to read a buffer (OpenCL: {errRead}).");

			return onCompletedEvents[0];
		}

		public static nint EnqueueWriteBuffer<T>(nint queueId, nint bufferId, bool blocking, int offset, int size, ReadOnlySpan<T> source, 
			ReadOnlySpan<nint> waitOnEvents = default)
			where T : unmanaged
		{
			var api = OclApi.Value;
			
			Span<nint> onCompletedEvents = stackalloc nint[1];
			var errWrite = api.EnqueueWriteBuffer(queueId, bufferId, blocking, Size<T>(offset), Size<T>(size), source,
				(uint)waitOnEvents.Length, 
				waitOnEvents, 
				onCompletedEvents);
			if (errWrite != (int)CLEnum.Success)
				throw new Exception($"Unable to write to a buffer (OpenCL: {errWrite}).");

			return onCompletedEvents[0];
		}

		public static int GetMemObjectMemoryFlags(nint memObjectId)
		{
			var api = OclApi.Value;

			Span<nuint> infoLength = stackalloc nuint[1];
			Span<nint> info = stackalloc nint[1];
			var errInfo = api.GetMemObjectInfo(memObjectId, (uint)CLEnum.MemFlags, Size<nint>(1), info, infoLength);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get memory object memory flags (OpenCL: {errInfo}).");

			return (int) info[0];
		}

		public static long GetMemObjectSize(nint memObjectId)
		{
			var api = OclApi.Value;

			Span<nuint> infoLength = stackalloc nuint[1];
			Span<nint> info = stackalloc nint[1];
			var errInfo = api.GetMemObjectInfo(memObjectId, (uint)CLEnum.MemSize, Size<nint>(1), info, infoLength);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get memory object size (OpenCL: {errInfo}).");

			return info[0];
		}

		public static nint GetMemObjectContext(nint memObjectId)
		{
			var api = OclApi.Value;

			Span<nuint> infoLength = stackalloc nuint[1];
			Span<nint> info = stackalloc nint[1];
			var errInfo = api.GetMemObjectInfo(memObjectId, (uint)CLEnum.MemContext, Size<nint>(1), info, infoLength);
			if (errInfo != (int)CLEnum.Success)
				throw new Exception($"Unable to get memory object context (OpenCL: {errInfo}).");

			return info[0];
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

			buildLog = Encoding.ASCII.GetString(result[..(resultCount - 1)]);
			return errInfo;
		}

		public static void ReleaseProgram(nint programId)
		{
			var api = OclApi.Value;
			var errRelease = api.ReleaseProgram(programId);
			if (errRelease != (int)CLEnum.Success)
				throw new Exception($"Unable to release program (OpenCL: {errRelease}).");
		}

		public static void ReleaseMemObject(nint memObjectId)
		{
			var api = OclApi.Value;
			var errRelease = api.ReleaseMemObject(memObjectId);
			if (errRelease != (int)CLEnum.Success)
				throw new Exception($"Unable to release memory object (OpenCL: {errRelease}).");
		}

		public static void ReleaseCommandQueue(nint queueId)
		{
			var api = OclApi.Value;
			var errRelease = api.ReleaseCommandQueue(queueId);
			if (errRelease != (int)CLEnum.Success)
				throw new Exception($"Unable to release command queue (OpenCL: {errRelease}).");
		}

		public static void ReleaseContext(nint contextId)
		{
			var api = OclApi.Value;
			var errRelease = api.ReleaseContext(contextId);
			if (errRelease != (int)CLEnum.Success)
				throw new Exception($"Unable to release context (OpenCL: {errRelease}).");
		}

		public static void ReleaseDevice(nint deviceId)
		{
			var api = OclApi.Value;
			var errRelease = api.ReleaseDevice(deviceId);
			if (errRelease != (int)CLEnum.Success)
				throw new Exception($"Unable to release device (OpenCL: {errRelease}).");
		}

		public static void ReleaseEvent(nint eventId)
		{
			var api = OclApi.Value;
			var errRelease = api.ReleaseEvent(eventId);
			if (errRelease != (int) CLEnum.Success)
				throw new Exception($"Unable to release event (OpenCL: {errRelease}).");
		}

		public static void ReleaseKernel(nint kernelId)
		{
			var api = OclApi.Value;
			var errRelease = api.ReleaseKernel(kernelId);
			if (errRelease != (int)CLEnum.Success)
				throw new Exception($"Unable to release kernel (OpenCL: {errRelease}).");
		}

		public static void ReleaseSampler(nint samplerId)
		{
			var api = OclApi.Value;
			var errRelease = api.ReleaseSampler(samplerId);
			if (errRelease != (int)CLEnum.Success)
				throw new Exception($"Unable to release sampler (OpenCL: {errRelease}).");
		}

		public static void RetainProgram(nint programId)
		{
			var api = OclApi.Value;
			var errRetain = api.RetainProgram(programId);
			if (errRetain != (int)CLEnum.Success)
				throw new Exception($"Unable to retain program (OpenCL: {errRetain}).");
		}

		public static void RetainMemObject(nint memObjectId)
		{
			var api = OclApi.Value;
			var errRetain = api.RetainMemObject(memObjectId);
			if (errRetain != (int)CLEnum.Success)
				throw new Exception($"Unable to retain memory object (OpenCL: {errRetain}).");
		}

		public static void RetainCommandQueue(nint queueId)
		{
			var api = OclApi.Value;
			var errRetain = api.RetainCommandQueue(queueId);
			if (errRetain != (int)CLEnum.Success)
				throw new Exception($"Unable to retain command queue (OpenCL: {errRetain}).");
		}

		public static void RetainContext(nint contextId)
		{
			var api = OclApi.Value;
			var errRetain = api.RetainContext(contextId);
			if (errRetain != (int)CLEnum.Success)
				throw new Exception($"Unable to retain context (OpenCL: {errRetain}).");
		}

		public static void RetainDevice(nint deviceId)
		{
			var api = OclApi.Value;
			var errRetain = api.RetainDevice(deviceId);
			if (errRetain != (int)CLEnum.Success)
				throw new Exception($"Unable to retain device (OpenCL: {errRetain}).");
		}

		public static void RetainEvent(nint eventId)
		{
			var api = OclApi.Value;
			var errRetain = api.RetainEvent(eventId);
			if (errRetain != (int)CLEnum.Success)
				throw new Exception($"Unable to retain event (OpenCL: {errRetain}).");
		}

		public static void RetainKernel(nint kernelId)
		{
			var api = OclApi.Value;
			var errRetain = api.RetainKernel(kernelId);
			if (errRetain != (int)CLEnum.Success)
				throw new Exception($"Unable to retain kernel (OpenCL: {errRetain}).");
		}

		public static void RetainSampler(nint samplerId)
		{
			var api = OclApi.Value;
			var errRetain = api.RetainSampler(samplerId);
			if (errRetain != (int)CLEnum.Success)
				throw new Exception($"Unable to retain sampler (OpenCL: {errRetain}).");
		}

		public static void AddContextDestructorCallback(nint contextId, Action action)
		{
			var api = OclApi.Value;
			var errCallback = api.SetContextDestructorCallback(contextId, delegate { action?.Invoke(); }, Span<byte>.Empty);
			if (errCallback != (int)CLEnum.Success)
				throw new Exception($"Unable to set context destructor callback (OpenCL: {errCallback}).");
		}

		public static void AddMemObjectDestructorCallback(nint memObjectId, Action action)
		{
			var api = OclApi.Value;
			var errCallback = api.SetMemObjectDestructorCallback(memObjectId, delegate { action?.Invoke(); }, Span<byte>.Empty);
			if (errCallback != (int) CLEnum.Success)
				throw new Exception($"Unable to set memory object destructor callback (OpenCL: {errCallback}).");
		}

		public static void AddProgramReleaseCallback(nint programId, Action action)
		{
			var api = OclApi.Value;
			var errCallback = api.SetProgramReleaseCallback(programId, delegate { action?.Invoke(); }, Span<byte>.Empty);
			if (errCallback != (int)CLEnum.Success)
				throw new Exception($"Unable to set program release callback (OpenCL: {errCallback}).");
		}

		public static void WaitForEvents(IEnumerable<nint> eventIds)
		{
			WaitForEvents(eventIds?.ToArray());
		}

		public static void WaitForEvents(params nint[] eventIds)
		{
			if (eventIds.Length == 0)
				return;

			var api = OclApi.Value;
			var errWait = api.WaitForEvents((uint) eventIds.Length, eventIds.AsSpan());
			if (errWait != (int) CLEnum.Success)
				throw new Exception($"Unable to wait for events (OpenCL: {errWait}).");
		}

		public static nint CreateUserEvent(nint contextId)
		{
			var api = OclApi.Value;
			var eventId = api.CreateUserEvent(contextId, out var errEvent);
			if (errEvent != (int) CLEnum.Success)
				throw new Exception($"Unable to create user event (OpenCL: {errEvent}).");
			
			return eventId;
		}

		public static void SetUserEventStatus(nint eventId, int status)
		{
			var api = OclApi.Value;
			var errEvent = api.SetUserEventStatus(eventId, status);
			if (errEvent != (int)CLEnum.Success)
				throw new Exception($"Unable to set user event execution status (OpenCL: {errEvent}).");
		}

		public static void SetUserEventCompleted(nint eventId)
		{
			SetUserEventStatus(eventId, (int) CLEnum.Complete);
		}
	}
}
