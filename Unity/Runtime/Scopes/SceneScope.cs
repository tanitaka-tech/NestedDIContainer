using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
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
    
    public abstract class SceneScopeWithConfig<TConfig> : MonoBehaviour, IScope
    {
        [SerializeField] private List<ScriptableObjectExtendScope> _extendScopes;
        
        ScopeId? IScope.ParentScopeId => _parentScopeId;
        
        private ScopeId? _scopeId = null;
        private ScopeId? _parentScopeId = null;
        
         private const BindingFlags MemberBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        
        protected void Awake()
        {
            // Init ScopeId
            _scopeId = ScopeId.Create();
            var parentScope = ProjectScope.Scope ?? ProjectScope.CreateProjectScope();
            _parentScopeId = _scopeId.Equals(parentScope.ScopeId) ? ScopeId.Create() : parentScope.ScopeId;
            
            var boundTypes = new List<System.Type>();
            var binder = new DependencyBinder(ProjectScope.Modules, _scopeId.Value, ref boundTypes);
            
            // Construct ScriptableObjectExtendScope
            _extendScopes.ForEach(extendScope => binder.ExtendScope(extendScope));
            
            // Inject
            Inject(this, _scopeId.Value);
            
            // Add Scope
            ProjectScope.NestedScopes.Add(_scopeId.Value, this);
            
            // Bind
            Construct(binder, (TConfig)ProjectScope.PopConfig());
            
            // Initialize
            Initialize();
            
            // Unbind on destroy
            this.GetCancellationTokenOnDestroy().Register(() =>
            {
                ProjectScope.NestedScopes.Remove(_scopeId.Value);
                boundTypes.ForEach(type => ProjectScope.Modules.Remove(_scopeId.Value, type));
            });

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
                // TODO: Concurrent Inject & Construct
                childrenGroup.ForEach(child =>
                {
                    var childBoundTypes = new List<System.Type>();
                    ProjectScope.NestedScopes.Add(child.scopeId, child.scope);
                    Inject(child.scope, child.parentScopeId);
                    var childBinder = new DependencyBinder(ProjectScope.Modules, child.scopeId, ref childBoundTypes);
                    ((IScope)child.scope).Construct(childBinder, null);
                    ((IScope)child.scope).Initialize();
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
        
        void IScope.Construct(DependencyBinder binder, object config)
        {
            Construct(binder, (TConfig)config);
        }
        protected abstract void Construct(DependencyBinder binder, TConfig config);
        void IScope.Initialize() => Initialize();
        protected virtual void Initialize() {}
    }
}