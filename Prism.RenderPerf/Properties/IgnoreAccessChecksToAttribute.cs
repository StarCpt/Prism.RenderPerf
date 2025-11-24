[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("Sandbox.Game")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("VRage.Render")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("VRage.Render11")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("VRage.Platform.Windows")]

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class IgnoresAccessChecksToAttribute : Attribute
    {
        internal IgnoresAccessChecksToAttribute(string assemblyName)
        {
        }
    }
}
