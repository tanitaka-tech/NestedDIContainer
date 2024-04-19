using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using TanitakaTech.NestedDIContainer;
using UnityEngine;
using Binder = TanitakaTech.NestedDIContainer.Binder;

namespace NestedDIContainer.Unity.Runtime
{
    public abstract class MonoBehaviourScope : MonoBehaviourScope<MonoBehaviourScope.EmptyConfig>
    {
        public abstract class EmptyConfig { }
        protected override void Construct(Binder binder, EmptyConfig config) => Construct(binder);
        protected abstract void Construct(Binder binder);
    }
    
    public abstract class MonoBehaviourScope<TConfig> : MonoBehaviour,
        IScope,
        IInternalAsyncInitializer
        where TConfig : class
    {
        ScopeId? IScope.ScopeId => _scopeId;
        private ScopeId? _scopeId = null;
        ScopeId? IScope.ParentScopeId => _parentScopeId;
        private ScopeId? _parentScopeId = null;
        
        private const BindingFlags MemberBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        
        protected void Awake()
        {
            _scopeId = ScopeId.Create();
            var parentScope = gameObject.GetComponentInParent<IScope>() ?? ProjectScope.Scope ?? ProjectScope.CreateProjectScope();
            _parentScopeId =_scopeId.Equals(parentScope.ScopeId.Value) ? ScopeId.Create() : parentScope.ScopeId;
            
            // Inject
            var fields = GetType().GetFields(MemberBindingFlags);
            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    field.SetValue(this, ProjectScope.Modules.Resolve(field.FieldType, _scopeId.Value));
                }
            }
            var props = GetType().GetProperties(MemberBindingFlags);
            foreach (var prop in props)
            {
                var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    prop.SetValue(this, ProjectScope.Modules.Resolve(prop.PropertyType, _scopeId.Value));
                }
            }
            
            // Add Scope
            ProjectScope.NestedScopes.Add(_scopeId.Value, this);
            this.GetCancellationTokenOnDestroy().Register(() =>
            {
                ProjectScope.NestedScopes.Remove(_scopeId.Value);
            });
            
            // Bind
            var boundTypes = new System.Collections.Generic.List<System.Type>();
            var binder = new Binder(ProjectScope.Modules, _scopeId.Value, boundTypes);
            Construct(binder, (TConfig)ProjectScope.PopConfig());
            this.GetCancellationTokenOnDestroy().Register(() =>
            {
                boundTypes.ForEach(type => ProjectScope.Modules.Remove(_scopeId.Value, type));
            });
        }

        void IScope.Construct(Binder binder, object config, ScopeId? parentScopeId)
        {
            _parentScopeId = parentScopeId;
            Construct(binder, (TConfig)config);
        }
        protected abstract void Construct(Binder binder, TConfig config);
        UniTask IInternalAsyncInitializer.InitializeAsyncInternal(CancellationToken cancellationToken)
        {
            return InitializeAsyncInternal(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy()).Token);
        }

        protected abstract UniTask InitializeAsyncInternal(CancellationToken cancellationToken);
    }
    
    internal interface IInternalAsyncInitializer
    {
        UniTask InitializeAsyncInternal(CancellationToken cancellationToken);
    }
}