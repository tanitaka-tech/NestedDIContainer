namespace TanitakaTech.NestedDIContainer
{
    public interface IScope
    {
        void Construct(DependencyBinder binder, object config);
        void Initialize();
        ScopeId ScopeId { get; set; }
        ScopeId? ParentScopeId { get; set; }
    }
}