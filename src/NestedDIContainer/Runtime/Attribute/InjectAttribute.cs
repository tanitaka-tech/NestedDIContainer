using System;

namespace TanitakaTech.NestedDIContainer.Runtime
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute : Attribute { }
    
#if USE_INJECT_OPTIONAL
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectOptionalAttribute : Attribute { }
#endif
}