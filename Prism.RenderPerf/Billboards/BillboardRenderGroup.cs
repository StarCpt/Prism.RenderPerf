using Prism.Maths;
using Prism.RenderPerf.ShaderTypes;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Render.Scene;
using VRage.Render11.Common;
using VRage.Render11.RenderContext;
using VRage.Render11.Resources;
using VRage.Utils;
using VRageMath;
using VRageMath.PackedVector;
using VRageRender;

namespace Prism.RenderPerf.Billboards;

class BillboardRenderGroup : IDisposable
{
    public class Batch
    {
        public int BillboardCount => Quads.Count + Triangles.Count + Points.Count + Lines.Count;
        public readonly List<BillboardDataUnion> Quads = [];
        public readonly List<BillboardDataUnion> Triangles = [];
        public readonly List<BillboardDataUnion> Points = [];
        public readonly List<BillboardDataUnion> Lines = [];

        public void Clear()
        {
            Quads.ClearFast();
            Triangles.ClearFast();
            Points.ClearFast();
            Lines.ClearFast();
        }
    }

    public MyBillboard.BlendTypeEnum BlendType { get; }
    public readonly Dictionary<MyStringId, Batch> Batches = new(MyStringId.Comparer);
    public int TotalBillboardCount { get; private set; } = 0;

    public IVertexBuffer? InstanceBuffer { get; private set; }

    public BillboardRenderGroup(MyBillboard.BlendTypeEnum blendType)
    {
        BlendType = blendType;
    }

    public void Add(MyBillboard billboard)
    {
        var batch = Batches.GetValueOrNew(billboard.Material);

        if (billboard.LocalType is MyBillboard.LocalTypeEnum.Custom)
        {
            if (billboard is MyTriangleBillboard tri)
            {
                Vector3D v0 = billboard.Position0;
                Vector3D v1 = billboard.Position1;
                Vector3D v2 = billboard.Position2;

                if (billboard.ParentID != uint.MaxValue && TryGetParentMatrix(billboard, out MatrixD mat))
                {
                    Vector3D.Transform(ref v0, ref mat, out v0);
                    Vector3D.Transform(ref v1, ref mat, out v1);
                    Vector3D.Transform(ref v2, ref mat, out v2);
                }

                AddTri(ref v0, ref v1, ref v2, tri);
                TotalBillboardCount++;
            }
            else // is quad
            {
                Vector3D v0 = billboard.Position0;
                Vector3D v1 = billboard.Position1;
                Vector3D v2 = billboard.Position2;
                Vector3D v3 = billboard.Position3;

                if (billboard.ParentID != uint.MaxValue && TryGetParentMatrix(billboard, out MatrixD mat))
                {
                    Vector3D.Transform(ref v0, ref mat, out v0);
                    Vector3D.Transform(ref v1, ref mat, out v1);
                    Vector3D.Transform(ref v2, ref mat, out v2);
                    Vector3D.Transform(ref v3, ref mat, out v3);
                }

                AddQuad(ref v0, ref v1, ref v2, ref v3);
                TotalBillboardCount++;
            }
        }
        else if (billboard.LocalType is MyBillboard.LocalTypeEnum.Line)
        {
            float length = (float)billboard.Position2.X;
            float thickness = (float)billboard.Position2.Y;

            if (length == 0 || thickness == 0)
                return;

            Vector3D origin = billboard.Position0;
            Vector3 direction = billboard.Position1;

            if (billboard.ParentID != uint.MaxValue && TryGetParentMatrix(billboard, out MatrixD mat))
            {
                Vector3D.Transform(ref origin, ref mat, out origin);
                Vector3.TransformNormal(ref direction, ref mat, out direction);
            }

            Vector3D cameraPosForProj = billboard.CustomViewProjection == -1 ? MyRender11.Environment.Matrices.CameraPosition : MyRenderProxy.BillboardsViewProjectionRead[billboard.CustomViewProjection].CameraPosition;
            Vector3D.Subtract(ref origin, ref cameraPosForProj, out origin);
                
            batch.Lines.Add(new BillboardDataUnion
            {
                Line = new LineBillboardData
                {
                    Origin = (float3)origin,
                    Length = length,
                    Direction = direction,
                    Thickness = thickness,

                    UVOffset = new HalfVector2(billboard.UVOffset),
                    UVSize = new HalfVector2(billboard.UVSize),
                    Color = new HalfVector4(
                        billboard.Color.X * billboard.ColorIntensity,
                        billboard.Color.Y * billboard.ColorIntensity,
                        billboard.Color.Z * billboard.ColorIntensity,
                        billboard.Color.W),

                    CustomViewProjection = (uint)(billboard.CustomViewProjection + 1),
                    Reflectivity = billboard.Reflectivity,
                    AlphaCutout = billboard.AlphaCutout,
                    SoftParticleDistanceScale = billboard.SoftParticleDistanceScale,
                },
            });
            TotalBillboardCount++;
        }
        else if (billboard.LocalType is MyBillboard.LocalTypeEnum.Point)
        {
            Vector3D pos = billboard.Position0;
            if (billboard.ParentID != uint.MaxValue && TryGetParentMatrix(billboard, out MatrixD mat))
                Vector3D.Transform(ref pos, ref mat, out pos);

            if (billboard.CustomViewProjection == -1)
                Vector3D.Subtract(ref pos, ref MyRender11.Environment.Matrices.CameraPosition, out pos);

            batch.Points.Add(new BillboardDataUnion
            {
                Point = new PointBillboardData
                {
                    Position = (float3)pos,
                    RadiusAndAngle = new HalfVector2((float)billboard.Position2.X, (float)billboard.Position2.Y),

                    UVOffset = new HalfVector2(billboard.UVOffset),
                    UVSize = new HalfVector2(billboard.UVSize),
                    Color = new HalfVector4(
                        billboard.Color.X * billboard.ColorIntensity,
                        billboard.Color.Y * billboard.ColorIntensity,
                        billboard.Color.Z * billboard.ColorIntensity,
                        billboard.Color.W),

                    CustomViewProjection = (uint)(billboard.CustomViewProjection + 1),
                    Reflectivity = billboard.Reflectivity,
                    AlphaCutout = billboard.AlphaCutout,
                    SoftParticleDistanceScale = billboard.SoftParticleDistanceScale,
                },
            });
            TotalBillboardCount++;
        }

        void AddTri(ref Vector3D v0, ref Vector3D v1, ref Vector3D v2, MyTriangleBillboard tri)
        {
            if (billboard.CustomViewProjection == -1)
            {
                Vector3D cameraPos = MyRender11.Environment.Matrices.CameraPosition;
                Vector3D.Subtract(ref v0, ref cameraPos, out v0);
                Vector3D.Subtract(ref v1, ref cameraPos, out v1);
                Vector3D.Subtract(ref v2, ref cameraPos, out v2);
            }

            batch.Triangles.Add(new BillboardDataUnion
            {
                Tri = new TriBillboardData
                {
                    V0 = (float3)v0,
                    V1 = (float3)v1,
                    V2 = (float3)v2,

                    UV0 = new HalfVector2(tri.UV0),
                    UV1 = new HalfVector2(tri.UV1),
                    UV2 = new HalfVector2(tri.UV2),

                    Normal = tri.Normal0,

                    Color = new HalfVector4(
                        billboard.Color.X * billboard.ColorIntensity,
                        billboard.Color.Y * billboard.ColorIntensity,
                        billboard.Color.Z * billboard.ColorIntensity,
                        billboard.Color.W),

                    CustomViewProjection = (uint)(billboard.CustomViewProjection + 1),
                    Reflectivity = billboard.Reflectivity,
                    AlphaCutout = billboard.AlphaCutout,
                    SoftParticleDistanceScale = billboard.SoftParticleDistanceScale,
                },
            });
        }
        void AddQuad(ref Vector3D v0, ref Vector3D v1, ref Vector3D v2, ref Vector3D v3, float rgbMulti = 1)
        {
            if (billboard.CustomViewProjection == -1)
            {
                Vector3D cameraPos = MyRender11.Environment.Matrices.CameraPosition;
                Vector3D.Subtract(ref v0, ref cameraPos, out v0);
                Vector3D.Subtract(ref v1, ref cameraPos, out v1);
                Vector3D.Subtract(ref v2, ref cameraPos, out v2);
                Vector3D.Subtract(ref v3, ref cameraPos, out v3);
            }

            batch.Quads.Add(new BillboardDataUnion
            {
                Quad = new QuadBillboardData
                {
                    V0 = (float3)v0,
                    V1 = (float3)v1,
                    V2 = (float3)v2,
                    V3 = (float3)v3,

                    UVOffset = new HalfVector2(billboard.UVOffset),
                    UVSize = new HalfVector2(billboard.UVSize),
                    Color = new HalfVector4(
                        billboard.Color.X * billboard.ColorIntensity * rgbMulti,
                        billboard.Color.Y * billboard.ColorIntensity * rgbMulti,
                        billboard.Color.Z * billboard.ColorIntensity * rgbMulti,
                        billboard.Color.W),

                    CustomViewProjection = (uint)(billboard.CustomViewProjection + 1),
                    Reflectivity = billboard.Reflectivity,
                    AlphaCutout = billboard.AlphaCutout,
                    SoftParticleDistanceScale = billboard.SoftParticleDistanceScale,
                }
            });
        }
    }

