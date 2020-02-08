Shader "VeryRealHelp/Unlit/Transparent/Fresnel"
{
    Properties
    {
		[HDR]
		_Color("Color", Color) = (1,1,1,1)
		_FresnelPower("Fresnel Power", Float) = 2
	}
	SubShader
	{
		Tags {
			"Queue" = "Transparent"
			"ForceNoShadowCasting" = "True"
		}
		LOD 100

		Pass
		{
			Cull Off
			Blend One One
			ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			float4 _Color;
			float _FresnelPower;

            struct VertexInput
            {
                float4 position : POSITION;
				float3 normal : NORMAL;
            };

            struct VertexOutput
            {
				float4 position : SV_POSITION;
				float3 worldPosition : TEXCOORD0;
				half3 worldNormal : TEXCOORD1;
            };

			VertexOutput vert (VertexInput v)
            {
				VertexOutput o;
                o.position = UnityObjectToClipPos(v.position);
				o.worldPosition = mul(unity_ObjectToWorld, v.position).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (VertexOutput i) : SV_Target
            {
				half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPosition));
				float dotResult = (1 - abs(dot(worldViewDir, i.worldNormal))) / 2;
				return _Color * pow(dotResult, _FresnelPower);
            }
            ENDCG
        }
    }
}
