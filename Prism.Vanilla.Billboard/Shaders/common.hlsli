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
    float2 UV : TEXCOORD0;
    nointerpolation float4 Color : COLOR;
    nointerpolation float AlphaCutout : ALPHACUTOUT;
    nointerpolation float SoftParticleDistanceScale : SOFTPARTICLEDISTSCALE;
    
#if defined(LIT_PARTICLE)
    float3 Light : LIGHT;
#endif
};

cbuffer FrameConstants : register(b1)
{
    float4x4 ViewMatrix;
    float4x4 ProjMatrix;
    
    float3 FogColor;
    float FogDensity;
    
    float FogMultiplier;
    uint _pad1;
    uint _pad2;
    uint _pad3;
    
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

float3 Fog(float3 color, float dist)
{
    dist = clamp(dist, 0, 1000);
    float fog0 = FogMultiplier * (1 - exp(-dist * FogDensity));
    return lerp(color, FogColor, fog0);
}

void WeightedOITCendos(float4 color, float linearZ, out float4 accumTarget, out float4 coverageTarget)
{
    // clip colors below very low transparency
    clip(color.a - 0.0001f);
    
    // apply fog
    if (color.a > 0.001f)
        color.rgb = Fog(color.rgb / color.a, -linearZ) * color.a;

    // Insert your favorite weighting function here. The color-based factor
    // avoids color pollution from the edges of wispy clouds. The z-based
    // factor gives precedence to nearer surfaces.
    //float invZ = max(0.005, 1 / max(-linearZ, 1.0f) );// *color.a;// clamp(1 + linearZ / 200, 0.01, 1);
    //float invZ = clamp(1 + linearZ / 200, 0.01, 1);
    float invZ = (1 + (linearZ + 3) / 200) * color.a;
    invZ = pow(abs(invZ), 32);
    invZ = clamp(invZ, 0.01, 1);
    float weight = invZ;
    // Blend Func: ONE, ONE
    // Switch to premultiplied alpha and weight
    accumTarget = color * weight;

    // Blend Func: zero, 1-source
    coverageTarget = color.a;
}
