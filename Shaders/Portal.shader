Shader "VeryRealHelp/Unlit/Portal"
{
    Properties
    {
		[NoScaleOffset]
		_Cubemap("Cube Map", CUBE) = "white" {}
		_RotateSpeed("Rotate Speed", float) = 0
		_FieldOfView("Field of View", Range(0,0.5)) = 0.2
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

            #include "UnityCG.cginc"
			#include "Matrices.cginc"

			samplerCUBE _Cubemap;
			half _RotateSpeed;
			fixed _FieldOfView;

            struct VertexInput
            {
                float4 position : POSITION;
            };

            struct VertexOutput
            {
				float4 position : SV_POSITION;
				float3 worldPosition : TEXCOORD0;
            };

			VertexOutput vert (VertexInput v)
            {
				VertexOutput o;
                o.position = UnityObjectToClipPos(v.position);
				o.worldPosition = mul(unity_ObjectToWorld, v.position).xyz;
                return o;
            }

            fixed4 frag (VertexOutput i) : SV_Target
            {
				half3 normal = normalize(UnityWorldSpaceViewDir(i.worldPosition));
				normal = reflect(normal, fixed3(0, 1, 0));
				normal += (half3(0, 0, 1) - normal) * _FieldOfView;
				normal = mul(rotationMatrix(fixed3(0, 1, 0), _Time * _RotateSpeed), normal);
				return texCUBE(_Cubemap, normal);
            }
            ENDCG
        }
    }
}
