struct Half2
{
    uint Packed;
    
    float2 Unpack()
    {
        return f16tof32(uint2(Packed, Packed >> 16));
    }
};

struct VS_INPUT_QUAD
{
    float3 Vertices[4] : POSITION;
    
    uint ViewProjId : VIEWPROJ;
    float Reflectivity : REFLECTIVITY;
    float AlphaCutout : ALPHACUTOUT;
    float SoftParticleDistanceScale : SOFTPARTICLEDISTSCALE;
    
    float2 UVOffset : UVOFFSET;
    float2 UVSize : UVSIZE;
    float4 Color : COLOR;
};

struct VS_INPUT_TRI
{
    float3 Vertices[3] : POSITION;
    float2 Texcoords[3] : TEXCOORD;
    
    float3 Normal : NORMAL;
    uint _pad0 : PADDING;
    
    uint ViewProjId : VIEWPROJ;
    float Reflectivity : REFLECTIVITY;
    float AlphaCutout : ALPHACUTOUT;
    float SoftParticleDistanceScale : SOFTPARTICLEDISTSCALE;
    
    float4 Color : COLOR;
};

struct VS_INPUT_POINT
{
    float3 Position : POSITION;
    float Radius : RADIUS;
    float Angle : ANGLE;
    
    uint ViewProjId : VIEWPROJ;
    float Reflectivity : REFLECTIVITY;
    float AlphaCutout : ALPHACUTOUT;
    float SoftParticleDistanceScale : SOFTPARTICLEDISTSCALE;
    
    float2 UVOffset : UVOFFSET;
    float2 UVSize : UVSIZE;
    float4 Color : COLOR;
};

struct VS_INPUT_LINE
{
    float3 Origin : POSITION;
    float Length : LENGTH;
    float3 Direction : DIRECTION;
    float Thickness : THICKNESS;
    
    uint ViewProjId : VIEWPROJ;
    float Reflectivity : REFLECTIVITY;
    float AlphaCutout : ALPHACUTOUT;
    float SoftParticleDistanceScale : SOFTPARTICLEDISTSCALE;
    
    float2 UVOffset : UVOFFSET;
    float2 UVSize : UVSIZE;
    float4 Color : COLOR;
};

struct VS_Output
{
    float4 Position : SV_Position;
    float3 WorldPos : WORLDPOS;
    float2 UV : TEXCOORD0;
    nointerpolation float4 Color : COLOR;
    nointerpolation float Reflectivity : REFLECTIVITY;
    nointerpolation float AlphaCutout : ALPHACUTOUT;
    nointerpolation float SoftParticleDistanceScale : SOFTPARTICLEDISTSCALE;
    nointerpolation float3 Normal : NORMAL;
    
#if defined(LIT_PARTICLE)
    float3 Light : LIGHT;
#endif
};

cbuffer FrameConstants : register(b1)
{
    float4x4 ViewMatrix;
    float4x4 ProjMatrix;
    
    float4x4 ViewProjections[33];
};

struct MaterialInfo
{
    uint PackedUVOffset;
    uint PackedUVSize;
    
    float AlphaSaturation;
    float SoftParticleDistanceScale;
    
    float2 UnpackUVOffset()
    {
        return ((Half2) PackedUVOffset).Unpack();
    }
    float2 UnpackUVSize()
    {
        return ((Half2) PackedUVSize).Unpack();
    }
};

cbuffer MaterialConstants : register(b2)
{
    MaterialInfo Material;
};

#define BLEND_STANDARD 0
#define BLEND_ADDITIVE_BOTTOM 1
#define BLEND_ADDITIVE_TOP 2
#define BLEND_LDR 3
#define BLEND_SDR 3
#define BLEND_POSTPP 4

#define FLOAT_EPSILON 1E-6
