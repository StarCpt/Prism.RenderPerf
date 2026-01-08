using HarmonyLib;
using Prism.RenderPerf.Billboards;
using System.IO;
using System.Reflection;
using VRage.Input;
using VRage.Plugins;

namespace Prism.RenderPerf;

public class Plugin : IPlugin
{
    public static string? ShaderDirectory { get; private set; }

    public Plugin()
    {
#if DEV
        ShaderDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Shaders");
#endif
        new Harmony(GetType().FullName).PatchAll(Assembly.GetExecutingAssembly());
    }

    public void Init(object gameInstance)
    {
        BillboardRenderer.Init();
    }

    public void LoadAssets(string path)
    {
        ShaderDirectory = path;
    }

    public void Update()
    {
#if DEBUG
        if (MyInput.Static.IsAnyShiftKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.OemPipe))
            BillboardRenderer.ReloadShaders();
#endif
    }

    public void Dispose()
    {
    }
}
