using SharpDX.DXGI;
using System;
using VRage.Render11.Common;
using VRage.Render11.Resources;
using VRage.Render11.Resources.Textures;
using VRage.Render11.Sprites;
using VRage.Utils;
using VRageMath;
using VRageMath.PackedVector;
using VRageRender;

namespace Prism.RenderPerf;

// must be padded to a multiple of 256 bytes
public struct MaterialInfo
{
    public HalfVector2 UVOffset; // 4 bytes
    public HalfVector2 UVSize;   // 8 bytes

    public float AlphaSaturation; // 12 bytes
    public float SoftParticleDistanceScale; // 16 bytes

    // only UV Offset/Size, AlphaCutout, AlphaSaturation, CanBeAffectedByOtherLights, and SoftParticleDistanceScale are used
    // UseAtlas, Texture, TextureType, Id, TargetSize are used for getting the texture
}

public class Material
{
    public ITexture? Texture { get; private set; }
    public BillboardFlags Flags { get; private set; } = BillboardFlags.None;
    public bool Lit => _material.CanBeAffectedByOtherLights;
    public readonly MaterialInfo Info;
    public readonly int MaterialInfoIndex;

    private readonly MyTransparentMaterial _material;

    public Material(MyStringId materialId, int materialInfoIndex)
    {
        _material = MyTransparentMaterials.GetMaterial(materialId);
        if (_material.UseAtlas)
        {
            var atlasElement = MyBillboardRenderer.m_atlas.FindElement(_material.Texture);
            var atlasOffset = new Vector2(atlasElement.UvOffsetScale.X, atlasElement.UvOffsetScale.Y);
            var atlasScale = new Vector2(atlasElement.UvOffsetScale.Z, atlasElement.UvOffsetScale.W);
            Info = new MaterialInfo
            {
                UVOffset = new HalfVector2(atlasOffset + _material.UVOffset * atlasScale),
                UVSize = new HalfVector2(_material.UVSize * atlasScale),
                AlphaSaturation = _material.AlphaSaturation,
                SoftParticleDistanceScale = _material.SoftParticleDistanceScale,
            };
        }
        else
        {
            Info = new MaterialInfo
            {
                UVOffset = new HalfVector2(_material.UVOffset),
                UVSize = new HalfVector2(_material.UVSize),
                AlphaSaturation = _material.AlphaSaturation,
                SoftParticleDistanceScale = _material.SoftParticleDistanceScale,
            };
        }
        MaterialInfoIndex = materialInfoIndex;
        UpdateBillboardFlags();
    }

    public void UpdateTexture()
    {
        if (_material.UseAtlas)
            Texture = MyBillboardRenderer.m_atlas.FindElement(_material.Texture).Texture.Texture;
        else if (_material.TextureType is MyTransparentMaterialTextureType.FileTexture)
        {
            IMyStreamedTexture? fileTexure = null;
            if (_material.Texture != null && !MyBillboardRenderer.m_fileTextures.TryGetValue(_material.Texture, out fileTexure))
            {
                fileTexure = MyManagers.Textures.GetTexture(_material.Texture, MyFileTextureEnum.GUI);
                if (fileTexure.Texture.IsTextureLoaded())
                    MyBillboardRenderer.m_fileTextures.Add(_material.Texture, fileTexure);
                else
                {
                    fileTexure.Touch(32767);
                    fileTexure = null;
                }
            }

            if (fileTexure != null)
            {
                fileTexure.Touch(32767);
                Texture = fileTexure.Texture;
            }
            else
            {
                MyManagers.FileTextures.TryGetTexture("EMPTY", out ITexture texture);
                Texture = texture;
            }
        }
        else if (_material.TextureType is MyTransparentMaterialTextureType.RenderTarget)
        {
            MySpriteMessageData drawMessages = MyManagers.SpritesManager.AcquireDrawMessages(_material.Id.String);
            Texture = MyRender11.DrawSpritesOffscreen(drawMessages, _material.Id.String, _material.TargetSize.X, _material.TargetSize.Y);
            MyManagers.SpritesManager.DisposeDrawMessages(drawMessages);
        }
        else
        {
            throw new Exception("Invalid texture type.");
        }
        UpdateBillboardFlags();
    }

    private void UpdateBillboardFlags()
    {
        Flags = BillboardFlags.None;
        Flags |= _material.AlphaCutout ? BillboardFlags.AlphaCutout : BillboardFlags.None;
        Flags |= (Texture?.Format ?? Format.Unknown) is Format.BC4_UNorm ? BillboardFlags.SingleChannel : BillboardFlags.None;
        Flags |= _material.CanBeAffectedByOtherLights ? BillboardFlags.LitParticle : BillboardFlags.None;
    }
}
