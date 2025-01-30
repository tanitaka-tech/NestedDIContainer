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
        ScopeId? IScope.ParentScopeId => _parentScopeId;
        private ScopeId? _parentScopeId = null;
        private ScopeId _scopeId;
        
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
            instance.InitializeScope(ScopeId.Create(), _scopeId, config);
            return instance;
        }
        
        public MonoBehaviourScopeWithConfig<TConfig> InstantiateWithConfig<TConfig>(MonoBehaviourScopeWithConfig<TConfig> prefab, TConfig config, Transform parent) 
            where TConfig : class
        {
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.InitializeScope(ScopeId.Create(), _scopeId, config);
            return instance;
        }
        
        internal void InitializeScope(ScopeId scopeId, ScopeId parentScopeId, object config = null)
        {
            _scopeId = scopeId;
            _parentScopeId = parentScopeId;
            var childBoundTypes = new List<System.Type>();
            var childBinder = new DependencyBinder(ProjectScope.Modules, scopeId, ref childBoundTypes);
            
            _extendScopes.ForEach(extendScope => childBinder.ExtendScope(extendScope));
            
            ProjectScope.NestedScopes.Add(scopeId, this);
            Inject(this, parentScopeId);
            ((IScope)this).Construct(childBinder, config);
            ((IScope)this).Initialize();
            this.GetCancellationTokenOnDestroy().Register(() =>
            {
                ProjectScope.NestedScopes.Remove(scopeId);
                childBoundTypes.ForEach(type => ProjectScope.Modules.Remove(scopeId, type));
                childBoundTypes.Clear();
            });
        }
        
        private void Inject(
            IScope scope,
            ScopeId scopeId)
        {
            var type = scope.GetType();
            var fields = type.GetFields(MemberBindingFlags);
            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    field.SetValue(scope, ProjectScope.Modules.Resolve(field.FieldType, scopeId));
                }
            }
            var props = type.GetProperties(MemberBindingFlags);
            foreach (var prop in props)
            {
                var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    prop.SetValue(scope, ProjectScope.Modules.Resolve(prop.PropertyType, scopeId));
                }
            }
        }
    }
}