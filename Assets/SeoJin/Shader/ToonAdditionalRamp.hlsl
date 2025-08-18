#ifndef TOON_ADDITIONAL_LIGHTS_RAMP_FWD_INCLUDED
#define TOON_ADDITIONAL_LIGHTS_RAMP_FWD_INCLUDED

// URP includes (Forward/Forward+ 조명 API)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

inline float2 SafeRange01(float2 mm) {
    return (mm.y > mm.x) ? mm : float2(0.0, 1.0);
}
inline float  SafeSteps(float s) {
    return max(2.0, s); // 최소 2 (=투톤)
}

// x(0..1)을 inMinMax 구간으로 정규화 → steps단 양자화 → 0..1 리매핑
inline float QuantizeToSteps01(float x, float2 inMinMax, float steps) {
    float2 inMM = SafeRange01(inMinMax);
    float  t    = saturate((saturate(x) - inMM.x) / max(1e-5, (inMM.y - inMM.x)));
    float  bands = SafeSteps(steps);       // 2,3,4...
    float  q     = bands - 1.0;            // 분모
    float  k     = floor(t * bands);       // 0..(bands-1)
    return saturate(k / q);                 // 0..1 (정확히 steps단)
}

inline float NL_Att(float3 nWS, Light L) {
    float nl = saturate(dot(SafeNormalize(nWS), normalize(L.direction)));
    return nl * L.distanceAttenuation * L.shadowAttenuation;
}

inline float CombineRamp(float acc, float r, float mode) {
    // 0=max(권장, 셀 감성 깔끔), 1=add(clamp), 2=screen
    if (mode < 0.5)       return max(acc, r);
    else if (mode < 1.5)  return saturate(acc + r);
    else                  return 1.0 - (1.0 - acc) * (1.0 - r);
}

// 모든 라이트(메인 + F+ 비-메인 dir + 픽셀 추가광)에 대해 per-light 램프 생성/합성
inline float ComputeRampFromAllLights(
    float3 positionWS, float3 normalWS,
    float2 inMinMax, float steps, float combineMode)
{
    // F+ 루프 요구: InputData가 스코프 내에 있어야 함
    float4 positionCS = TransformWorldToHClip(positionWS);

    InputData inputData = (InputData)0;
    inputData.positionWS = positionWS;
    inputData.normalWS = SafeNormalize(normalWS);
    inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(positionWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);

    float acc = 0.0;

    // Main light
    {
        Light mainL = GetMainLight();
        float r = QuantizeToSteps01(NL_Att(normalWS, mainL), inMinMax, steps);
        acc = CombineRamp(acc, r, combineMode);
    }

    // Forward+ : non-main directional lights
    #if USE_FORWARD_PLUS
    {
        UNITY_LOOP for (uint i = 0u; i < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); i++)
        {
            Light L = GetAdditionalLight(i, inputData.positionWS, half4(1,1,1,1));
            float r = QuantizeToSteps01(NL_Att(normalWS, L), inMinMax, steps);
            acc = CombineRamp(acc, r, combineMode);
        }
    }
    #endif

    // Additional per-pixel lights (point/spot/dir)
    #if defined(_ADDITIONAL_LIGHTS)
    {
        uint count = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(count)
            Light L = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
            float r = QuantizeToSteps01(NL_Att(normalWS, L), inMinMax, steps);
            acc = CombineRamp(acc, r, combineMode);
        LIGHT_LOOP_END
    }
    #endif

    return saturate(acc); // 0..1
}

// ---- Shader Graph Custom Function API ----
// out: 0..1 램프
void AdditionalLightsRampFwd_float(
    float3 normalWS,
    float3 positionWS,
    float4 positionCS,   // SG 시그니처 호환용 (내부에서 직접 positionCS 계산하므로 미사용 OK)
    float3 viewDirWS,    // SG 시그니처 호환용
    float2 inMinMax,
    float2 outMinMax,
    float  steps,
    float  combineMode,
    out float rampOut)
{
    float r01 = ComputeRampFromAllLights(positionWS, normalWS, inMinMax, steps, combineMode);
    float2 outMM = SafeRange01(outMinMax);
    rampOut = lerp(outMM.x, outMM.y, r01);
}

void AdditionalLightsRampFwd_half(
    half3 normalWS,
    half3 positionWS,
    half4 positionCS,
    half3 viewDirWS,
    half2 inMinMax,
    half2 outMinMax,
    half  steps,
    half  combineMode,
    out half rampOut)
{
    float r;
    AdditionalLightsRampFwd_float((float3)normalWS, (float3)positionWS, (float4)positionCS, (float3)viewDirWS,
                                  (float2)inMinMax, (float2)outMinMax, (float)steps, (float)combineMode, r);
    rampOut = (half)r;
}

#endif // TOON_ADDITIONAL_LIGHTS_RAMP_FWD_INCLUDED
