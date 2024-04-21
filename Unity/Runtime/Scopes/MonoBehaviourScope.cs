using System.Collections.Generic;
using TanitakaTech.NestedDIContainer;
using UnityEngine;

namespace NestedDIContainer.Unity.Runtime
{
    public abstract class MonoBehaviourScopeBase : MonoBehaviour, IScope
    {
        [SerializeField] private List<ScriptableObjectExtendScope> _extendScopes;
        ScopeId? IScope.ParentScopeId => _parentScopeId;
        private readonly ScopeId? _parentScopeId = null;

        void IScope.Construct(DependencyBinder binder, object config)
        {
            Construct(binder, config);
        }
        protected abstract void Construct(DependencyBinder binder, object config);
    }
    
    public abstract class MonoBehaviourScope : MonoBehaviourScope<MonoBehaviourScope.EmptyConfig>
    {
        public abstract class EmptyConfig { }
        protected override void Construct(DependencyBinder binder, EmptyConfig config) => Construct(binder);
        protected abstract void Construct(DependencyBinder binder);
    }
    
    public abstract class MonoBehaviourScope<TConfig> : MonoBehaviourScopeBase
    {
        protected override void Construct(DependencyBinder binder, object config) => Construct(binder, (TConfig)config);
        protected abstract void Construct(DependencyBinder binder, TConfig config);
    }
}