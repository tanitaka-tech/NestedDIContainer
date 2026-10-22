using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TanitakaTech.NestedDIContainer.Runtime;

namespace TanitakaTech.NestedDIContainer
{
    public class ScopeContainer
    {
        private readonly Dictionary<IntPtr, object> _value = new Dictionary<IntPtr, object>();
        private readonly IScope _scope;
        private readonly ScopeContainer _parentScopeContainer;
        public IScope Scope => _scope;
        public IScope ParentScope => _parentScopeContainer?.Scope;

        public ScopeContainer(IScope scope, ScopeContainer parentScopeContainer)
        {
            _scope = scope;
            _parentScopeContainer = parentScopeContainer;
        }

        public void Bind(Type type, object module)
        {
            var key = type.TypeHandle.Value;
            if (!_value.TryAdd(key, module))
            {
                throw new ConstructException($"Module already exists: {type}, {_scope}");
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
        
        public T TryResolve<T>()
        {
            return (T)TryResolve(typeof(T));
        }
        
        public object TryResolve(Type type)
        {
            var typePtr = type.TypeHandle.Value;
            if (_value.TryGetValue(typePtr, out var module))
                return module;

            if (_parentScopeContainer != null)
            {
                return _parentScopeContainer.TryResolve(type);
            }
            return null;
        }

        private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// 型ごとの注入先のフィールドとプロパティ。
        /// NOTE: 注入のたびに GetFields / GetProperties と、メンバーごとの GetCustomAttribute を呼ぶと、
        ///       注入するオブジェクトが多い画面（シーンの読み込み）でリフレクションの時間が積み重なる。同じ型の結果は変わらないのでキャッシュする
        /// </summary>
        private sealed class InjectionTargets
        {
            public readonly FieldInfo[] Fields;
            public readonly FieldInfo[] OptionalFields;
            public readonly PropertyInfo[] Properties;
            public readonly PropertyInfo[] OptionalProperties;

            public InjectionTargets(Type type)
            {
                var fields = type.GetFields(MemberBindingFlags);
                Fields = fields.Where(field => field.GetCustomAttribute<InjectAttribute>() != null).ToArray();
                var props = type.GetProperties(MemberBindingFlags);
                Properties = props.Where(prop => prop.GetCustomAttribute<InjectAttribute>() != null).ToArray();
#if USE_INJECT_OPTIONAL
                OptionalFields = fields.Where(field => field.GetCustomAttribute<InjectOptionalAttribute>() != null).ToArray();
                OptionalProperties = props.Where(prop => prop.GetCustomAttribute<InjectOptionalAttribute>() != null).ToArray();
#else
                OptionalFields = Array.Empty<FieldInfo>();
                OptionalProperties = Array.Empty<PropertyInfo>();
#endif
            }
        }

        private static readonly ConcurrentDictionary<Type, InjectionTargets> InjectionTargetsCache = new ConcurrentDictionary<Type, InjectionTargets>();
        private static readonly Func<Type, InjectionTargets> CreateInjectionTargets = type => new InjectionTargets(type);

        private static InjectionTargets GetInjectionTargets(Type type)
        {
            return InjectionTargetsCache.GetOrAdd(type, CreateInjectionTargets);
        }

        public void Inject(object injectableObject)
        {
            var targets = GetInjectionTargets(injectableObject.GetType());
            foreach (var field in targets.Fields)
            {
                field.SetValue(injectableObject, Resolve(field.FieldType));
            }
            foreach (var field in targets.OptionalFields)
            {
                field.SetValue(injectableObject, TryResolve(field.FieldType));
            }

            foreach (var prop in targets.Properties)
            {
                prop.SetValue(injectableObject, Resolve(prop.PropertyType));
            }
            foreach (var prop in targets.OptionalProperties)
            {
                prop.SetValue(injectableObject, TryResolve(prop.PropertyType));
            }
        }
    }
}