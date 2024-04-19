using System;
using UnityEngine.Scripting;

namespace NestedDIContainer.Unity.Runtime
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class InjectAttribute : PreserveAttribute { }
}