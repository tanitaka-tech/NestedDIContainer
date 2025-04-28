using System;

namespace TanitakaTech.NestedDIContainer
{
    public readonly struct ScopeId : IEquatable<ScopeId>
    {
        private readonly Guid _value;

        private ScopeId(Guid value)
        {
            _value = value;
        }
        public static ScopeId Create() => new ScopeId(Guid.NewGuid());

        public bool Equals(ScopeId other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is ScopeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(ScopeId left, ScopeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ScopeId left, ScopeId right)
        {
            return !left.Equals(right);
        }
    }
}