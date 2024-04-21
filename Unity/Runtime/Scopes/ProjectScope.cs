﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TanitakaTech.NestedDIContainer;
using UnityEngine;

namespace NestedDIContainer.Unity.Runtime
{
    public abstract class ProjectScope : MonoBehaviourScope
    {
        internal static ProjectScope Scope => _scope;
        private static ProjectScope _scope;
        internal ScopeId ScopeId => _scopeId;
        private ScopeId _scopeId;

        internal static ProjectScope CreateProjectScope()
        {
            if (_scope != null)
            {
                return _scope;
            }
            var loaded = Resources.Load($"ProjectScopeReference");
            var projectScopeReference = loaded as ProjectScopeReference;
            if (projectScopeReference == null)
            {
                throw new Exception("ProjectScopeReference was not found. Please check the ProjectSettings.");
            }
            _scope = projectScopeReference.CreateProjectScope();
            _scope._scopeId = ScopeId.Create();
            NestedScopes.Add(_scope._scopeId, ProjectScope._scope);
            
            var boundType = new System.Collections.Generic.List<System.Type>();
            ((IScope)_scope).Construct(new DependencyBinder(ProjectScope.Modules, _scope._scopeId, ref boundType), null);
            _scope.GetCancellationTokenOnDestroy().Register(() =>
            {
                boundType.ForEach(type => ProjectScope.Modules.Remove(_scope._scopeId, type));
                ProjectScope.NestedScopes.Remove(_scope._scopeId);
            });

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
            _scope = this;
            if (gameObject.scene.name != "DontDestroyOnLoad")
                throw new ConstructException("ProjectScope must not be in a scene");
        }

        public void OnDestroy()
        {
            _nestedScopes = null;
            _modules = null;
            _scope = null;
            _tempConfig = null;
        }
    }
}