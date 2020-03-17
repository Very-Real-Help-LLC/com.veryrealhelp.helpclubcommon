Shader "VeryRealHelp/Unlit/Portal"
{
	Properties
	{
		[NoScaleOffset]
		_Cubemap("Cube Map", CUBE) = "white" {}
		_FieldOfView("Field of View", Range(-0.5, 0.5)) = 0.2
		_StereoDivergence("Stereo Divergence", Range(-5, 5)) = -1
	}

	SubShader
	{
		Tags {
			"Queue" = "Geometry"
			"ForceNoShadowCasting" = "True"
		}
		LOD 100

		Pass
		{
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

			samplerCUBE _Cubemap;
			fixed _FieldOfView;
			fixed _StereoDivergence;

			float3 RotateAroundYInDegrees(float3 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float3(mul(m, vertex.xz), vertex.y).xzy;
			}

			struct VertexInput
			{
				float4 position : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 position : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.position = UnityObjectToClipPos(v.position);
				o.texcoord = mul(unity_ObjectToWorld, v.position).xyz;
				return o;
			}

			fixed4 frag(VertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				half3 normal = normalize(UnityWorldSpaceViewDir(i.texcoord));
				normal *= half3(-1, -1, 1) + (half3(0, 1, -1) - normal) * _FieldOfView;
				normal = RotateAroundYInDegrees(normal, unity_StereoEyeIndex * _StereoDivergence);
				return texCUBE(_Cubemap, normal);
			}
			ENDCG
		}

	}

	Fallback Off
}
