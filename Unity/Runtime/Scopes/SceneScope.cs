using System.Collections.Generic;
using System.Linq;
using NestedDIContainer.Unity.Runtime.Core;
using TanitakaTech.NestedDIContainer;
using UnityEngine;

namespace NestedDIContainer.Unity.Runtime
{
    public abstract class SceneScope : SceneScopeWithConfig<SceneScope.EmptyConfig>
    {
        public abstract class EmptyConfig { }
        protected override void Construct(DependencyBinder binder, EmptyConfig config) => Construct(binder);
        protected abstract void Construct(DependencyBinder binder);
    }
    
    public abstract class SceneScopeWithConfig<TConfig> : MonoBehaviourScopeBase,
        IScope
    {
        ScopeId? IScope.ParentScopeId => _parentScopeId;
        
        private ScopeId? _scopeId = null;
        private ScopeId? _parentScopeId = null;
        public static TConfig Config { get; set; } = default;

        protected void Awake()
        {
            // Init ScopeId
            _scopeId = ScopeId.Create();
            var parentScope = ProjectScope.Scope ?? ProjectScope.CreateProjectScope();
            _parentScopeId = _scopeId.Equals(parentScope.ScopeId) ? ScopeId.Create() : parentScope.ScopeId;
            
            InitializeScope(_scopeId.Value, _parentScopeId.Value, Config);
            Config = default;

            // Inject Children
            List<List<(MonoBehaviourScopeBase scope, ScopeId scopeId, ScopeId parentScopeId)>> childrenGroups = new();
            var beforeChildren = FindComponentsInChildrenOnce<MonoBehaviourScopeBase>(this.gameObject)
                .Select(child => (scope: child, scopeId: ScopeId.Create(), parentScopeId: _scopeId.Value))
                .ToList();
            while (beforeChildren is { Count: > 0 })
            {
                childrenGroups.Add(beforeChildren);
                var next = beforeChildren
                    .SelectMany(child =>
                    {
                        var ret = 
                            FindComponentsInChildrenOnce<MonoBehaviourScopeBase>(child.scope.gameObject)
                            .Select(son => (scope: son, scopeId: ScopeId.Create(), parentScopeId: child.scopeId))
                            .ToList();
                        return ret;
                    })
                    .ToList();
                beforeChildren = next;
            }
            
            foreach (var childrenGroup in childrenGroups)
            {
                childrenGroup.ForEach(child =>
                {
                    child.scope.InitializeScope(scopeId: child.scopeId, parentScopeId: child.parentScopeId);
                });
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
        }

        private void FindComponentsRecursive<T>(Transform current, List<T> foundComponents)
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
        
        protected override void Construct(DependencyBinder binder, object config) => Construct(binder, (TConfig)config);
        protected abstract void Construct(DependencyBinder binder, TConfig config);
        void IScope.Initialize() => Initialize();
        protected virtual void Initialize() {}
    }
}