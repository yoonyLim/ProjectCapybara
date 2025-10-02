// URP-compatible version of IL3DN/Branch
Shader "IL3DN/Branch URP"
{
    Properties
    {
       _Color("Color", Color) = (1,1,1,1)
       _MainTex("MainTex", 2D) = "white" {}
       [NoScaleOffset] _NoiseTexture("NoiseTexture", 2D) = "white" {}

       [Header(Wind)]
       [Toggle(_WIND_ON)] _Wind("Enable Wind", Float) = 1
       _WindStrenght("Wind Strenght", Range( 0 , 1)) = 0.5
    }

    SubShader
    {
       Tags{ "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
       
       Pass
       {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ _WIND_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float _WindStrenght;
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
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

                #if _WIND_ON
                    float2 panner = (_Time.y * WindDirection.xz * 0.4 * 10.0) + worldPos.xy;
                    float4 worldNoise = SAMPLE_TEXTURE2D_LOD(_NoiseTexture, sampler_NoiseTexture, (panner * 0.1) / 10.0, 0) * _WindStrenght * 0.8;
                    
                    float windInfluence = (v.color.a * worldNoise.r) + (worldNoise.r * v.color.g);
                    float3 windOffset = WindDirection * windInfluence;

                    // THE FIX: Add the world-space offset directly to the world-space position.
                    worldPos += windOffset;
                #endif

                o.positionHCS = TransformWorldToHClip(worldPos);
                o.normalWS = normalWS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;
                
                Light mainLight = GetMainLight();
                half3 normalWS = normalize(i.normalWS);
                half lambert = saturate(dot(normalWS, mainLight.direction));
                half3 finalColor = albedo.rgb * mainLight.color * lambert;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
       }
    }
    Fallback "Universal Render Pipeline/Lit"
}
