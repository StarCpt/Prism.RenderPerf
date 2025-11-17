#include "common.hlsli"
#include <Math/Math.hlsli>

SamplerState LinearSampler : register(s2);

#if defined(SINGLE_CHANNEL)
Texture2D<float> billboardTex : register(t1);
#else
Texture2D<float4> billboardTex : register(t1);
#endif

Texture2D<float> DepthTexture : register(t2);

static const float2 UVTable[4] =
{
    float2(0, 0),
    float2(1, 0),
    float2(1, 1),
    float2(0, 1),
};

float3 reject(float3 vec, float3 dir)
{
    float dot_dd = dot(dir, dir);
    float inv_dot_dd = rcp(dot_dd);
    
    float dot_vd = dot(dir, vec);
    dot_vd *= inv_dot_dd;
    
    return vec - dot_vd * (dir * inv_dot_dd);
}

float3 GetPointQuadVertex(const uint vertexId, const float3 pos, const float radius, const float angle)
{
    static const float3 unit_up = float3(0, 1, 0);
    static const float3 unit_forward = float3(0, 0, -1);
    
    const float3 posDir = normalize(pos);
    const float3 result = reject(unit_up, posDir);
    const float3 normalizedResult = dot(result, result) < FLOAT_EPSILON ? unit_forward : normalize(result);
    const float3 result2 = normalize(cross(normalizedResult, posDir));
    
    float cosAngle, sinAngle;
    sincos(angle, sinAngle, cosAngle);
    
    cosAngle *= radius;
    sinAngle *= radius;
    const float3 right = cosAngle * result2 + sinAngle * normalizedResult;
    const float3 up = sinAngle * result2 + cosAngle * normalizedResult;
    
    // 0,1 -> 1,-1
    const float2 multiplier = -mad(UVTable[vertexId], 2, -1);
    return pos + (right * multiplier.x) + (up * multiplier.y);
}

float3 GetLineQuadVertex(const uint vertexId, const float3 pos, const float3 dir, const float len, const float thickness)
{
    float3 right = normalize(cross(dir, -pos)) * thickness;
    return pos + (dir * len * UVTable[vertexId].x) + (right * mad(UVTable[vertexId].y, 2, -1));
}

float GetLineColorMulti(const float3 pos, const float3 dir)
{
    // weird function
    return (1 - pow(abs(dot(normalize(pos), dir)), 30)) * 0.5;
}

void vs_quad(uint vertexId : SV_VertexID, VS_INPUT_QUAD input, out VS_Output result)
{
    result.Position = mul(ViewProjections[input.ViewProjId], float4(input.Vertices[vertexId].xyz, 1));
    result.UV = input.UVOffset + (UVTable[vertexId] * input.UVSize);
    
    float2 matUVStart = Material.UnpackUVOffset();
    float2 matUVEnd = matUVStart + Material.UnpackUVSize();
    result.UV = lerp(matUVStart, matUVEnd, result.UV);
    
    result.Color = input.Color;
    result.AlphaCutout = input.AlphaCutout;
    result.SoftParticleDistanceScale = input.SoftParticleDistanceScale * Material.SoftParticleDistanceScale;
}

void vs_tri(uint vertexId : SV_VertexID, VS_INPUT_TRI input, out VS_Output result)
{
    result.Position = mul(ViewProjections[input.ViewProjId], float4(input.Vertices[vertexId].xyz, 1));
    result.UV = input.Texcoords[vertexId];
    
    float2 matUVStart = Material.UnpackUVOffset();
    float2 matUVEnd = matUVStart + Material.UnpackUVSize();
    result.UV = lerp(matUVStart, matUVEnd, result.UV);
    
    result.Color = input.Color;
    result.AlphaCutout = input.AlphaCutout;
    result.SoftParticleDistanceScale = input.SoftParticleDistanceScale * Material.SoftParticleDistanceScale;
}

void vs_point(uint vertexId : SV_VertexID, VS_INPUT_POINT input, out VS_Output result)
{
    float3 worldPos = GetPointQuadVertex(vertexId, input.Position, input.Radius, input.Angle);
    result.Position = mul(ViewProjections[input.ViewProjId], float4(worldPos, 1));
    result.UV = input.UVOffset + (UVTable[vertexId] * input.UVSize);
    
    float2 matUVStart = Material.UnpackUVOffset();
    float2 matUVEnd = matUVStart + Material.UnpackUVSize();
    result.UV = lerp(matUVStart, matUVEnd, result.UV);
    
    result.Color = input.Color;
    result.AlphaCutout = input.AlphaCutout;
    result.SoftParticleDistanceScale = input.SoftParticleDistanceScale * Material.SoftParticleDistanceScale;
}

void vs_line(uint vertexId : SV_VertexID, VS_INPUT_LINE input, out VS_Output result)
{
    float3 worldPos = GetLineQuadVertex(vertexId, input.Origin, input.Direction, input.Length, input.Thickness);
    result.Position = mul(ViewProjections[input.ViewProjId], float4(worldPos, 1));
    result.UV = input.UVOffset + (UVTable[vertexId] * input.UVSize);
    
    float2 matUVStart = Material.UnpackUVOffset();
    float2 matUVEnd = matUVStart + Material.UnpackUVSize();
    result.UV = lerp(matUVStart, matUVEnd, result.UV);
    
    result.Color = input.Color;
    result.Color.xyz *= GetLineColorMulti(input.Origin, input.Direction);
    result.AlphaCutout = input.AlphaCutout;
    result.SoftParticleDistanceScale = input.SoftParticleDistanceScale * Material.SoftParticleDistanceScale;
}

#if defined(OIT)
float4 ps(VS_Output input, out float4 coverageTarget : SV_Target1) : SV_Target0
#else
float4 ps(VS_Output input) : SV_Target0
#endif
{
    float4 color = billboardTex.Sample(LinearSampler, input.UV);
    color *= input.Color;
    
#if defined(ALPHA_CUTOUT)
    float cutout = step(input.AlphaCutout, color.w);
    color = float4(color.xyz * cutout, cutout);
#endif
    
#if defined(SOFT_PARTICLE)
    float linearSceneDepth = linearize_depth(DepthTexture[input.Position.xy], ProjMatrix);
    float linearParticleDepth = linearize_depth(input.Position.z, ProjMatrix);
    float softParticleFade = CalcSoftParticle(input.SoftParticleDistanceScale, -linearSceneDepth, -linearParticleDepth);
    color *= softParticleFade;
#endif
    
#if defined(OIT)
    float linearDepth = linearize_depth(input.Position.z, ProjMatrix);
    WeightedOITCendos(color, linearDepth, color, coverageTarget);
#endif
    
    return color;
}