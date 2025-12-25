using System;
using System.Threading;

namespace TanitakaTech.NestedDIContainer
{
    public readonly ref struct DependencyBinder
    {
        private readonly ScopeContainer _scopeContainer;

        public DependencyBinder(ScopeContainer scopeContainer)
        {
            _scopeContainer = scopeContainer;
        }

        public void ExtendScope(IExtendScope scope, CancellationToken scopeLifetime)
        {
            _scopeContainer.Inject(scope);
            scope.Construct(this, scopeLifetime);
        }
        
        public void Bind(Type type, object instance)
        {
            _scopeContainer.Bind(type, instance);
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