namespace TanitakaTech.NestedDIContainer
{
    public interface IScope : IInjectable
    {
        void Construct(DependencyBinder binder, object config);
        ScopeId ScopeId { get; set; }
        ScopeId? ParentScopeId { get; set; }
    }
}