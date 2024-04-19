using System;

namespace TanitakaTech.NestedDIContainer
{
    public abstract class Scope : Scope<Scope.EmptyConfig>
    {
        public abstract class EmptyConfig {}

        protected override void Construct(Binder binder, EmptyConfig config)
        {
            Construct(binder);
        }
        protected abstract void Construct(Binder binder);
    }
    
    public abstract class Scope<TConfig> : IScope
        where TConfig : class
    {
        ScopeId? IScope.ScopeId => _scopeId;
        private ScopeId? _scopeId = ScopeId.Create();
        ScopeId? IScope.ParentScopeId => _parentScopeId;
        private ScopeId? _parentScopeId;

        void IScope.Construct(Binder binder, object config, ScopeId? parentScopeId)
        {
            _parentScopeId = parentScopeId;
            Construct(binder, (TConfig)config);
        }
        protected abstract void Construct(Binder binder, TConfig config);
    }
}