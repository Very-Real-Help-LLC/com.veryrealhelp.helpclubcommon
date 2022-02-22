Shader "VeryRealHelp/Lit/Avatar"
{
    Properties
    {
        _SkinColor ("Skin Color", Color) = (1,1,1,1)
        _HairColor ("Hair Color", Color) = (1,1,1,1)
        _PrimaryColor ("Primary Color", Color) = (1,1,1,1)
        _SecondaryColor ("Secondary Color", Color) = (1,1,1,1)
        _ColorMap ("ColorMap", 2D) = "gray" {}
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Shininess ("Shininess", Range(0,1)) = 0.5
		_ShininessTex("Shininess Texture", 2D) = "gray" {}
		_EmissionTex ("Emission", 2D) = "black" {}
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard noshadow noshadowmask nodynlightmap nodirlightmap exclude_path:deferred 
        //#pragma surface surf StandardSpecular noshadow noshadowmask nodynlightmap nodirlightmap exclude_path:deferred 
        #pragma target 3.5

        #include "UnityPBSLighting.cginc"

        fixed4 _SkinColor;
        fixed4 _HairColor;
        fixed4 _PrimaryColor;
        fixed4 _SecondaryColor;

        sampler2D _ColorMap;
        sampler2D _MainTex;
		sampler2D _ShininessTex;
        sampler2D _EmissionTex;
        
        fixed _Shininess;

        struct Input
        {
            float2 uv_MainTex;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        fixed4 get_color_from_map (float2 uv) {
            half4 s = tex2D(_ColorMap, uv);
            fixed skinWeight = smoothstep(0, 1, clamp((s.r - 0.5)  * 2.0, 0, 1)) * s.a;
            fixed hairWeight = smoothstep(0, 1, 1- clamp(s.r * 2.0, 0, 1)) * s.a;
            fixed primaryWeight = smoothstep(0, 1, clamp((s.g - 0.5)  * 2.0, 0, 1)) * s.a;
            fixed secondaryWeight = smoothstep(0, 1, 1- clamp(s.g * 2.0, 0, 1)) * s.a;
            fixed4 skin = float4(1,1,1,1) - (1-_SkinColor) * skinWeight;
            fixed4 hair = float4(1,1,1,1) - (1-_HairColor) * hairWeight;
            fixed4 primary = float4(1,1,1,1) - (1-_PrimaryColor) * primaryWeight;
            fixed4 secondary = float4(1,1,1,1) - (1-_SecondaryColor) * secondaryWeight;
            return skin * hair * primary * secondary;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = get_color_from_map(IN.uv_MainTex) * tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
			o.Alpha = c.a;
			fixed4 shiny = tex2D(_ShininessTex, IN.uv_MainTex);
            o.Smoothness = shiny.r * _Shininess;
			//o.Specular = c.rgb * shiny.rgb;
			o.Emission = tex2D(_EmissionTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
