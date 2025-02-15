using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using TanitakaTech.NestedDIContainer;
using UnityEngine;
using IInjectable = TanitakaTech.NestedDIContainer.IInjectable;

namespace NestedDIContainer.Unity.Runtime.Core
{
    [DefaultExecutionOrder(-5000)]
    public abstract class MonoBehaviourScopeBase : MonoBehaviour, IScope, IInjectable
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
        
        internal void InitializeScope(ScopeId scopeId, ScopeId parentScopeId, object config = null, IExtendScope optionExtendScope = null)
        {
            ScopeId = scopeId;
            ParentScopeId = parentScopeId;

            var childBinder = new DependencyBinder(scopeId);
            if (optionExtendScope != null)
            {
                childBinder.ExtendScope(optionExtendScope);
            }
            foreach (var extendScope in _extendScopes)
            {
                childBinder.ExtendScope(extendScope);
            }

            GlobalProjectScope.Scopes.Add(scopeId, this);
            Inject(this, scopeId);
            IScope scope = this;
            scope.Construct(childBinder, config);
            scope.Initialize();

            this.GetCancellationTokenOnDestroy().Register(() =>
            {
                GlobalProjectScope.Scopes.Remove(scopeId);
                GlobalProjectScope.Modules.RemoveScope(scopeId);
            });

            InjectOrInitializeChildren(this.gameObject);
        }
        
        private void Inject(object injectableObject, ScopeId scopeId)
        {
            var type = injectableObject.GetType();
            var fields = type.GetFields(MemberBindingFlags);
            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    field.SetValue(injectableObject, GlobalProjectScope.Modules.Resolve(field.FieldType, scopeId));
                }
            }
            var props = type.GetProperties(MemberBindingFlags);
            foreach (var prop in props)
            {
                var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    prop.SetValue(injectableObject, GlobalProjectScope.Modules.Resolve(prop.PropertyType, scopeId));
                }
            }
        }

        private void InjectOrInitializeChildren(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                InjectOrInitializeChildrenRecursive(child);
            }
            return;

            void InjectOrInitializeChildrenRecursive(Transform current)
            {
                var injectable = current.GetComponent<IInjectable>();
                if (injectable != null)
                {
                    var scopeId = ScopeId.Create();
                    if (injectable is MonoBehaviourScopeBase monoBehaviourScope)
                    {
                        monoBehaviourScope.InitializeScope(scopeId: scopeId, parentScopeId: ScopeId);
                    }
                    else
                    {
                        GlobalProjectScope.Scopes.Add(scopeId, this);
                        Inject(injectableObject: injectable, scopeId: scopeId);
                    }
                    return;
                }

                foreach (Transform child in current)
                {
                    InjectOrInitializeChildrenRecursive(child);
                }
            }
        }
    }
}