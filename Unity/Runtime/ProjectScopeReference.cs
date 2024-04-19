using UnityEngine;

namespace NestedDIContainer.Unity.Runtime
{
    public class ProjectScopeReference : ScriptableObject
    {
        [SerializeField] ProjectScope _projectScope;
        
        public ProjectScope ProjectScope
        {
            get => _projectScope;
            set
            {
                if (_projectScope != null && value != _projectScope)
                {
                    _projectScope = value;
                }
            }
        }
        
        internal ProjectScope CreateProjectScope()
        {
            var instance = Instantiate(_projectScope, null);
            DontDestroyOnLoad(instance);
            return instance;
        }
    }
}