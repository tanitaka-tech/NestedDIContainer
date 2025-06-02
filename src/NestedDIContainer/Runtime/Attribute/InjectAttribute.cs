using System;

namespace NestedDIContainer.Unity.Runtime
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute : Attribute { }
    
#if USE_INJECT_OPTIONAL
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectOptionalAttribute : Attribute { }
#endif
}