using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL
{
	public interface IOclRepository
	{

	}

	public class OclRepository : IOclRepository, ICancelable
	{
		//private readonly ConcurrentDictionary<(nint contextId, nint deviceId, nint programId), bool> _programs = new();
		private readonly ConcurrentDictionary<string, Lazy<nint>> _programBySource = new();

		private readonly ConcurrentBag<(string[] source, string options)> _pendingPrograms = new();
		private readonly ConcurrentBag<nint> _pendingContextIds = new();
		private readonly ConcurrentBag<(nint contextId, nint programId)> _programs;

		private readonly HashSet<nint> ContextIds = new();
		private readonly HashSet<string> Sources = new();

		public void AddContexts(nint[] contextIds)
		{
			foreach (var contextId in contextIds)
				_pendingContextIds.Add(contextId);
		}

		public void AddProgram(string[] sources, string options = null)
		{
			_pendingPrograms.Add((sources, options));
		}
		
		public async Task BuildPrograms(string options = null) 
		{
			await Task.WhenAll(
				ContextIds.Select(contextId => BuildPrograms(contextId, options)));
		}
		 
		public async Task BuildPrograms(nint contextId, string options = null)
		{
			var api = OclApi.Value;
			
			var deviceIds = GetDeviceIds(contextId);
			
		}

		private async Task BuildProgram(nint contextId, nint programId, string options = null)
		{

		}

		public async Task BuildProgramForDevices(nint programId, nint[] deviceIds, string options = null)
		{
			var exceptions = new List<Exception>();
			foreach (var deviceId in deviceIds)
			{
				Exception exception = null;
				try
				{
					await BuildProgramForDevice(programId, deviceId, options);
				}
				catch (Exception ex)
				{
					exception = ex;
					exceptions.Add(ex);
				}

				if (exception != null)
					
			}

			if (exceptions.Count != 0)
				throw new AggregateException($"Unable to build program {programId}.", exceptions);
		}

		public async Task BuildProgramForDevice(nint programId, nint deviceId, string options = null)
		{
			await OclHelper.BuildProgram(programId, deviceId, options);
		}

		public async Task<nint> GetKernel(nint programId, string name)
		{
			var api = OclApi.Value;

			var kernelId = api.CreateKernel(programId, name, out var err);
			if (err != (int) CLEnum.Success)
				throw new Exception($"Unable to create kernel {name} (OpenCL: {err}).");


		}

		#region ICancelable
		private readonly CompositeDisposable _disposable = new();
		public bool IsDisposed => _disposable.IsDisposed;

		public void Dispose()
		{
			if (!IsDisposed)
				_disposable.Dispose();
		}
		#endregion
	}
}
