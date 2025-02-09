using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using TanitakaTech.NestedDIContainer;
using UnityEngine;

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

            // Inject Children
            var scopeChildren = FindComponentsInChildrenOnce<IInjectable>(this.gameObject)
                .Select(InjectOrSelect)
                .Where(child => child != null)
                .Select(child => (scope: child, scopeId: ScopeId.Create(), parentScopeId: ScopeId))
                .ToList();
            foreach (var scopeChild in scopeChildren)
            {
                scopeChild.scope.InitializeScope(scopeId: scopeChild.scopeId, parentScopeId: scopeChild.parentScopeId);
            }

            return;

            MonoBehaviourScopeBase InjectOrSelect(IInjectable child)
            {
                if (child is MonoBehaviourScopeBase monoBehaviourScope)
                {
                    return monoBehaviourScope;
                }
                else
                {
                    // Inject to IInjectable
                    Inject(injectableObject: child, scopeId: ScopeId.Create());
                    return null;
                }
            }
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

        private List<T> FindComponentsInChildrenOnce<T>(GameObject parent)
        {
            List<T> foundComponents = new List<T>();
            foreach (Transform child in parent.transform)
            {
                FindComponentsRecursive(child, foundComponents);
            }
            return foundComponents;

            void FindComponentsRecursive<T>(Transform current, List<T> foundComponents)
            {
                T component = current.GetComponent<T>();

                if (component != null)
                {
                    foundComponents.Add(component);
                    return;
                }

                foreach (Transform child in current)
                {
                    FindComponentsRecursive(child, foundComponents);
                }
            }
        }
    }
}