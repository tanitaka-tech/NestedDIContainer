using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using TanitakaTech.NestedDIContainer;
using UnityEngine;

namespace NestedDIContainer.Unity.Runtime.Core
{
    public abstract class MonoBehaviourScopeBase : MonoBehaviour, IScope
    {
        [SerializeField] protected List<ScriptableObjectExtendScope> _extendScopes;

        public ScopeId ScopeId { get; set; }
        public ScopeId? ParentScopeId { get; set; }

        private const BindingFlags MemberBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        void IScope.Construct(DependencyBinder binder, object config)
        {
            Construct(binder, config);
        }
        protected abstract void Construct(DependencyBinder binder, object config);
        void IScope.Initialize() => Initialize();
        protected virtual void Initialize() {}

        public T Instantiate<T>(T prefab, Transform parent, object config = null) where T : MonoBehaviourScopeBase
        {
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.InitializeScope(ScopeId.Create(), ScopeId, config);
            return instance;
        }
        
        public MonoBehaviourScopeWithConfig<TConfig> InstantiateWithConfig<TConfig>(MonoBehaviourScopeWithConfig<TConfig> prefab, TConfig config, Transform parent) 
            where TConfig : class
        {
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.InitializeScope(ScopeId.Create(), ScopeId, config);
            return instance;
        }
        
        internal void InitializeScope(ScopeId scopeId, ScopeId parentScopeId, object config = null)
        {
            ScopeId = scopeId;
            ParentScopeId = parentScopeId;
            var childBinder = new DependencyBinder(scopeId);
            foreach (var extendScope in _extendScopes)
            {
                childBinder.ExtendScope(extendScope);
            }

            GlobalProjectScope.Scopes.Add(scopeId, this);
            Inject(this, parentScopeId);
            ((IScope)this).Construct(childBinder, config);
            ((IScope)this).Initialize();
            this.GetCancellationTokenOnDestroy().Register(() =>
            {
                GlobalProjectScope.Scopes.Remove(scopeId);
                GlobalProjectScope.Modules.RemoveScope(scopeId);
            });
        }
        
        private void Inject(IScope scope, ScopeId scopeId)
        {
            var type = scope.GetType();
            var fields = type.GetFields(MemberBindingFlags);
            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    field.SetValue(scope, GlobalProjectScope.Modules.Resolve(field.FieldType, scopeId));
                }
            }
            var props = type.GetProperties(MemberBindingFlags);
            foreach (var prop in props)
            {
                var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    prop.SetValue(scope, GlobalProjectScope.Modules.Resolve(prop.PropertyType, scopeId));
                }
            }
        }
    }
}