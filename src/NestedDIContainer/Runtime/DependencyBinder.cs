using System;
using System.Linq;

namespace TanitakaTech.NestedDIContainer
{
    public readonly ref struct DependencyBinder
    {
        private ScopeId ScopeId { get; }

        public DependencyBinder(ScopeId scopeId)
        {
            ScopeId = scopeId;
        }
        
        public void ExtendScope(IExtendScope scope)
        {
            scope.Construct(this);
        }
        
        public T ExtendScope<T>() where T : IExtendScope
        {
            var type = typeof(T);
            var constructor = type.GetConstructors().First();
            var parameters = constructor.GetParameters();
            var parameterValues = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterValues[i] = GlobalProjectScope.Modules.Resolve(parameters[i].ParameterType, ScopeId);
            }
            var instance = (T)Activator.CreateInstance(type, parameterValues);
            
            ExtendScope(instance);
            
            return instance;
        }
        
        public TResult ExtendScopeWithResult<T, TResult>() where T : IExtendScopeWithResult<TResult>
        {
            var instance = ExtendScope<T>();
            return instance.GetResult();
        }
        
        public void Bind(Type type, object instance)
        {
            GlobalProjectScope.Modules.Bind(ScopeId, type, instance);
        }
        
        public void Bind<T>(T instance) => Bind(typeof(T), instance);
        
        public void Bind<TInstance, T1, T2>(TInstance instance) where TInstance : T1, T2
        {
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
        }
        
        public void Bind<T1, T2>(object instance)
        {
            if( instance is T1 == false || instance is T2 == false)
                throw new ConstructException("Binding failed: instance type does not match with generic type");
            
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
        }
        
        public void Bind<T1, T2, T3>(object instance)
        {
            if( instance is T1 == false || instance is T2 == false || instance is T3 == false)
                throw new ConstructException("Binding failed: instance type does not match with generic type");
            
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
            Bind(typeof(T3), instance);
        }
        
        public void Bind<T1, T2, T3, T4>(object instance)
        {
            if( instance is T1 == false || instance is T2 == false || instance is T3 == false || instance is T4 == false)
                throw new ConstructException("Binding failed: instance type does not match with generic type");
            
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
            Bind(typeof(T3), instance);
            Bind(typeof(T4), instance);
        }
        
        public void Bind<T1, T2, T3, T4, T5>(object instance)
        {
            if( instance is T1 == false || instance is T2 == false || instance is T3 == false || instance is T4 == false || instance is T5 == false)
                throw new ConstructException("Binding failed: instance type does not match with generic type");
            
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
            Bind(typeof(T3), instance);
            Bind(typeof(T4), instance);
            Bind(typeof(T5), instance);
        }
        
        public void Bind<T1, T2, T3, T4, T5, T6>(object instance)
        {
            if( instance is T1 == false || instance is T2 == false || instance is T3 == false || instance is T4 == false || instance is T5 == false || instance is T6 == false)
                throw new ConstructException("Binding failed: instance type does not match with generic type");
            
            Bind(typeof(T1), instance);
            Bind(typeof(T2), instance);
            Bind(typeof(T3), instance);
            Bind(typeof(T4), instance);
            Bind(typeof(T5), instance);
            Bind(typeof(T6), instance);
        }
    }
}