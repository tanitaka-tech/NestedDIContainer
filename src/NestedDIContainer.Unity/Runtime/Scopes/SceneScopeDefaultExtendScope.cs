using NestedDIContainer.Unity.Runtime;
using TanitakaTech.NestedDIContainer;

namespace src.NestedDIContainer.Unity.Runtime.Scopes
{
    public class SceneScopeDefaultExtendScope : IExtendScope
    {
        private ISceneScopeLoader SceneScopeLoader { get; }

        public SceneScopeDefaultExtendScope(ISceneScopeLoader sceneScopeLoader)
        {
            SceneScopeLoader = sceneScopeLoader;
        }

        void IExtendScope.Construct(DependencyBinder binder)
        {
            binder.Bind<ISceneScopeLoader>(SceneScopeLoader);
        }
    }
}