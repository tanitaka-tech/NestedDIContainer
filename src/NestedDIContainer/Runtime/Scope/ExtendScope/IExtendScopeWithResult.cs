namespace TanitakaTech.NestedDIContainer
{
    public interface IExtendScopeWithResult<out TResult> : IExtendScope
    {
        TResult GetResult();
    }
}