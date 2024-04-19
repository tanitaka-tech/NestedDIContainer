namespace TanitakaTech.NestedDIContainer
{
    public interface IScope
    {
        ScopeId? ScopeId { get; }
        ScopeId? ParentScopeId { get; }
        void Construct(Binder binder, object config, ScopeId? parentScopeId);
        
        bool WasInitialized => ScopeId != null;
    }
}