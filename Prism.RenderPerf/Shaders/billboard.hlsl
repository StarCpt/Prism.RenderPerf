#include "common.hlsli"
#include <Math/Math.hlsli>
#include <Shadows/Csm.hlsli>
#include <Lighting/EnvAmbient.hlsli>
#include <Transparent/OIT/Globals.hlsli>

#if defined(SINGLE_CHANNEL)
Texture2D<float> billboardTex : register(t0);
#else
Texture2D<float4> billboardTex : register(t0);
#endif

Texture2D<float> DepthTexture : register(t1);

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

#if defined(LIT_PARTICLE)
float3 GetVertexLight(const float3 pos)
{
    float3 V = normalize(-pos);
    return CalculateShadowFast(pos) + AmbientDiffuse(V, -1) * frame_.Light.ambientDiffuseFactor;
}
#endif

// TODO: eliminate duplicated code
void vs_quad(uint vertexId : SV_VertexID, VS_INPUT_QUAD input, out VS_Output result)
{
    result.Position = mul(ViewProjections[input.ViewProjId], float4(input.Vertices[vertexId].xyz, 1));
    result.WorldPos = input.Vertices[vertexId].xyz;
    result.UV = input.UVOffset + (UVTable[vertexId] * input.UVSize);
    
    float2 matUVStart = Material.UnpackUVOffset();
    float2 matUVEnd = matUVStart + Material.UnpackUVSize();
    result.UV = lerp(matUVStart, matUVEnd, result.UV);
    
    result.Color = input.Color;
    result.Reflectivity = input.Reflectivity;
    result.AlphaCutout = input.AlphaCutout;
    result.SoftParticleDistanceScale = input.SoftParticleDistanceScale * Material.SoftParticleDistanceScale;
    
    float3 v0 = input.Vertices[0];
    float3 v1 = input.Vertices[1];
    float3 v2 = input.Vertices[2];
    result.Normal = normalize(cross(v1 - v0, v2 - v0));
    
#if defined(LIT_PARTICLE)
    result.Light = GetVertexLight(input.Vertices[vertexId].xyz);
#endif
}

void vs_tri(uint vertexId : SV_VertexID, VS_INPUT_TRI input, out VS_Output result)
{
    result.Position = mul(ViewProjections[input.ViewProjId], float4(input.Vertices[vertexId].xyz, 1));
    result.WorldPos = input.Vertices[vertexId].xyz;
    result.UV = input.Texcoords[vertexId];
    
    float2 matUVStart = Material.UnpackUVOffset();
    float2 matUVEnd = matUVStart + Material.UnpackUVSize();
    result.UV = lerp(matUVStart, matUVEnd, result.UV);
    
    result.Color = input.Color;
    result.Reflectivity = input.Reflectivity;
    result.AlphaCutout = input.AlphaCutout;
    result.SoftParticleDistanceScale = input.SoftParticleDistanceScale * Material.SoftParticleDistanceScale;
    
    result.Normal = normalize(input.Normal);
    
#if defined(LIT_PARTICLE)
    result.Light = GetVertexLight(input.Vertices[vertexId].xyz);
#endif
}

void vs_point(uint vertexId : SV_VertexID, VS_INPUT_POINT input, out VS_Output result)
{
    float3 worldPos = GetPointQuadVertex(vertexId, input.Position, input.Radius, input.Angle);
    result.Position = mul(ViewProjections[input.ViewProjId], float4(worldPos, 1));
    result.WorldPos = worldPos;
    result.UV = input.UVOffset + (UVTable[vertexId] * input.UVSize);
    
    float2 matUVStart = Material.UnpackUVOffset();
    float2 matUVEnd = matUVStart + Material.UnpackUVSize();
    result.UV = lerp(matUVStart, matUVEnd, result.UV);
    
    result.Color = input.Color;
    result.Reflectivity = input.Reflectivity;
    result.AlphaCutout = input.AlphaCutout;
    result.SoftParticleDistanceScale = input.SoftParticleDistanceScale * Material.SoftParticleDistanceScale;
    
    float3 v0 = GetPointQuadVertex(0, input.Position, input.Radius, input.Angle);
    float3 v1 = GetPointQuadVertex(1, input.Position, input.Radius, input.Angle);
    float3 v2 = GetPointQuadVertex(2, input.Position, input.Radius, input.Angle);
    result.Normal = normalize(cross(v1 - v0, v2 - v0));
    
#if defined(LIT_PARTICLE)
    result.Light = GetVertexLight(worldPos);
#endif
}

void vs_line(uint vertexId : SV_VertexID, VS_INPUT_LINE input, out VS_Output result)
{
    float3 worldPos = GetLineQuadVertex(vertexId, input.Origin, input.Direction, input.Length, input.Thickness);
    result.Position = mul(ViewProjections[input.ViewProjId], float4(worldPos, 1));
    result.WorldPos = worldPos;
    result.UV = input.UVOffset + (UVTable[vertexId] * input.UVSize);
    
    float2 matUVStart = Material.UnpackUVOffset();
    float2 matUVEnd = matUVStart + Material.UnpackUVSize();
    result.UV = lerp(matUVStart, matUVEnd, result.UV);
    
    result.Color = input.Color;
    result.Color.xyz *= GetLineColorMulti(input.Origin, input.Direction);
    result.Reflectivity = input.Reflectivity;
    result.AlphaCutout = input.AlphaCutout;
    result.SoftParticleDistanceScale = input.SoftParticleDistanceScale * Material.SoftParticleDistanceScale;
    
    float3 v0 = GetLineQuadVertex(0, input.Origin, input.Direction, input.Length, input.Thickness);
    float3 v1 = GetLineQuadVertex(1, input.Origin, input.Direction, input.Length, input.Thickness);
    float3 v2 = GetLineQuadVertex(2, input.Origin, input.Direction, input.Length, input.Thickness);
    result.Normal = normalize(cross(v1 - v0, v2 - v0));
    
#if defined(LIT_PARTICLE)
    result.Light = GetVertexLight(worldPos);
#endif
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
    float linearSceneDepth = linearize_depth(DepthTexture[input.Position.xy], frame_.Environment.projection_matrix);
    float linearParticleDepth = linearize_depth(input.Position.z, frame_.Environment.projection_matrix);
    float softParticleFade = CalcSoftParticle(input.SoftParticleDistanceScale, linearSceneDepth, linearParticleDepth);
    color *= softParticleFade;
#endif
    
    if (input.Reflectivity > 0)
    {
        float3 N = input.Normal;
        float3 viewVector = normalize(-input.WorldPos);
        
        float3 reflectionSample = AmbientSpecularBillboard(0.04f, 0.95f, N, viewVector, -1);
        float3 reflectionColor = lerp(color.xyz, reflectionSample, input.Reflectivity);
        
        color = float4(reflectionColor, max(color.w, input.Reflectivity));
    }
    
#if defined(LIT_PARTICLE)
    color.xyz *= input.Light;
#endif
    
#if defined(OIT)
    float linearDepth = linearize_depth(input.Position.z, frame_.Environment.projection_matrix);
    WeightedOITCendos(color, linearDepth, input.Position.z, 1, color, coverageTarget);
#endif
    
    return color;
}