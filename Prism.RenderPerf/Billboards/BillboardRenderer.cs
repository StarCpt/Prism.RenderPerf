using Prism.Common;
using Prism.Maths;
using Prism.RenderPerf.ShaderTypes;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VRage.FileSystem;
using VRage.Render11.Common;
using VRage.Render11.RenderContext;
using VRage.Render11.Resources;
using VRage.Utils;
using VRageMath;
using VRageRender;

namespace Prism.RenderPerf.Billboards;

public static class BillboardRenderer
{
    const int NUM_RENDER_PASSES = 5;
    const int CUSTOM_VIEWPROJ_COUNT = 32;

    static readonly BillboardRenderGroup[] _renderGroups = new BillboardRenderGroup[NUM_RENDER_PASSES];

    static readonly Dictionary<MyStringId, Material> _materials = new(MyStringId.Comparer);
    static readonly HashSet<MyStringId> _frameMaterials = new(MyStringId.Comparer);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    static IConstantBuffer _cbv;
    static IConstantBuffer _cbvMaterials;
    static IIndexBuffer _indices; // for tri use first 3 indices

    static VertexShader _vsQuad, _vsQuadLit;
    static VertexShader _vsTri, _vsTriLit;
    static VertexShader _vsPoint, _vsPointLit;
    static VertexShader _vsLine, _vsLineLit;
    // render passes
    // flags
    static PixelShader[,] _pixelShaders = new PixelShader[NUM_RENDER_PASSES, (int)BillboardFlags.MAX_NUM];
    static InputLayout _ilQuad;
    static InputLayout _ilTri;
    static InputLayout _ilPoint;
    static InputLayout _ilLine;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static unsafe void Init()
    {
        for (int i = 0; i < _renderGroups.Length; i++)
        {
            _renderGroups[i] = new BillboardRenderGroup((MyBillboard.BlendTypeEnum)i);
        }

        _cbv = MyManagers.Buffers.CreateConstantBuffer("Prism.RenderPerf.Billboards.FrameConstants", sizeof(FrameConstants) + sizeof(float4x4) * 33, usage: ResourceUsage.Dynamic, isGlobal: true);

        Span<ushort> indices = stackalloc ushort[6]
        {
            0, 1, 2, 2, 3, 0,
        };
        fixed (ushort* ptr = indices)
        {
            _indices = MyManagers.Buffers.CreateIndexBuffer("Prism.RenderPerf.Billboards.Indices", 6, (IntPtr)ptr, MyIndexBufferFormat.UShort, ResourceUsage.Immutable, true);
        }

        ReloadShadersInternal();
    }