    public static bool TryGetParentMatrix(MyBillboard billboard, out MatrixD matrix)
    {
        MyActor? parent = billboard.ParentID != uint.MaxValue ? MyIDTracker<MyActor>.FindByID(billboard.ParentID) : null;
        if (parent is null)
        {
            matrix = MatrixD.Identity;
            return false;
        }

        matrix = parent.WorldMatrix;
        return true;
    }

    public unsafe void UploadData(MyRenderContext rc)
    {
        if (TotalBillboardCount is 0)
            return;

        if (InstanceBuffer is null)
            InstanceBuffer = MyManagers.Buffers.CreateVertexBuffer($"Prism.RenderPerf.Billboards.InstanceBuffer{(int)BlendType}", TotalBillboardCount, sizeof(BillboardDataUnion), usage: ResourceUsage.Dynamic, isGlobal: true);
        else if (InstanceBuffer.ElementCount < TotalBillboardCount)
        {
            MyManagers.Buffers.Resize(InstanceBuffer, TotalBillboardCount);
        }

        using var mapping = InstanceBuffer.MapWriteDiscard(rc);
        foreach (var batch in Batches.Where(i => i.Value.BillboardCount > 0).OrderByDescending(i => i.Key.Id))
        {
            mapping.WriteAndPosition(batch.Value.Quads.AsSpan());
            mapping.WriteAndPosition(batch.Value.Triangles.AsSpan());
            mapping.WriteAndPosition(batch.Value.Points.AsSpan());
            mapping.WriteAndPosition(batch.Value.Lines.AsSpan());
        }
    }

    public void Clear()
    {
        foreach (var batch in Batches.Values)
        {
            batch.Clear();
        }
        TotalBillboardCount = 0;
    }

    public void Dispose()
    {
        Clear();
        MyManagers.Buffers.Dispose(InstanceBuffer);
    }
}
