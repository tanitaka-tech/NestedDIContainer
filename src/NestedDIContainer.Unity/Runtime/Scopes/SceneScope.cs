using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestedDIContainer.Unity.Runtime.Core;
using src.NestedDIContainer.Unity.Runtime.Scopes;
using TanitakaTech.NestedDIContainer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NestedDIContainer.Unity.Runtime
{
    public abstract class SceneScope : SceneScopeWithConfig<SceneScope.EmptyConfig>
    {
        public abstract class EmptyConfig { }
        protected override void Construct(DependencyBinder binder, EmptyConfig config) => Construct(binder);
        protected abstract void Construct(DependencyBinder binder);
    }
    
    public abstract class SceneScopeWithConfig<TConfig> : MonoBehaviourScopeBase, IScope, IChildSceneScopeLoader
    {
        protected void Awake()
        {
            // Init ScopeId
            ScopeId = ScopeId.Create();
            var parentScope = ProjectScope.Scope ?? ProjectScope.CreateProjectScope();
            ParentScopeId = ScopeId.Equals(parentScope.ScopeId) ? ScopeId.Create() : parentScope.ScopeId;

            InitializeScope(ScopeId, ParentScopeId.Value, ProjectScope.PopConfig(), new SceneScopeDefaultExtendScope(this));

            // Inject Children
            List<List<(MonoBehaviourScopeBase scope, ScopeId scopeId, ScopeId parentScopeId)>> childrenGroups = new();
            var beforeChildren = FindComponentsInChildrenOnce<MonoBehaviourScopeBase>(this.gameObject)
                .Select(child => (scope: child, scopeId: ScopeId.Create(), parentScopeId: ScopeId))
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
                foreach (var child in childrenGroup)
                {
                    child.scope.InitializeScope(scopeId: child.scopeId, parentScopeId: child.parentScopeId);
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


        // ISceneLoader implementation -----
        void ISceneScopeLoader.LoadScene<TConfig>(Action loadSceneAction, TConfig config = null) where TConfig : class
        {
            ProjectScope.PushConfig(config);
            loadSceneAction();
        }

        async UniTask ISceneScopeLoader.LoadSceneAsync<TConfig>(Func<CancellationToken, UniTask> loadSceneFunc, CancellationToken cancellationToken, TConfig config = null) where TConfig : class
        {
            ProjectScope.PushConfig(config);
            await loadSceneFunc(cancellationToken);
        }

        async UniTask ISceneScopeLoader.LoadSceneAsync<TConfig>(string sceneName, LoadSceneMode loadSceneMode, CancellationToken cancellationToken, TConfig config = null) where TConfig : class
        {
            ProjectScope.PushConfig(config);
            await SceneManager.LoadSceneAsync(sceneName, loadSceneMode).ToUniTask(cancellationToken: cancellationToken);
        }

        void ISceneScopeLoader.LoadScene<TConfig>(string sceneName, LoadSceneMode loadSceneMode, TConfig config = null) where TConfig : class
        {
            ProjectScope.PushConfig(config);
            SceneManager.LoadScene(sceneName, loadSceneMode);
        }
    }
}