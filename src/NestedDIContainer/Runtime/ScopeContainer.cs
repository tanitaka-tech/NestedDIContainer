using System;
using System.Collections.Generic;
using System.Reflection;
using NestedDIContainer.Unity.Runtime;

namespace TanitakaTech.NestedDIContainer
{
    public class ScopeContainer
    {
        private readonly Dictionary<IntPtr, object> _value = new Dictionary<IntPtr, object>();
        private readonly ScopeId _scopeId;
        private readonly ScopeContainer _parentScopeContainer;

        public ScopeContainer(ScopeId scopeId, ScopeContainer parentScopeContainer)
        {
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

        private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        public void Inject(object injectableObject, IScope scope)
        {
            var type = injectableObject.GetType();
            var fields = type.GetFields(MemberBindingFlags);
            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    field.SetValue(injectableObject, Resolve(field.FieldType));
                }
            }

            var props = type.GetProperties(MemberBindingFlags);
            foreach (var prop in props)
            {
                var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    prop.SetValue(scope, Resolve(prop.PropertyType));
                }
            }
        }
    }
}