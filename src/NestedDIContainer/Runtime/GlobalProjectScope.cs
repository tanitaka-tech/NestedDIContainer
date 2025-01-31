using System.Collections.Generic;

namespace TanitakaTech.NestedDIContainer
{
    public static class GlobalProjectScope
    {
        public static Dictionary<ScopeId, IScope> Scopes
        {
            get
            {
                _scopes ??= new Dictionary<ScopeId, IScope>();
                return _scopes;
            }
        }
        private static Dictionary<ScopeId, IScope> _scopes;

        public static Modules Modules
        {
            get
            {
                _modules ??= new Modules(new Dictionary<ModuleRelation, object>(), Scopes);
                return _modules;
            }
        }
        private static Modules _modules;

        public static void Dispose()
        {
            _scopes = null;
            _modules = null;
        }
    }
}