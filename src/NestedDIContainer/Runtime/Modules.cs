using System;
using System.Collections.Generic;
using System.Linq;

namespace TanitakaTech.NestedDIContainer
{
    public class Modules
    {
        private readonly Dictionary<(ScopeId BelongingScopeId, IntPtr TypePtr), object> _value;
        private readonly Dictionary<ScopeId, IScope> _scopes;

        public Modules(Dictionary<(ScopeId, IntPtr), object> value, Dictionary<ScopeId, IScope> scopes)
        {
            _value = value;
            _scopes = scopes;
        }

        public void Bind(ScopeId belongingScopeId, Type type, object module)
        {
            var key = (belongingScopeId, type.TypeHandle.Value);
            if (!_value.TryAdd(key, module))
            {
                throw new ConstructException($"Module already exists: {type}, {belongingScopeId}");
            }
        }
        
        public void Remove(ScopeId belongingScopeId, Type type)
        {
            var key = (belongingScopeId, type.TypeHandle.Value);
            (_value[key] as IDisposable)?.Dispose();
            _value.Remove(key);
        }

        public void RemoveScope(ScopeId belongingScopeId)
        {
            var needRemoveKey = _value.Where(k => k.Key.BelongingScopeId.Equals(belongingScopeId))
                .Select(k => k.Key)
                .ToList();
            foreach (var moduleRelation in needRemoveKey)
            {
                (_value[moduleRelation] as IDisposable)?.Dispose();
                _value.Remove(moduleRelation);
            }
            needRemoveKey.Clear();
        }

        public T Resolve<T>(IScope callScope)
        {
            return (T)Resolve(typeof(T), callScope);
        }

        public T Resolve<T>(ScopeId callScopeId)
        {
            _scopes.TryGetValue(callScopeId, out var callScope);
            return Resolve<T>(callScope);
        }

        public object Resolve(Type type, IScope callScope)
        {
            var typePtr = type.TypeHandle.Value;
            if (_value.TryGetValue((callScope.ScopeId, typePtr), out var module))
                return module;

            var parentScopeIdNullable = callScope?.ParentScopeId;
            while (parentScopeIdNullable != null)
            {
                var parentScopeId = parentScopeIdNullable.Value;

                if (_value.TryGetValue((parentScopeId, typePtr), out var module2))
                    return module2;

                _scopes.TryGetValue(parentScopeId, out var parentScope);
                parentScopeIdNullable = parentScope?.ParentScopeId;
            }
            throw new ConstructException($"Module not found: {type}, {callScope?.ScopeId}");
        }
    }
}