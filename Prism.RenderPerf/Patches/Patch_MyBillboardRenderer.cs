using HarmonyLib;
using Prism.RenderPerf.Billboards;
using VRage.Render11.RenderContext;
using VRage.Render11.Resources;
using VRageRender;

namespace Prism.RenderPerf.Patches;

[HarmonyPatch(typeof(MyBillboardRenderer))]
public static class Patch_MyBillboardRenderer
{
    [HarmonyPatch(nameof(MyBillboardRenderer.OnSessionEnd))]
    [HarmonyPostfix]
    static void OnSessionEnd_Postfix()
    {
        BillboardRenderer.OnSessionEnd();
    }

    [HarmonyPatch(nameof(MyBillboardRenderer.Gather))]
    [HarmonyPrefix]
    static bool Gather_Prefix(MyRenderContext rc, bool immediateContext)
    {
        BillboardRenderer.Gather(rc, immediateContext);
        return false;
    }

    [HarmonyPatch(nameof(MyBillboardRenderer.RenderAdditiveBottom))]
    [HarmonyPrefix]
    static bool RenderAdditiveBottom_Prefix(MyRenderContext rc, ISrvBindable depthRead)
    {
        BillboardRenderer.RenderAdditiveBottom(rc, depthRead);
        return false;
    }

    [HarmonyPatch(nameof(MyBillboardRenderer.RenderAdditiveTop))]
    [HarmonyPrefix]
    static bool RenderAdditiveTop_Prefix(MyRenderContext rc)
    {
        BillboardRenderer.RenderAdditiveTop(rc);
        return false;
    }

    [HarmonyPatch(nameof(MyBillboardRenderer.RenderLDR))]
    [HarmonyPrefix]
    static bool RenderLDR_Prefix(MyRenderContext rc, ISrvBindable depthRead, IRtvBindable target)
    {
        BillboardRenderer.RenderLDR(rc, depthRead, target);
        return false;
    }

    [HarmonyPatch(nameof(MyBillboardRenderer.RenderPostPP))]
    [HarmonyPrefix]
    static bool RenderPostPP_Prefix(MyRenderContext rc, ISrvBindable depthRead, IRtvBindable target)
    {
        BillboardRenderer.RenderPostPP(rc, depthRead, target);
        return false;
    }

    [HarmonyPatch(nameof(MyBillboardRenderer.RenderStandard))]
    [HarmonyPrefix]
    static bool RenderStandard_Prefix(MyRenderContext rc, ISrvBindable depthRead)
    {
        BillboardRenderer.RenderStandard(rc, depthRead);
        return false;
    }
}
