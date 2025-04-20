namespace TanitakaTech.NestedDIContainer
{
    public interface IScope : IInjectable
    {
        void Construct(DependencyBinder binder, object config);
        ScopeId? ParentScopeId { get; }
        ScopeContainer ScopeContainer { get; }
    }
}