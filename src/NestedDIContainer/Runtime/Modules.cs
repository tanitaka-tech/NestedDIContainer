using System;
using System.Collections.Generic;
using System.Linq;

namespace TanitakaTech.NestedDIContainer
{
    public class Modules
    {
        private Dictionary<ModuleRelation, object> Value { get; }
        private Dictionary<ScopeId, IScope> Scopes { get; }

        public Modules(Dictionary<ModuleRelation, object> value, Dictionary<ScopeId, IScope> scopes)
        {
            Value = value;
            Scopes = scopes;
        }

        public void Bind(ScopeId belongingScopeId, Type type, object module)
        {
            var key = new ModuleRelation(belongingScopeId, type);
            if (!Value.TryAdd(key, module))
            {
                throw new ConstructException($"Module already exists: {type}, {belongingScopeId}");
            }
        }
        
        public void Remove(ScopeId belongingScopeId, Type type)
        {
            var key = new ModuleRelation(belongingScopeId, type);
            (Value[key] as IDisposable)?.Dispose();
            Value.Remove(key);
        }

        public void RemoveScope(ScopeId belongingScopeId)
        {
            var needRemoveKey = Value.Where(k => k.Key.BelongingScopeId.Equals(belongingScopeId))
                .Select(k => k.Key)
                .ToList();
            needRemoveKey.ForEach(k =>
            {
                (Value[k] as IDisposable)?.Dispose();
                Value.Remove(k);
            });
            needRemoveKey.Clear();
        }
        
        public T Resolve<T>(ScopeId callScopeId) => (T) Resolve(typeof(T), callScopeId);
        
        public object Resolve(Type type, ScopeId callScopeId)
        {
            if (Value.TryGetValue(new ModuleRelation(callScopeId, type), out var module))
                return module;
            
            Scopes.TryGetValue(callScopeId, out var callScope);
            var parentScopeIdNullable = callScope?.ParentScopeId;
            
            while (parentScopeIdNullable != null)
            {
                var parentScopeId = parentScopeIdNullable.Value;
                
                if (Value.TryGetValue(new ModuleRelation(parentScopeId, type), out var module2))
                    return module2;

                Scopes.TryGetValue(parentScopeId, out var parentScope);
                parentScopeIdNullable = parentScope?.ParentScopeId;
            }
            ThrowException(type: type, callScope: callScope);
            return null;
            
            void ThrowException(Type type, IScope callScope)
            {
                throw new ConstructException($"Module not found: {type}, {callScope}");
            }
        }
    }

    public readonly struct ModuleRelation : IEquatable<ModuleRelation>
    {
        public ScopeId BelongingScopeId { get; }
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