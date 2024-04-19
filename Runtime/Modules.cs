using System;
using System.Collections.Generic;

namespace TanitakaTech.NestedDIContainer
{
    public class Modules
    {
        private Dictionary<ModuleRelation, object> Value { get; }
        private NestedScopes NestedScopes { get; }

        public Modules(Dictionary<ModuleRelation, object> value, NestedScopes nestedScopes)
        {
            Value = value;
            NestedScopes = nestedScopes;
        }
        
        public void Bind(ScopeId belongingScopeId, Type type, object module)
        {
            Value.Add(new ModuleRelation(belongingScopeId, type), module);
        }
        
        public void Remove(ScopeId belongingScopeId, Type type)
        {
            Value.Remove(new ModuleRelation(belongingScopeId, type));
        }
        
        public T Resolve<T>(ScopeId callScopeId) => (T) Resolve(typeof(T), callScopeId);
        
        public object Resolve(Type type, ScopeId callScopeId)
        {
            if (Value.TryGetValue(new ModuleRelation(callScopeId, type), out var module))
                return module;
            
            var callScope = NestedScopes.Get(callScopeId);
            var parentScopeIdNullable = callScope.ParentScopeId;
            
            while (parentScopeIdNullable != null)
            {
                var parentScopeId = parentScopeIdNullable.Value;
                
                if (Value.TryGetValue(new ModuleRelation(parentScopeId, type), out var module2))
                    return module2;
                parentScopeIdNullable = NestedScopes.Get(parentScopeId).ParentScopeId;
            }
            throw new ConstructException($"Module not found: {type}, {callScope}");
        }
    }

    public readonly struct ModuleRelation : IEquatable<ModuleRelation>
    {
        private ScopeId BelongingScopeId { get; }
        private Type Type { get; }

        public ModuleRelation(ScopeId belongingScopeId, Type type)
        {
            BelongingScopeId = belongingScopeId;
            Type = type;
        }

        public bool Equals(ModuleRelation other)
        {
            return BelongingScopeId.Equals(other.BelongingScopeId) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleRelation other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BelongingScopeId, Type);
        }
    }
    
}