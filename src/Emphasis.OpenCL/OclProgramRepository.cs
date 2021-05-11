using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.OpenCL
{
	public interface IOclProgramRepository
	{
		Task<nint> GetProgram(nint context, nint deviceId, string source, string options);
	}

	public class OclProgramRepository : IOclProgramRepository
	{
		private readonly object _gate = new();

		private readonly ConcurrentDictionary<OclProgram, Lazy<nint>> _programsLazy = new();
		private readonly ConcurrentDictionary<OclProgram, nint> _programs = new();

		public async Task<nint> GetProgram(OclProgram program)
		{
			
			if (_programs.TryGetValue(program, out var programId))
				return programId;
			
			var lazy = _programsLazy.GetOrAdd(program, new Lazy<nint>())
		}

		
	}
}
