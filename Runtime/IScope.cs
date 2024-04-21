namespace TanitakaTech.NestedDIContainer
{
    public interface IScope
    {
        ScopeId? ParentScopeId { get; }
        void Construct(DependencyBinder binder, object config);
    }
}