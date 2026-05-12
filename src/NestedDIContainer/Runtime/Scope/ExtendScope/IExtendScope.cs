namespace TanitakaTech.NestedDIContainer
{
    public interface IExtendScope : IInjectable
    {
        void Construct(DependencyBinder binder);
    }
}
