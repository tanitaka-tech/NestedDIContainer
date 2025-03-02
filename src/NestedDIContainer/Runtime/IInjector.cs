namespace TanitakaTech.NestedDIContainer
{
    public interface IInjector
    {
        void Inject(object injectableObject, IScope scope);
    }
}