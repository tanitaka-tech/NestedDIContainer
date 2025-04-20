using System;
using System.Collections.Generic;
using System.Reflection;
using NestedDIContainer.Unity.Runtime;

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

        public static void Dispose()
        {
            _scopes = null;
        }
    }
}