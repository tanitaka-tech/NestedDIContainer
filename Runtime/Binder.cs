using System;
using System.Collections.Generic;

namespace TanitakaTech.NestedDIContainer
{
    public readonly struct Binder
    {
        private ScopeId ScopeId { get; }
        private readonly Modules _bindingModules;
        private readonly List<Type> _boundTypes;

        public Binder(Modules bindingModules, ScopeId scopeId, List<Type> boundTypes)
        {
            ScopeId = scopeId;
            _bindingModules = bindingModules;
            _boundTypes = new List<Type>();
        }
        
        public void Bind(Type type, object instance)
        {
            if (_bindingModules == null)
                throw new ConstructException("Binding failed: no container assigned!");

            _bindingModules.Bind(ScopeId, type, instance);
            _boundTypes.Add(type);
        }
        public void Bind<T>(T instance) => Bind(typeof(T), instance);
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