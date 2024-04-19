using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TanitakaTech.NestedDIContainer;
using UnityEngine;

namespace NestedDIContainer.Unity.Runtime
{
    public abstract class ProjectScope : MonoBehaviourScope
    {
        internal static IScope Scope => _scope;
        private static ProjectScope _scope;

        internal static ProjectScope CreateProjectScope()
        {
            if (_scope != null)
            {
                return _scope;
            }
            var projectScopeReference = Resources.Load($"Assets/Resources/ProjectScopeReference.asset") as ProjectScopeReference;
            if (projectScopeReference == null)
            {
                throw new Exception("ProjectScopeReference was not found. Please check the ProjectSettings.");
            }
            _scope = projectScopeReference.CreateProjectScope();
            return _scope;
        }

        internal static NestedScopes NestedScopes
        {
            get
            {
                _nestedScopes ??= new NestedScopes(new Dictionary<ScopeId, IScope>());
                return _nestedScopes;
            }
        }
        private static NestedScopes _nestedScopes;
        internal static Modules Modules
        {
            get
            {
                _modules ??= new Modules(new Dictionary<ModuleRelation, object>(), NestedScopes);
                return _modules;
            }
        }
        private static Modules _modules;
        
        internal static object PopConfig()
        {
            var temp = _tempConfig;
            _tempConfig = null;
            return temp;
        }
        internal static void PushConfig(object config)
        {
            _tempConfig = config;
        }
        private static object _tempConfig = null;
        
        protected void Awake()
        {
            base.Awake();
            _scope = this;
            if (gameObject.scene.name != "DontDestroyOnLoad")
                throw new ConstructException("ProjectScope must not be in a scene");
        }
        
        public void Start()
        {
            InitializeAsyncInternal(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void OnDestroy()
        {
            _nestedScopes = null;
            _modules = null;
            _scope = null;
            _tempConfig = null;
        }
        
        protected override async UniTask InitializeAsyncInternal(CancellationToken cancellationToken)
        {
            await InitializeAsync(cancellationToken);

            var orderedAllNestedScope = NestedScopes.GetOrderedAllNestedScope(Scope);
            foreach (var initGroup in orderedAllNestedScope)
            {
                await UniTask.WhenAll(
                    initGroup
                        .Where(scope => (ProjectScope)scope != this)
                        .Cast<IInternalAsyncInitializer>()
                        .Select(initializer => initializer.InitializeAsyncInternal(cancellationToken))
                );
            }
        }

        protected abstract UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}