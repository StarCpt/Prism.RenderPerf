using HarmonyLib;
using Prism.Vanilla.Billboard.Patches;
using System.Reflection;
using VRage.Input;
using VRage.Plugins;

namespace Prism.Vanilla.Billboard;

public class Plugin : IPlugin
{
    public Plugin()
    {
        new Harmony(GetType().FullName).PatchAll(Assembly.GetExecutingAssembly());
    }

    public void Init(object gameInstance)
    {
    }

    public void Update()
    {
        if (MyInput.Static.IsAnyShiftKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.OemPipe))
        {
            Patch_MyBillboardRenderer.ReloadShaders();
        }
    }

    public void Dispose()
    {
    }
}
