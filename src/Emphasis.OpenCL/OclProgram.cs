using System;

namespace Emphasis.OpenCL
{
	public readonly struct OclProgram
	{
		public nint ContextId { get; }
		public nint DeviceId { get; }
		public string Source { get; }
		public string Options { get; }

		private readonly int _hashCode;

		public OclProgram(nint contextId, nint deviceId, string source, string options)
		{
			ContextId = contextId;
			DeviceId = deviceId;
			Source = source;
			Options = options;

			_hashCode = HashCode.Combine(contextId, deviceId, source, options);
		}

		public bool Equals(OclProgram other)
		{
			return ContextId.Equals(other.ContextId)
			    && DeviceId.Equals(other.DeviceId)
			    && Source == other.Source
			    && Options == other.Options;
		}

		public override bool Equals(object obj)
		{
			return obj is OclProgram other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}
	}
}
