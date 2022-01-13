Shader "VeryRealHelp/Lit/Avatar Cel"
{
    Properties
    {
        _SkinColor ("Skin Color", Color) = (1,1,1,1)
        _HairColor ("Hair Color", Color) = (1,1,1,1)
        _PrimaryColor ("Primary Color", Color) = (1,1,1,1)
        _SecondaryColor ("Secondary Color", Color) = (1,1,1,1)
        _ColorMap ("ColorMap", 2D) = "gray" {}
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissionTex ("Emission", 2D) = "black" {}
        _HighlightStep ("Step Highlight", Range(0,1)) = 0.7
        _ShadowStep ("Step Shadow", Range(0,1)) = 0.4
        _StepMix ("Step Mix", Range(0,1)) = 0.2
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineSize("Outline Size", Range(0, 5)) = 1.5
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
			"LightMode"="ForwardBase"
        }
        LOD 200 
        
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "../Lib/HSV.cginc"
    
			UNITY_INSTANCING_BUFFER_START(Props)
			   UNITY_DEFINE_INSTANCED_PROP(fixed4, _SkinColor)
			   UNITY_DEFINE_INSTANCED_PROP(fixed4, _HairColor)
			   UNITY_DEFINE_INSTANCED_PROP(fixed4, _PrimaryColor)
			   UNITY_DEFINE_INSTANCED_PROP(fixed4, _SecondaryColor)
			UNITY_INSTANCING_BUFFER_END(Props)
    
            sampler2D _ColorMap;
            sampler2D _MainTex;
            sampler2D _EmissionTex;
        
            fixed _HighlightStep;
            fixed _ShadowStep;
            fixed _StepMix;
		    fixed4 _OutlineColor;
		    fixed _OutlineSize;
    
            
            fixed4 get_color_from_map (float2 uv) {
                half4 s = tex2D(_ColorMap, uv);
                fixed skinWeight = smoothstep(0, 1, clamp((s.r - 0.5)  * 2.0, 0, 1)) * s.a;
                fixed hairWeight = smoothstep(0, 1, 1- clamp(s.r * 2.0, 0, 1)) * s.a;
                fixed primaryWeight = smoothstep(0, 1, clamp((s.g - 0.5)  * 2.0, 0, 1)) * s.a;
                fixed secondaryWeight = smoothstep(0, 1, 1- clamp(s.g * 2.0, 0, 1)) * s.a;
                fixed4 skin = float4(1,1,1,1) - (1-UNITY_ACCESS_INSTANCED_PROP(_SkinColor_arr, _SkinColor)) * skinWeight;
                fixed4 hair = float4(1,1,1,1) - (1-UNITY_ACCESS_INSTANCED_PROP(_HairColor_arr, _HairColor)) * hairWeight;
                fixed4 primary = float4(1,1,1,1) - (1-UNITY_ACCESS_INSTANCED_PROP(_PrimaryColor_arr, _PrimaryColor)) * primaryWeight;
                fixed4 secondary = float4(1,1,1,1) - (1-UNITY_ACCESS_INSTANCED_PROP(_SecondaryColor_arr, _SecondaryColor)) * secondaryWeight;
                fixed4 color = skin * hair * primary * secondary;
				return color;
            }


            struct VertexInput
            {
                float4 position : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPosition : TEXCOORD1;
				half3 worldNormal : TEXCOORD2;
				fixed4 color : COLOR0;
				UNITY_FOG_COORDS(1)
            };


			VertexOutput vert (VertexInput v)
            {
				VertexOutput o;
                o.position = UnityObjectToClipPos(v.position);
				o.uv = v.uv;
				o.worldPosition = mul(unity_ObjectToWorld, v.position).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.color = _LightColor0 * max(0, dot(o.worldNormal, normalize(_WorldSpaceLightPos0.xyz)));
				o.color = half4(ShadeSH9(half4(o.worldNormal, 1)), 1);
				UNITY_TRANSFER_FOG(o, o.position);
				return o;
            }

            fixed4 ApplyStep (fixed4 dry) {
                fixed3 hsv = rgb2hsv(dry.rgb);
                fixed v = saturate(hsv.z);
                fixed highlight = step(_HighlightStep, v) * 0.25;
                fixed midtone = step(_ShadowStep, v) * 0.2 + 0.5;
                hsv.z = highlight + midtone;
				fixed4 wet = saturate(fixed4(hsv2rgb(hsv).xyz, dry.a));
                return lerp(dry, wet, _StepMix);
            }

            fixed4 frag (VertexOutput i) : SV_Target
            {
				fixed4 color = tex2D(_MainTex, i.uv) * get_color_from_map(i.uv) * ApplyStep(i.color);
				fixed4 emission = tex2D(_EmissionTex, i.uv);
				color = fixed4(lerp(color.rgb, emission.rgb, emission.a), color.a);
				UNITY_APPLY_FOG(i.fogCoord, color);
				return color;
            }
            ENDCG
        }

		Pass
		{
			Cull Front

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			
			fixed4 _OutlineColor;
			fixed _OutlineSize;


			struct VertexInput
			{
				float4 position : POSITION;
				float3 normal : NORMAL;
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
				UNITY_FOG_COORDS(1)
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;
				o.position = UnityObjectToClipPos(v.position);
				float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, v.normal));
				o.position.xy += normalize(clipNormal.xy) / _ScreenParams.xy * _OutlineSize * o.position.w * 2;
				UNITY_TRANSFER_FOG(o, o.position);
				return o;
			}

			fixed4 frag(VertexOutput i) : SV_Target
			{
				return _OutlineColor;
			}
			ENDCG
		}
    }
    FallBack "Diffuse"
}