    private static void ReloadShadersInternal()
    {
        _ilQuad?.Dispose();
        _ilTri?.Dispose();
        _ilPoint?.Dispose();
        _ilLine?.Dispose();

        _vsQuad?.Dispose();
        _vsTri?.Dispose();
        _vsPoint?.Dispose();
        _vsLine?.Dispose();
        _vsQuadLit?.Dispose();
        _vsTriLit?.Dispose();
        _vsPointLit?.Dispose();
        _vsLineLit?.Dispose();

        foreach (var ps in _pixelShaders)
            ps?.Dispose();

        var compiler = new FileShaderCompiler(Plugin.ShaderDirectory, Path.Combine(MyFileSystem.ShadersBasePath, "Shaders"));
        _vsQuad = compiler.CompileVertex(MyRender11.DeviceInstance, "billboard.hlsl", "vs_quad");
        _vsTri = compiler.CompileVertex(MyRender11.DeviceInstance, "billboard.hlsl", "vs_tri");
        _vsPoint = compiler.CompileVertex(MyRender11.DeviceInstance, "billboard.hlsl", "vs_point");
        _vsLine = compiler.CompileVertex(MyRender11.DeviceInstance, "billboard.hlsl", "vs_line");
        _vsQuadLit = compiler.CompileVertex(MyRender11.DeviceInstance, "billboard.hlsl", "vs_quad", new ShaderMacro("LIT_PARTICLE", null));
        _vsTriLit = compiler.CompileVertex(MyRender11.DeviceInstance, "billboard.hlsl", "vs_tri", new ShaderMacro("LIT_PARTICLE", null));
        _vsPointLit = compiler.CompileVertex(MyRender11.DeviceInstance, "billboard.hlsl", "vs_point", new ShaderMacro("LIT_PARTICLE", null));
        _vsLineLit = compiler.CompileVertex(MyRender11.DeviceInstance, "billboard.hlsl", "vs_line", new ShaderMacro("LIT_PARTICLE", null));

        var defines = new List<ShaderMacro>();
        for (int pass = 0; pass < NUM_RENDER_PASSES; pass++)
        {
            for (BillboardFlags flag = 0; flag < BillboardFlags.MAX_NUM; flag++)
            {
                defines.Add(new ShaderMacro("BLEND_ENUM", pass));

                if ((flag & BillboardFlags.SingleChannel) != 0)
                    defines.Add(new ShaderMacro("SINGLE_CHANNEL", null));
                if ((flag & BillboardFlags.AlphaCutout) != 0)
                    defines.Add(new ShaderMacro("ALPHA_CUTOUT", null));
                if ((flag & BillboardFlags.OIT) != 0)
                    defines.Add(new ShaderMacro("OIT", null));
                if ((flag & BillboardFlags.SoftParticle) != 0)
                    defines.Add(new ShaderMacro("SOFT_PARTICLE", null));
                if ((flag & BillboardFlags.LitParticle) != 0)
                    defines.Add(new ShaderMacro("LIT_PARTICLE", null));

                _pixelShaders[pass, (int)flag] = compiler.CompilePixel(MyRender11.DeviceInstance, "billboard.hlsl", "ps", defines.ToArray());
                defines.Clear();
            }
        }

        _ilQuad = new InputLayout(MyRender11.DeviceInstance, compiler.CompileVertexBytecode(MyRender11.DeviceInstance, "billboard.hlsl", "vs_quad"), new InputElement[]
        {
            new InputElement("POSITION",              0, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1), // V0
            new InputElement("VIEWPROJ",              0, Format.R32_UInt,           -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("POSITION",              1, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1), // V1
            new InputElement("REFLECTIVITY",          0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("POSITION",              2, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1), // V2
            new InputElement("ALPHACUTOUT",           0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("POSITION",              3, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1), // V3
            new InputElement("SOFTPARTICLEDISTSCALE", 0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("UVOFFSET",              0, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("UVSIZE",                0, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("COLOR",                 0, Format.R16G16B16A16_Float, -1, 0, InputClassification.PerInstanceData, 1),
        });
        _ilTri = new InputLayout(MyRender11.DeviceInstance, compiler.CompileVertexBytecode(MyRender11.DeviceInstance, "billboard.hlsl", "vs_tri"), new InputElement[]
        {
            new InputElement("POSITION",              0, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1), // V0
            new InputElement("TEXCOORD",              0, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1), // UV0

            new InputElement("POSITION",              1, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1), // V1
            new InputElement("TEXCOORD",              1, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1), // UV1

            new InputElement("POSITION",              2, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1), // V2
            new InputElement("TEXCOORD",              2, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1), // UV2

            new InputElement("NORMAL",                0, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("PADDING",               0, Format.R32_UInt,           -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("VIEWPROJ",              0, Format.R32_UInt,           -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("REFLECTIVITY",          0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("ALPHACUTOUT",           0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("SOFTPARTICLEDISTSCALE", 0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("COLOR",                 0, Format.R16G16B16A16_Float, -1, 0, InputClassification.PerInstanceData, 1),
        });
        _ilPoint = new InputLayout(MyRender11.DeviceInstance, compiler.CompileVertexBytecode(MyRender11.DeviceInstance, "billboard.hlsl", "vs_point"), new InputElement[]
        {
            new InputElement("POSITION",              0, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("RADIUS",                0, Format.R16_Float,          -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("ANGLE",                 0, Format.R16_Float,          -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("VIEWPROJ",              0, Format.R32_UInt,           -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("REFLECTIVITY",          0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("ALPHACUTOUT",           0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("SOFTPARTICLEDISTSCALE", 0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("UVOFFSET",              0, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("UVSIZE",                0, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("COLOR",                 0, Format.R16G16B16A16_Float, -1, 0, InputClassification.PerInstanceData, 1),
        });
        _ilLine = new InputLayout(MyRender11.DeviceInstance, compiler.CompileVertexBytecode(MyRender11.DeviceInstance, "billboard.hlsl", "vs_line"), new InputElement[]
        {
            new InputElement("POSITION",              0, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("LENGTH",                0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("DIRECTION",             0, Format.R32G32B32_Float,    -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("THICKNESS",             0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("VIEWPROJ",              0, Format.R32_UInt,           -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("REFLECTIVITY",          0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("ALPHACUTOUT",           0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("SOFTPARTICLEDISTSCALE", 0, Format.R32_Float,          -1, 0, InputClassification.PerInstanceData, 1),

            new InputElement("UVOFFSET",              0, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("UVSIZE",                0, Format.R16G16_Float,       -1, 0, InputClassification.PerInstanceData, 1),
            new InputElement("COLOR",                 0, Format.R16G16B16A16_Float, -1, 0, InputClassification.PerInstanceData, 1),
        });
    }

    public static void OnSessionEnd()
    {
        _materials.Clear();
    }

    public static void Gather(MyRenderContext rc, bool immediateContext)
    {
        foreach (var group in _renderGroups)
        {
            group.Clear();
        }

        // add all billboards
        {
            foreach (var bb in MyRenderProxy.BillboardsRead)
            {
                AddToGroup(bb);
            }

            int billboardsOncePoolCount = MyBillboardRenderer.m_billboardsOncePool.GetAllocatedCount();
            for (int i = 0; i < billboardsOncePoolCount; i++)
            {
                AddToGroup(MyBillboardRenderer.m_billboardsOncePool.GetAllocatedItem(i));
            }

            MyRenderProxy.ApplyActionOnPersistentBillboards(AddToGroup);
        }

        LoadMaterials(rc);
        UploadData(rc);

        static void AddToGroup(MyBillboard billboard)
        {
            if (billboard.Material != MyStringId.NullOrEmpty)
                _renderGroups[(int)billboard.BlendType].Add(billboard);
        }
    }

    private static unsafe void LoadMaterials(MyRenderContext rc)
    {
        _frameMaterials.Clear();

        bool newMaterialsAdded = false;
        foreach (var group in _renderGroups)
        {
            if (group.TotalBillboardCount == 0)
                continue;

            foreach (var (materialId, batch) in group.Batches)
            {
                if (batch.BillboardCount == 0)
                    continue;

                if (!_materials.ContainsKey(materialId))
                {
                    var material = new Material(materialId, _materials.Count);
                    _materials.Add(materialId, material);
                    newMaterialsAdded = true;
                }

                _frameMaterials.Add(materialId);
            }
        }

        foreach (var id in _frameMaterials)
        {
            // textures may change after first material fetch (eg. unloaded to loaded)
            _materials[id].UpdateTexture();
        }

        if (newMaterialsAdded)
        {
            int alignedStride = Align(sizeof(MaterialInfo), 16 * 16);
            if (_cbvMaterials is null)
            {
                int initialElementCount = Math.Max(_materials.Count, 512);
                _cbvMaterials = MyManagers.Buffers.CreateConstantBuffer("Prism.RenderPerf.Billboards.MaterialInfos", alignedStride * initialElementCount, usage: ResourceUsage.Dynamic, isGlobal: true);
            }
            else if (_cbvMaterials.ByteSize < alignedStride * _materials.Count)
            {
                MyManagers.Buffers.Resize(_cbvMaterials, _materials.Count, newByteStride: alignedStride);
            }

            using var mapping = _cbvMaterials.MapWriteDiscard(rc);
            foreach (var material in _materials.Values)
            {
                mapping.Write(in material.Info, alignedStride * material.MaterialInfoIndex);
            }
        }
    }

    private static int Align(int unaligned, int alignment)
    {
        checked
        {
            return unaligned + (alignment - 1) & ~(alignment - 1);
        }
    }

    private static void UploadData(MyRenderContext rc)
    {
        for (int i = 0; i < NUM_RENDER_PASSES; i++)
        {
            _renderGroups[i].UploadData(rc);
        }

        UploadFrameConstants(rc);
    }

    private static void UploadFrameConstants(MyRenderContext rc)
    {
        using (var mapping = _cbv.MapWriteDiscard(rc))
        {
            var matrices = MyRender11.Environment.Matrices;
            var frameData = new FrameConstants
            {
                ViewMatrixAt0 = matrices.ViewAt0,
                ProjMatrix = matrices.Projection,
            };
            mapping.WriteAndPosition(ref frameData);

            Vector2 viewport = MyRender11.ViewportResolutionF;
            for (int i = -1; i < NUM_RENDER_PASSES; i++)
            {
                if (i == -1)
                    mapping.WriteAndPosition(ref matrices.ViewProjectionAt0);
                else
                {
                    if (MyRenderProxy.BillboardsViewProjectionRead.TryGetValue(i, out var data))
                    {
                        float widthRatio = data.Viewport.Width / viewport.X;
                        float heightRatio = data.Viewport.Height / viewport.Y;
                        Matrix proj2 = new Matrix(
                            widthRatio, 0f, 0f, 0f, 0f, heightRatio, 0f, 0f, 0f, 0f, 1f, 0f,
                            data.Viewport.OffsetX / viewport.X,
                            (viewport.Y - data.Viewport.OffsetY - data.Viewport.Height) / viewport.Y,
                            0f, 1f);
                        Matrix viewProj = Matrix.Transpose(data.ViewAtZero * data.Projection * proj2);
                        mapping.WriteAndPosition(ref viewProj);
                    }
                    else
                    {
                        mapping.WriteAndPosition(ref Matrix.Identity);
                    }
                }
            }
        }
    }

    public static void RenderAdditiveBottom(MyRenderContext rc, ISrvBindable depthRead)
    {
        rc.SetBlendState(MyBlendStateManager.BlendAdditive);
        rc.SetDepthStencilState(MyDepthStencilStateManager.DefaultDepthState);

        rc.PixelShader.SetSrv(4, null);
        rc.SetRtv(MyGBuffer.Main.ResolvedDepthStencil.DsvRoDepth, MyGBuffer.Main.LBuffer);
        rc.SetScreenViewport();

        Render(rc, depthRead, MyBillboard.BlendTypeEnum.AdditiveBottom, false, true);
        rc.SetRtvNull();
    }

    public static void RenderAdditiveTop(MyRenderContext rc)
    {
        rc.SetBlendState(MyBlendStateManager.BlendAdditive);
        rc.SetDepthStencilState(MyDepthStencilStateManager.IgnoreDepthStencil);
        rc.SetRtv(MyGBuffer.Main.LBuffer);
        rc.SetScreenViewport();

        Render(rc, null, MyBillboard.BlendTypeEnum.AdditiveTop, false, true);
        rc.SetRtvNull();
    }

    public static void RenderLDR(MyRenderContext rc, ISrvBindable depthRead, IRtvBindable target)
    {
        rc.SetViewport(0f, 0f, target.Size.X, target.Size.Y);
        rc.SetBlendState(MyBlendStateManager.BlendAlphaPremult);
        rc.SetDepthStencilState(MyDepthStencilStateManager.DefaultDepthState);
        rc.SetRtv(MyGBuffer.Main.ResolvedDepthStencil.DsvRoDepth, target);

        Render(rc, depthRead, MyBillboard.BlendTypeEnum.LDR, false, false);
        rc.SetRtvNull();
    }

    public static void RenderPostPP(MyRenderContext rc, ISrvBindable depthRead, IRtvBindable target)
    {
        rc.SetViewport(0f, 0f, target.Size.X, target.Size.Y);
        rc.SetBlendState(MyBlendStateManager.BlendAlphaPremult);
        rc.SetDepthStencilState(MyDepthStencilStateManager.DefaultDepthState);
        rc.SetRtv(MyGBuffer.Main.ResolvedDepthStencil.DsvRoDepth, target);

        Render(rc, depthRead, MyBillboard.BlendTypeEnum.PostPP, false, false);
        rc.SetRtvNull();
    }

    public static void RenderStandard(MyRenderContext rc, ISrvBindable depthRead)
    {
        rc.SetDepthStencilState(MyDepthStencilStateManager.DefaultDepthState);
        rc.SetScreenViewport();

        Render(rc, depthRead, MyBillboard.BlendTypeEnum.Standard, true, true);
    }

    private static unsafe void Render(MyRenderContext rc, ISrvBindable? depthRead, MyBillboard.BlendTypeEnum blendType, bool oit, bool lit)
    {
        oit &= MyRender11.DebugOverrides.OIT;

        var group = _renderGroups[(int)blendType];
        if (group.TotalBillboardCount is 0)
            return;

        BindCommonResources(rc);
        rc.SetVertexBuffer(0, group.InstanceBuffer);
        rc.PixelShader.SetSrv(1, depthRead);

        BillboardFlags globalFlags = BillboardFlags.None;
        globalFlags |= oit ? BillboardFlags.OIT : BillboardFlags.None;
        globalFlags |= depthRead != null ? BillboardFlags.SoftParticle : BillboardFlags.None;

        int instanceOffset = 0;
        // needs to be ordered for correct render order
        // some mods like BuildInfo depend on this behavior to render ui elements properly
        foreach (var kv in group.Batches.Where(i => i.Value.BillboardCount > 0).OrderByDescending(i => i.Key.Id))
        {
            var batch = kv.Value;
            if (_materials.TryGetValue(kv.Key, out var material) && material.Texture is not null)
            {
                // material info cbv
                int materialCbvStride = Align(sizeof(MaterialInfo), 16 * 16);
                rc.AllShaderStages.SetConstantBuffer(2, _cbvMaterials, materialCbvStride * material.MaterialInfoIndex, materialCbvStride);

                // texture
                rc.PixelShader.SetSrv(0, material.Texture);

                BillboardFlags flags = globalFlags | material.Flags;
                rc.PixelShader.Set(_pixelShaders[(int)blendType, (int)flags]);

                if (batch.Quads.Count > 0)
                {
                    rc.SetInputLayout(_ilQuad);
                    rc.VertexShader.Set(lit && material.Lit ? _vsQuadLit : _vsQuad);
                    rc.DrawIndexedInstanced(6, batch.Quads.Count, 0, 0, instanceOffset);
                    instanceOffset += batch.Quads.Count;
                }

                if (batch.Triangles.Count > 0)
                {
                    rc.SetInputLayout(_ilTri);
                    rc.VertexShader.Set(lit && material.Lit ? _vsTriLit : _vsTri);
                    rc.DrawIndexedInstanced(3, batch.Triangles.Count, 0, 0, instanceOffset);
                    instanceOffset += batch.Triangles.Count;
                }

                if (batch.Points.Count > 0)
                {
                    rc.SetInputLayout(_ilPoint);
                    rc.VertexShader.Set(lit && material.Lit ? _vsPointLit : _vsPoint);
                    rc.DrawIndexedInstanced(6, batch.Points.Count, 0, 0, instanceOffset);
                    instanceOffset += batch.Points.Count;
                }

                if (batch.Lines.Count > 0)
                {
                    rc.SetInputLayout(_ilLine);
                    rc.VertexShader.Set(lit && material.Lit ? _vsLineLit : _vsLine);
                    rc.DrawIndexedInstanced(6, batch.Lines.Count, 0, 0, instanceOffset);
                    instanceOffset += batch.Lines.Count;
                }
            }
            else
            {
                instanceOffset += batch.BillboardCount;
            }
        }

        rc.SetRasterizerState(null);
    }

    private static void BindCommonResources(MyRenderContext rc)
    {
        rc.SetRasterizerState(MyRasterizerStateManager.NocullRasterizerState);

        rc.AllShaderStages.SetConstantBuffer(1, _cbv);
        rc.VertexShader.SetConstantBuffer(0, MyCommon.FrameConstants);
        rc.AllShaderStages.SetConstantBuffer(4, MyManagers.Shadows.ShadowCascades.CascadeConstantBuffer);

        rc.VertexShader.SetSamplers(0, MySamplerStateManager.StandardSamplers);
        rc.PixelShader.SetSamplers(0, MySamplerStateManager.StandardSamplers);
        rc.VertexShader.SetSampler(15, MySamplerStateManager.Shadowmap);

        rc.VertexShader.SetSrv(16, MyManagers.Shadows.ShadowCascades.CascadeShadowmapArray);
        rc.VertexShader.SetSrv(2, MyEyeAdaptation.GetExposure());
        rc.PixelShader.SetSrv(11, MyManagers.EnvironmentProbe.CloseCubemapFinal);
        rc.PixelShader.SetSrv(17, MyManagers.EnvironmentProbe.FarCubemapFinal);

        rc.SetIndexBuffer(_indices);
        rc.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
    }

    public static void ReloadShaders()
    {
        MyRender11.EnqueueUpdate(ReloadShadersInternal);
    }
}
