using System.Collections.Generic;
using TanitakaTech.NestedDIContainer;
using UnityEngine;

namespace NestedDIContainer.Unity.Runtime.Core
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
        void IScope.Initialize() => Initialize();
        protected virtual void Initialize() {}
    }
}