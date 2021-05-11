using System;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL
{
	public interface IOclProgramRepository : ICancelable
	{
		Task<nint> GetProgram(OclProgram program);
	}

	public class OclProgramRepository : IOclProgramRepository
	{
		private readonly ConcurrentDictionary<OclProgram, Lazy<Task<nint>>> _programsLazy = new();

		public async Task<nint> GetProgram(OclProgram program)
		{
			var api = OclApi.Value;

			async Task<nint> CreateProgram()
			{
				var pid = OclHelper.CreateProgram(program.ContextId, program.Source);
				await BuildProgram(pid, program.DeviceId, program.Options);
				return pid;
			}

			var init = _programsLazy.GetOrAdd(program, new Lazy<Task<nint>>(CreateProgram));
			var programId = await init.Value;

			_disposable.Add(Disposable.Create(() => api.ReleaseProgram(programId)));

			return programId;
		}

		#region ICancellable
		private readonly CompositeDisposable _disposable = new();

		public void Dispose()
		{
			_disposable.Dispose();
		}

		public bool IsDisposed => _disposable.IsDisposed;
		#endregion
	}
}
