namespace TanitakaTech.NestedDIContainer
{
    public interface IScope : IInjectable
    {
        void Construct(DependencyBinder binder, object config);
        IScope ParentScope { get; }
        ScopeContainer ScopeContainer { get; }
    }
}