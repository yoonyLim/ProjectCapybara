// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TidalFlask/Foliage Wind Standard TextureBlend Lit"
{
	Properties
	{
		_BaseTexture("Base Texture", 2D) = "white" {}
		_BaseTexColorTint("Base Tex Color Tint", Color) = (1,1,1,0)
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_ShadowRecievedStrength("Shadow Recieved Strength", Range( 0 , 1)) = 1
		_BlendTexture("Blend Texture", 2D) = "white" {}
		_BlendTextureTiling("Blend Texture Tiling", Float) = 1
		_BlendTextureHeight("Blend Texture Height", Range( -5 , 1)) = -0.5
		_BlendTextureContrast("Blend Texture Contrast", Range( 0 , 5)) = 3
		_WindStrength("Wind Strength", Range( 0 , 1)) = 0.4
		_WindScale("Wind Scale", Range( 0 , 1)) = 0.4
		_WindSpeed("Wind Speed", Vector) = (2,1,0,0)
		_DistanceFadeColor("Distance Fade Color", Color) = (0.3820755,0.8960492,1,1)
		_DistanceFadeStart("Distance Fade Start", Float) = 20
		_DistanceFadeEnd("Distance Fade End", Float) = 50
		_DistanceFadeOpacity("Distance Fade Opacity", Range( 0 , 1)) = 0.8
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float eyeDepth;
			float3 worldNormal;
			INTERNAL_DATA
			half ASEVFace : VFACE;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float2 _WindSpeed;
		uniform float _WindScale;
		uniform float _WindStrength;
		uniform float4 _BaseTexColorTint;
		uniform sampler2D _BaseTexture;
		uniform float4 _BaseTexture_ST;
		uniform sampler2D _BlendTexture;
		uniform float _BlendTextureTiling;
		uniform float _BlendTextureHeight;
		uniform float _BlendTextureContrast;
		uniform float4 _DistanceFadeColor;
		uniform float _DistanceFadeOpacity;
		uniform float _DistanceFadeEnd;
		uniform float _DistanceFadeStart;
		uniform float _ShadowRecievedStrength;
		uniform float _Cutoff = 0.5;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 break219 = ase_worldPos;
			float4 appendResult221 = (float4(break219.x , break219.z , 0.0 , 0.0));
			float simplePerlin2D38 = snoise( (( appendResult221 / 2 )*1.0 + float4( ( _Time.y * _WindSpeed ), 0.0 , 0.0 )).xy*_WindScale );
			simplePerlin2D38 = simplePerlin2D38*0.5 + 0.5;
			float3 break49 = ase_worldPos;
			float4 appendResult51 = (float4(( ( ( simplePerlin2D38 - 0.5 ) * _WindStrength ) + break49.x ) , break49.y , break49.z , 0.0));
			float4 lerpResult3 = lerp( float4( ase_worldPos , 0.0 ) , appendResult51 , v.texcoord.xy.y);
			float3 worldToObj1 = mul( unity_WorldToObject, float4( lerpResult3.xyz, 1 ) ).xyz;
			float3 VertexPosition77 = worldToObj1;
			v.vertex.xyz = VertexPosition77;
			v.vertex.w = 1;
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float2 uv_BaseTexture = i.uv_texcoord * _BaseTexture_ST.xy + _BaseTexture_ST.zw;
			float4 tex2DNode4 = tex2D( _BaseTexture, uv_BaseTexture );
			float4 temp_output_98_0 = ( _BaseTexColorTint * tex2DNode4 );
			float4 BaseTexture407 = temp_output_98_0;
			float clampResult333 = clamp( ( pow( i.uv_texcoord.y , ( 1.0 - _BlendTextureHeight ) ) * _BlendTextureContrast ) , 0.0 , 1.0 );
			float4 lerpResult315 = lerp( BaseTexture407 , tex2D( _BlendTexture, (i.uv_texcoord*_BlendTextureTiling + 0.0) ) , clampResult333);
			float4 GroundColorGradient319 = lerpResult315;
			float4 temp_cast_1 = (0.0).xxxx;
			#ifdef UNITY_PASS_FORWARDBASE
				float4 staticSwitch111 = _DistanceFadeColor;
			#else
				float4 staticSwitch111 = temp_cast_1;
			#endif
			float cameraDepthFade83 = (( i.eyeDepth -_ProjectionParams.y - _DistanceFadeStart ) / _DistanceFadeEnd);
			float4 lerpResult84 = lerp( GroundColorGradient319 , staticSwitch111 , ( _DistanceFadeOpacity * saturate( cameraDepthFade83 ) ));
			float4 CustomLightingAndFade169 = lerpResult84;
			float3 switchResult415 = (((i.ASEVFace>0)?(float3(0,0,1)):(float3(0,0,-1))));
			UnityGI gi75 = gi;
			float3 diffNorm75 = WorldNormalVector( i , switchResult415 );
			gi75 = UnityGI_Base( data, 1, diffNorm75 );
			float3 indirectDiffuse75 = gi75.indirect.diffuse + diffNorm75 * 0.0001;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult418 = dot( (WorldNormalVector( i , switchResult415 )) , ase_worldlightDir );
			float clampResult116 = clamp( max( ( max( dotResult418 , 0.0 ) * ase_lightAtten ) , ( 1.0 - _ShadowRecievedStrength ) ) , 0.0 , 1.0 );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 temp_output_72_0 = ( saturate( ( indirectDiffuse75 + clampResult116 ) ) * ase_lightColor.rgb );
			float4 CustomLighting167 = ( CustomLightingAndFade169 * float4( temp_output_72_0 , 0.0 ) );
			c.rgb = CustomLighting167.rgb;
			c.a = 1;
			clip( tex2DNode4.a - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			float2 uv_BaseTexture = i.uv_texcoord * _BaseTexture_ST.xy + _BaseTexture_ST.zw;
			float4 tex2DNode4 = tex2D( _BaseTexture, uv_BaseTexture );
			float4 temp_output_98_0 = ( _BaseTexColorTint * tex2DNode4 );
			o.Albedo = temp_output_98_0.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.z = customInputData.eyeDepth;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.eyeDepth = IN.customPack1.z;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18912
361;530;1906;786;606.6703;-1073.441;1.10729;True;False
Node;AmplifyShaderEditor.CommentaryNode;90;-1770.437,-1343.694;Inherit;False;3500.173;651.9265;;22;167;80;406;408;72;73;412;405;75;116;420;419;96;63;418;416;417;93;415;414;413;424;custom lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;79;-5038.34,96.44376;Inherit;False;3975.085;998.6667;;23;77;1;3;54;51;52;48;41;49;50;39;42;38;35;46;34;225;222;33;44;221;219;218;wind movement;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;331;-327.1618,110.1846;Inherit;False;2445.04;808.2798;Comment;13;319;315;333;394;325;317;395;396;425;404;426;427;428;texture blend;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;414;-1742.254,-1264.622;Inherit;False;Constant;_Vector3;Vector 3;5;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;413;-1739.654,-1106.022;Inherit;False;Constant;_Vector2;Vector 2;5;0;Create;True;0;0;0;False;0;False;0,0,-1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;218;-4917.365,229.5107;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;395;-294.189,763.4894;Inherit;False;Property;_BlendTextureHeight;Blend Texture Height;6;0;Create;True;0;0;0;False;0;False;-0.5;0;-5;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwitchByFaceNode;415;-1508.234,-1261.935;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;219;-4677.92,231.3396;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.TextureCoordinatesNode;317;-280.5805,511.2802;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;404;-5.132393,766.9035;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;27.80743,-385.3248;Inherit;True;Property;_BaseTexture;Base Texture;0;0;Create;True;0;0;0;False;0;False;-1;a35a304ea256b734baa786da99f2cadc;8c80f401599d6904a97cc7995352df7e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;97;96.0648,-569.1783;Inherit;False;Property;_BaseTexColorTint;Base Tex Color Tint;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;44;-4269.143,616.3005;Inherit;False;Property;_WindSpeed;Wind Speed;10;0;Create;True;0;0;0;False;0;False;2,1;3,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;222;-4489.383,448.3716;Inherit;False;Constant;_Vector1;Vector 1;0;0;Create;True;0;0;0;False;0;False;2,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;417;-1240.563,-1094.755;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;33;-4263.212,501.8857;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;221;-4507.776,225.5426;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldNormalVector;416;-1191.929,-1262.273;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;373.9261,-404.178;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;428;35.72034,294.3917;Inherit;False;Property;_BlendTextureTiling;Blend Texture Tiling;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;325;238.4171,511.3732;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;396;220.7915,765.4894;Inherit;False;Property;_BlendTextureContrast;Blend Texture Contrast;7;0;Create;True;0;0;0;False;0;False;3;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;91;-303.1327,1195.304;Inherit;False;2159.169;567.1646;;12;169;84;111;109;85;108;89;112;83;86;87;320;distance fade to color;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;407;619.9618,-310.5927;Inherit;False;BaseTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;427;384.2825,271.9981;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;394;542.8649,511.3163;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;225;-4223.775,225.5426;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-4051.184,500.4236;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;418;-909.4273,-1259.717;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;426;640.7825,271.4628;Inherit;True;Property;_BlendTexture;Blend Texture;4;0;Create;True;0;0;0;False;0;False;-1;4b53c0b07c59df74e9fef56f4719d2ed;4b53c0b07c59df74e9fef56f4719d2ed;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;419;-732.8576,-1258.374;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-250.3058,1667.803;Inherit;False;Property;_DistanceFadeStart;Distance Fade Start;12;0;Create;True;0;0;0;False;0;False;20;10.28;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;425;720.7263,169.8422;Inherit;False;407;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-3841.686,500.2346;Inherit;False;Property;_WindScale;Wind Scale;9;0;Create;True;0;0;0;False;0;False;0.4;0.158;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;35;-3883.848,225.3067;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-247.7969,1561.821;Inherit;False;Property;_DistanceFadeEnd;Distance Fade End;13;0;Create;True;0;0;0;False;0;False;50;40;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-808.0762,-794.0907;Inherit;False;Property;_ShadowRecievedStrength;Shadow Recieved Strength;3;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;63;-807.9033,-915.2872;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;333;799.5853,509.2443;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;315;1041.115,238.5318;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CameraDepthFade;83;91.68939,1626.557;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;96;-501.2783,-790.6197;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;420;-490.6986,-916.6545;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;38;-3621.201,217.9587;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;424;-251.7295,-913.8488;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;112;191.038,1314.883;Inherit;False;Constant;_Float1;Float 1;13;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;568.5875,1579.512;Inherit;False;Property;_DistanceFadeOpacity;Distance Fade Opacity;14;0;Create;True;0;0;0;False;0;False;0.8;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;89;354.7884,1628.261;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-3324.725,503.1466;Inherit;False;Property;_WindStrength;Wind Strength;8;0;Create;True;0;0;0;False;0;False;0.4;0.346;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;50;-3317.243,713.5375;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;39;-3323.864,225.2826;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;85;93.84082,1405.452;Inherit;False;Property;_DistanceFadeColor;Distance Fade Color;11;0;Create;True;0;0;0;False;0;False;0.3820755,0.8960492,1,1;0.5333334,0.6745098,0.1607843,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;319;1698.896,232.1729;Inherit;False;GroundColorGradient;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;895.1083,1607.702;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;116;-31.21597,-911.5196;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-3054.057,225.2896;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;111;397.5506,1407.407;Inherit;False;Property;_Keyword0;Keyword 0;11;0;Create;True;0;0;0;False;0;False;0;0;0;False;UNITY_PASS_FORWARDBASE;Toggle;2;Key0;Key1;Fetch;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;49;-3045.079,710.4855;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.IndirectDiffuseLighting;75;-36.8986,-1038.952;Inherit;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;320;803.3039,1321.95;Inherit;False;319;GroundColorGradient;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;84;1158.553,1388.965;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-2794.242,687.1675;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;405;270.8574,-1039.179;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightColorNode;73;234.2935,-900.3678;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;51;-2633.382,710.9655;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;54;-2394.091,497.9655;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;52;-2398.239,922.8834;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;412;448.9924,-1036.55;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;169;1514.616,1381.886;Inherit;False;CustomLightingAndFade;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;406;905.1004,-1084.444;Inherit;False;169;CustomLightingAndFade;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;3;-2108.269,687.9495;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;657.9799,-1042.181;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;1242.129,-1062.528;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TransformPositionNode;1;-1778.393,683.4116;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;167;1439.19,-1059.094;Inherit;False;CustomLighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-1421.055,684.0695;Inherit;False;VertexPosition;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;408;905.9928,-866.3229;Inherit;False;CustomLightingInfo;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;1527.912,-92.77522;Inherit;False;77;VertexPosition;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;170;1525.94,-172.0761;Inherit;False;167;CustomLighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;32;1911.958,-397.672;Float;False;True;-1;6;;0;0;CustomLighting;TidalFlask/Foliage Wind Standard TextureBlend Lit;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;1;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;1;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;415;0;414;0
WireConnection;415;1;413;0
WireConnection;219;0;218;0
WireConnection;404;0;395;0
WireConnection;221;0;219;0
WireConnection;221;1;219;2
WireConnection;416;0;415;0
WireConnection;98;0;97;0
WireConnection;98;1;4;0
WireConnection;325;0;317;2
WireConnection;325;1;404;0
WireConnection;407;0;98;0
WireConnection;427;0;317;0
WireConnection;427;1;428;0
WireConnection;394;0;325;0
WireConnection;394;1;396;0
WireConnection;225;0;221;0
WireConnection;225;1;222;1
WireConnection;34;0;33;0
WireConnection;34;1;44;0
WireConnection;418;0;416;0
WireConnection;418;1;417;0
WireConnection;426;1;427;0
WireConnection;419;0;418;0
WireConnection;35;0;225;0
WireConnection;35;2;34;0
WireConnection;333;0;394;0
WireConnection;315;0;425;0
WireConnection;315;1;426;0
WireConnection;315;2;333;0
WireConnection;83;0;87;0
WireConnection;83;1;86;0
WireConnection;96;0;93;0
WireConnection;420;0;419;0
WireConnection;420;1;63;0
WireConnection;38;0;35;0
WireConnection;38;1;46;0
WireConnection;424;0;420;0
WireConnection;424;1;96;0
WireConnection;89;0;83;0
WireConnection;39;0;38;0
WireConnection;319;0;315;0
WireConnection;109;0;108;0
WireConnection;109;1;89;0
WireConnection;116;0;424;0
WireConnection;41;0;39;0
WireConnection;41;1;42;0
WireConnection;111;1;112;0
WireConnection;111;0;85;0
WireConnection;49;0;50;0
WireConnection;75;0;415;0
WireConnection;84;0;320;0
WireConnection;84;1;111;0
WireConnection;84;2;109;0
WireConnection;48;0;41;0
WireConnection;48;1;49;0
WireConnection;405;0;75;0
WireConnection;405;1;116;0
WireConnection;51;0;48;0
WireConnection;51;1;49;1
WireConnection;51;2;49;2
WireConnection;412;0;405;0
WireConnection;169;0;84;0
WireConnection;3;0;54;0
WireConnection;3;1;51;0
WireConnection;3;2;52;2
WireConnection;72;0;412;0
WireConnection;72;1;73;1
WireConnection;80;0;406;0
WireConnection;80;1;72;0
WireConnection;1;0;3;0
WireConnection;167;0;80;0
WireConnection;77;0;1;0
WireConnection;408;0;72;0
WireConnection;32;0;98;0
WireConnection;32;10;4;4
WireConnection;32;13;170;0
WireConnection;32;11;78;0
ASEEND*/
//CHKSM=A26ADEEEBE51D3077CB112690F42913343AF5389