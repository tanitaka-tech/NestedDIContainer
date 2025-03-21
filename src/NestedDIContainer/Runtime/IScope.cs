namespace TanitakaTech.NestedDIContainer
{
    public interface IScope : IInjector, IInjectable
    {
        void Construct(DependencyBinder binder, object config);
        ScopeId ScopeId { get; set; }
        ScopeId? ParentScopeId { get; set; }
    }
}