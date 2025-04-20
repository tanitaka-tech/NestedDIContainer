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

        public static Modules Modules
        {
            get
            {
                _modules ??= new Modules(new Dictionary<(ScopeId, IntPtr), object>(), Scopes);
                return _modules;
            }
        }
        private static Modules _modules;

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
                    field.SetValue(injectableObject, Modules.Resolve(field.FieldType, scope));
                }
            }

            var props = type.GetProperties(MemberBindingFlags);
            foreach (var prop in props)
            {
                var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    prop.SetValue(scope, Modules.Resolve(prop.PropertyType, scope));
                }
            }
        }

        public static void Dispose()
        {
            _scopes = null;
            _modules = null;
        }
    }
}