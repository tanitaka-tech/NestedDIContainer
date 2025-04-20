using System;
using System.Collections.Generic;
using System.Linq;

namespace TanitakaTech.NestedDIContainer
{
    public class Modules
    {
        private Dictionary<(ScopeId BelongingScopeId, IntPtr TypePtr), object> Value { get; }
        private Dictionary<ScopeId, IScope> Scopes { get; }

        public Modules(Dictionary<(ScopeId, IntPtr), object> value, Dictionary<ScopeId, IScope> scopes)
        {
            Value = value;
            Scopes = scopes;
        }

        public void Bind(ScopeId belongingScopeId, Type type, object module)
        {
            var key = (belongingScopeId, type.TypeHandle.Value);
            if (!Value.TryAdd(key, module))
            {
                throw new ConstructException($"Module already exists: {type}, {belongingScopeId}");
            }
        }
        
        public void Remove(ScopeId belongingScopeId, Type type)
        {
            var key = (belongingScopeId, type.TypeHandle.Value);
            (Value[key] as IDisposable)?.Dispose();
            Value.Remove(key);
        }

        public void RemoveScope(ScopeId belongingScopeId)
        {
            var needRemoveKey = Value.Where(k => k.Key.BelongingScopeId.Equals(belongingScopeId))
                .Select(k => k.Key)
                .ToList();
            foreach (var moduleRelation in needRemoveKey)
            {
                (Value[moduleRelation] as IDisposable)?.Dispose();
                Value.Remove(moduleRelation);
            }
            needRemoveKey.Clear();
        }

        public T Resolve<T>(IScope callScope)
        {
            return (T)Resolve(typeof(T), callScope);
        }

        public T Resolve<T>(ScopeId callScopeId)
        {
            Scopes.TryGetValue(callScopeId, out var callScope);
            return Resolve<T>(callScope);
        }

        public object Resolve(Type type, IScope callScope)
        {
            var typePtr = type.TypeHandle.Value;
            if (Value.TryGetValue((callScope.ScopeId, typePtr), out var module))
                return module;

            var parentScopeIdNullable = callScope?.ParentScopeId;
            while (parentScopeIdNullable != null)
            {
                var parentScopeId = parentScopeIdNullable.Value;

                if (Value.TryGetValue((parentScopeId, typePtr), out var module2))
                    return module2;

                Scopes.TryGetValue(parentScopeId, out var parentScope);
                parentScopeIdNullable = parentScope?.ParentScopeId;
            }
            throw new ConstructException($"Module not found: {type}, {callScope?.ScopeId}");
        }
    }
}