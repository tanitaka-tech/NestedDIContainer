using System;

namespace NestedDIContainer.Unity.Runtime
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute : Attribute { }
}