using System;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL
{
	public interface IOclProgramRepository
	{
		Task<nint> GetProgram(nint contextId, nint deviceId, string source, string options);
		Task<nint> GetProgram(OclProgram program);
	}

	public class OclProgramRepository : IOclProgramRepository, ICancelable
	{
		public static IOclProgramRepository Shared { get; } = new OclProgramRepository();

		private readonly ConcurrentDictionary<OclProgram, Lazy<Task<nint>>> _programsLazy = new();

		public Task<nint> GetProgram(nint contextId, nint deviceId, string source, string options)
		{
			return GetProgram(new OclProgram(contextId, deviceId, source, options));
		}

		public async Task<nint> GetProgram(OclProgram program)
		{
			async Task<nint> CreateProgram()
			{
				var pid = OclHelper.CreateProgram(program.ContextId, program.Source);
				await BuildProgram(pid, program.DeviceId, program.Options);
				return pid;
			}

			var init = _programsLazy.GetOrAdd(program, new Lazy<Task<nint>>(CreateProgram));
			var programId = await init.Value;

			_disposable.Add(Disposable.Create(() => ReleaseProgram(programId)));

			return programId;
		}

		#region ICancellable
		private readonly CompositeDisposable _disposable = new();

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool isDisposing)
		{
			if (isDisposing)
				GC.SuppressFinalize(this);

			_disposable.Dispose();
		}

		public bool IsDisposed => _disposable.IsDisposed;
		#endregion

		~OclProgramRepository()
		{
			Dispose(false);
		}
	}
}
