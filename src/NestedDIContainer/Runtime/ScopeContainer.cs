using System;
using System.Collections.Generic;

namespace TanitakaTech.NestedDIContainer
{
    public class ScopeContainer
    {
        private readonly Dictionary<IntPtr, object> _value;
        private readonly ScopeId _scopeId;
        private readonly ScopeContainer _parentScopeContainer;

        public ScopeContainer(Dictionary<IntPtr, object> value, ScopeId scopeId, ScopeContainer parentScopeContainer)
        {
            _value = value;
            _scopeId = scopeId;
            _parentScopeContainer = parentScopeContainer;
        }

        public void Bind(Type type, object module)
        {
            var key = type.TypeHandle.Value;
            if (!_value.TryAdd(key, module))
            {
                throw new ConstructException($"Module already exists: {type}, {_scopeId}");
            }
        }
        
        public void Remove(Type type)
        {
            var key = type.TypeHandle.Value;
            (_value[key] as IDisposable)?.Dispose();
            _value.Remove(key);
        }

        public void RemoveScope()
        {
            _value.Clear();
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            var typePtr = type.TypeHandle.Value;
            if (_value.TryGetValue(typePtr, out var module))
                return module;

            if (_parentScopeContainer != null)
            {
                return _parentScopeContainer.Resolve(type);
            }
            throw new ConstructException($"Module not found: {type}");
        }
    }
}