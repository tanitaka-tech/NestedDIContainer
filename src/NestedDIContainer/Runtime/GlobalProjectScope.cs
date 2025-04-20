using System;
using System.Collections.Generic;
using System.Reflection;
using NestedDIContainer.Unity.Runtime;

namespace TanitakaTech.NestedDIContainer
{
    public static class GlobalProjectScope
    {
        public static Dictionary<ScopeId, IScope> Scopes
        {
            get
            {
                _scopes ??= new Dictionary<ScopeId, IScope>();
                return _scopes;
            }
        }
        private static Dictionary<ScopeId, IScope> _scopes;

        public static ScopeContainer ScopeContainer
        {
            get
            {
                _scopeContainer ??= new ScopeContainer(new Dictionary<IntPtr, object>(), ScopeId.Create(), null);
                return _scopeContainer;
            }
        }
        private static ScopeContainer _scopeContainer;

        private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public static void Inject(object injectableObject, IScope scope)
        {
            var type = injectableObject.GetType();
            var fields = type.GetFields(MemberBindingFlags);
            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    field.SetValue(injectableObject, ScopeContainer.Resolve(field.FieldType));
                }
            }

            var props = type.GetProperties(MemberBindingFlags);
            foreach (var prop in props)
            {
                var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    prop.SetValue(scope, ScopeContainer.Resolve(prop.PropertyType));
                }
            }
        }

        public static void Dispose()
        {
            _scopes = null;
            _scopeContainer = null;
        }
    }
}