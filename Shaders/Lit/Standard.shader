Shader "VeryRealHelp/Lit/Standard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ShinyColor("Shinyness Color", Color) = (0,0,0,0)
		_ShinyTex("Shinyness", 2D) = "gray" {}
		[HDR]
		_EmissionColor("Emission Color", Color) = (0,0,0,1)
		_EmissionTex ("Emission", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard noshadow noshadowmask nodynlightmap nodirlightmap exclude_path:deferred 
        #pragma target 3.5

        sampler2D _MainTex;
		half4 _ShinyColor;
		sampler2D _ShinyTex;
		half4 _EmissionColor;
        sampler2D _EmissionTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
			o.Alpha = c.a;
			fixed4 shiny = lerp(tex2D(_ShinyTex, IN.uv_MainTex), _ShinyColor, _ShinyColor.a);
            o.Metallic = shiny.r;
            o.Smoothness = shiny.r;
			o.Emission = tex2D(_EmissionTex, IN.uv_MainTex) * _EmissionColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
