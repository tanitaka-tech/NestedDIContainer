using System;
using System.Collections.Generic;
using System.Linq;

namespace TanitakaTech.NestedDIContainer
{
    public readonly ref struct DependencyBinder
    {
        private readonly ScopeId _scopeId;
        private readonly Dictionary<ScopeId, IScope> _scopes;
        private readonly Modules _modules;

        public DependencyBinder(ScopeId scopeId, Dictionary<ScopeId, IScope> scopes, Modules modules)
        {
            _scopeId = scopeId;
            _scopes = scopes;
            _modules = modules;
        }

        public void ExtendScope(IExtendScope scope)
        {
            GlobalProjectScope.Inject(scope, GlobalProjectScope.Scopes[_scopeId]);
            scope.Construct(this);
        }
        
        public T ExtendScope<T>() where T : IExtendScope
        {
            var type = typeof(T);
            var constructor = type.GetConstructors().First();
            var parameters = constructor.GetParameters();
            var parameterValues = new object[parameters.Length];
            _scopes.TryGetValue(_scopeId, out var scope);
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterValues[i] = _modules.Resolve(parameters[i].ParameterType, scope);
            }
            var instance = (T)Activator.CreateInstance(type, parameterValues);

            instance.Construct(this);

            return instance;
        }
        
        public TResult ExtendScopeWithResult<T, TResult>() where T : IExtendScopeWithResult<TResult>
        {
            var instance = ExtendScope<T>();
            return instance.GetResult();
        }
        
        public void Bind(Type type, object instance)
        {
            GlobalProjectScope.Modules.Bind(_scopeId, type, instance);
        }
        
        public void Bind<T>(T instance) => Bind(typeof(T), instance);
        
        public void Bind<TInstance, T1, T2>(TInstance instance) where TInstance : T1, T2
        {
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
        }
        
        public void Bind<TInstance, T1, T2, T3>(TInstance instance) where TInstance : T1, T2, T3
        {
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
            Bind(typeof(T3), instance);
        }
        
        public void Bind<TInstance, T1, T2, T3, T4>(TInstance instance) where TInstance : T1, T2, T3, T4
        {
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
            Bind(typeof(T3), instance);
            Bind(typeof(T4), instance);
        }
        
        public void Bind<TInstance, T1, T2, T3, T4, T5>(TInstance instance) where TInstance : T1, T2, T3, T4, T5
        {
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
            Bind(typeof(T3), instance);
            Bind(typeof(T4), instance);
            Bind(typeof(T5), instance);
        }
        
        public void Bind<TInstance, T1, T2, T3, T4, T5, T6>(TInstance instance) where TInstance : T1, T2, T3, T4, T5, T6
        {
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
            Bind(typeof(T3), instance);
            Bind(typeof(T4), instance);
            Bind(typeof(T5), instance);
            Bind(typeof(T6), instance);
        }
    }
}