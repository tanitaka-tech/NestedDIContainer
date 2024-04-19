using System;
using System.Collections.Generic;
using System.Linq;

namespace TanitakaTech.NestedDIContainer
{
    public class NestedScopes
    {
        private Dictionary<ScopeId, IScope> Value { get; }

        public NestedScopes(Dictionary<ScopeId, IScope> values)
        {
            Value = values;
        }
        
        public void Add(ScopeId key, IScope value)
        {
            Value.Add(key, value);
        }
        
        public void Remove(ScopeId key)
        {
            Value.Remove(key);
        }
        
        public IScope Get(ScopeId key)
        {
            return Value[key];
        }
        
        public List<List<IScope>> GetOrderedAllNestedScope(IScope rootScope)
        {
            Span<IScope> ret = new Span<IScope>();

            List<IGrouping<ScopeId, IScope>> temp = Value
                .Select(pair => pair.Value)
                .GroupBy(pair => pair.ParentScopeId.Value)
                .OrderBy(group => group.Key.Equals(rootScope.ScopeId.Value))
                .ToList();
            
            var concurrentInitGroup = new List<List<IScope>>();
            List<IScope> before = new List<IScope> { rootScope };
            while (before.Count > 0)
            {
                concurrentInitGroup.Add(before);
                
                var next = temp
                    .Where(group => group.Any(pair =>
                    {
                        return before.Any(_ => Equals(_.ScopeId.Value, pair.ParentScopeId.Value));
                    }))
                    .SelectMany(group => group)
                    .ToList();
                before = next;
            }
            
            return concurrentInitGroup;
        }
    }
}