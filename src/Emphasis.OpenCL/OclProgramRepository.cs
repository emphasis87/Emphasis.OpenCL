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

	}

	public class OclProgramRepository
	{
		private readonly object _gate = new();
		private readonly Dictionary<string, int> _sourceDictionary = new();
		private readonly List<string> _sourceList = new();
		private readonly Dictionary<string, int> _optionsDictionary = new() {{"", 0}};
		private readonly List<string> _optionsList = new() {""};
		
		public int AddSource(string source)
		{
			lock (_gate)
			{
				if (_sourceDictionary.TryGetValue(source, out var index))
					return index;

				index = _sourceList.Count;
				_sourceList.Add(source);
				_sourceDictionary.Add(source, index);
				return index;
			}
		}

		public int AddOptions(string options)
		{
			lock (_gate)
			{
				if (_optionsDictionary.TryGetValue(options, out var index))
					return index;

				index = _optionsList.Count;
				_optionsList.Add(options);
				_optionsDictionary.Add(options, index);
				return index;
			}
		}
		
		public async Task<int> BuildProgram(nint contextId, int source, int options = default)
		{

		}

		public async Task<int> BuildProgram(nint contextId, nint deviceId, int sourceId, int optionsId = default)
		{
			string source;
			lock (_gate)
			{
				source = _sourceList[sourceId];
			}

			var programId = CreateProgram(contextId, source);
		}

		public nint CreateKernel(int program)
		{

		}
	}
}
