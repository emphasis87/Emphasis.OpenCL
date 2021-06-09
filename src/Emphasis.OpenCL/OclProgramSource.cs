using System;

namespace Emphasis.OpenCL
{
	public class OclProgramSource
	{
		public nint ContextId { get; }
		public nint DeviceId { get; }
		public string Source { get; }
		public string Options { get; }

		private int? _hashCode;

		public OclProgramSource(nint contextId, nint deviceId, string source, string options)
		{
			ContextId = contextId;
			DeviceId = deviceId;
			Source = source;
			Options = options;
		}

		public bool Equals(OclProgramSource other)
		{
			return ContextId.Equals(other.ContextId)
			    && DeviceId.Equals(other.DeviceId)
			    && Source == other.Source
			    && Options == other.Options;
		}

		public override bool Equals(object obj)
		{
			return obj is OclProgramSource other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _hashCode ??= HashCode.Combine(ContextId, DeviceId, Source, Options);
		}
	}
}
