using System;

namespace TanitakaTech.NestedDIContainer
{
    public readonly struct ScopeId : IEquatable<ScopeId>
    {
        private Guid Value { get; }

        private ScopeId(Guid value)
        {
            Value = value;
        }
        public static ScopeId Create() => new ScopeId(Guid.NewGuid());

        public bool Equals(ScopeId other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ScopeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}