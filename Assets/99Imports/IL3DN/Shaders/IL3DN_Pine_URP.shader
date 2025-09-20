// URP-compatible version of IL3DN/Pine
Shader "IL3DN/Pine URP"
{
    Properties
    {
       _Color("Color", Color) = (1,1,1,1)
       _AlphaCutoff("Alpha Cutoff", Range( 0 , 1)) = 0.5
       _MainTex("MainTex", 2D) = "white" {}
       [NoScaleOffset] _NoiseTexture("NoiseTexture", 2D) = "white" {}

       [Header(Wind)]
       [Toggle(_WIND_ON)] _Wind("Enable Wind", Float) = 1
       _WindStrenght("Wind Strenght", Range( 0 , 1)) = 0.5

       [Header(Wiggle)]
       [Toggle(_WIGGLE_ON)] _Wiggle("Enable Wiggle", Float) = 1
       _WiggleStrenght("Wiggle Strenght", Range( 0 , 1)) = 0.5
    }

    SubShader
    {
       Tags{ "RenderPipeline" = "UniversalPipeline" "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
       Cull Off

       Pass
       {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ _WIND_ON
            #pragma multi_compile __ _WIGGLE_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
                float3 worldPos     : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _AlphaCutoff;
                float _WindStrenght;
                float _WiggleStrenght;
            CBUFFER_END

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTexture);   SAMPLER(sampler_NoiseTexture);

            static const float3 WindDirection = float3(-0.7, 0, -0.7);

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                // THE FIX: We calculate the final position in World Space before converting to Clip Space.
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);

                #if _WIND_ON
                    float2 panner = (_Time.y * WindDirection.xz * 0.4 * 10.0) + worldPos.xy;
                    float4 worldNoise = SAMPLE_TEXTURE2D_LOD(_NoiseTexture, sampler_NoiseTexture, (panner * 0.1) / 10.0, 0) * _WindStrenght * 0.8;
                    
                    float windInfluence = (v.color.a * worldNoise.r) + (worldNoise.r * v.color.g);
                    float3 windOffset = WindDirection * windInfluence;
                    
                    // THE FIX: Add the world-space offset directly to the world-space position.
                    worldPos += windOffset;
                #endif

                o.positionHCS = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float2 finalUV = i.uv;

                #if _WIGGLE_ON
                    float2 panner = (_Time.y * WindDirection.xz * 0.4 * 10.0) + i.worldPos.xy;
                    float4 worldNoise = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, (panner * 0.1) / 10.0);
                    
                    float wiggleAmount = (SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, worldNoise.rg).r * i.color.g) * _WiggleStrenght;
                    
                    float s, c;
                    sincos(wiggleAmount, s, c);
                    float2x2 rotationMatrix = float2x2(c, -s, s, c);

                    finalUV -= float2(0.5, 0.5);
                    finalUV = mul(rotationMatrix, finalUV);
                    finalUV += float2(0.5, 0.5);
                #endif
                
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);
                half4 finalColor = mainTex * _Color;

                clip(finalColor.a - _AlphaCutoff);

                return finalColor;
            }
            ENDHLSL
       }
    }
    Fallback "Universal Render Pipeline/Lit"
}
