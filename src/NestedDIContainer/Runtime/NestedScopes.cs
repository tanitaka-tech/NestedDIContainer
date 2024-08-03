using System.Collections.Generic;

namespace TanitakaTech.NestedDIContainer
{
    public class NestedScopes
    {
        private Dictionary<ScopeId, IScope> Value { get; }

        public NestedScopes(Dictionary<ScopeId, IScope> values)
        {
            Value = values;
        }
        
        public void Add(ScopeId key, IScope value)
        {
            Value.Add(key, value);
        }
        
        public void Remove(ScopeId key)
        {
            Value.Remove(key);
        }

        public IScope Get(ScopeId key)
        {
            Value.TryGetValue(key, out var value);
            return value;
        }
    }
}