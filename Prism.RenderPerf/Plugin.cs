using HarmonyLib;
using Prism.RenderPerf.Patches;
using System.Reflection;
using VRage.Input;
using VRage.Plugins;

namespace Prism.RenderPerf;

public class Plugin : IPlugin
{
    public static string? ShaderDirectory { get; private set; }

    public Plugin()
    {
        new Harmony(GetType().FullName).PatchAll(Assembly.GetExecutingAssembly());
    }

    public void Init(object gameInstance)
    {
        Patch_MyBillboardRenderer.Init();
    }

    public void LoadAssets(string path)
    {
        ShaderDirectory = path;
    }

    public void Update()
    {
#if DEBUG
        if (MyInput.Static.IsAnyShiftKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.OemPipe))
            Patch_MyBillboardRenderer.ReloadShaders();
#endif
    }

    public void Dispose()
    {
    }
}
