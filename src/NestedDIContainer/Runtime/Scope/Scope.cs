namespace TanitakaTech.NestedDIContainer
{
    public abstract class Scope : Scope<Scope.EmptyConfig>
    {
        public abstract class EmptyConfig {}

        protected override void Construct(DependencyBinder dependencyBinder, EmptyConfig config)
        {
            Construct(dependencyBinder);
        }
        protected abstract void Construct(DependencyBinder dependencyBinder);
    }
    
    public abstract class Scope<TConfig> : IScope
        where TConfig : class
    {
        public ScopeId? ParentScopeId { get; set; }
        public ScopeId ScopeId { get; set; }

        void IScope.Construct(DependencyBinder binder, object config)
        {
            Construct(binder, (TConfig)config);
        }
        protected abstract void Construct(DependencyBinder dependencyBinder, TConfig config);
        void IScope.Initialize() => Initialize();
        protected virtual void Initialize() {}
    }
}