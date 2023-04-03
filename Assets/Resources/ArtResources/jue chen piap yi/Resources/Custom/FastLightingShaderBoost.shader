Shader "Custom/FastLightingShaderBoost" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Emissive ("Emissive Color", Vector) = (0,0,0,0)
		_FresnePow ("Fresnel power", Range(0, 2)) = 1
		_Specularity ("Specularity", Range(0.01, 1)) = 0.5
		_SpecularPower ("SpecularPower", Range(0, 1)) = 0.5
		_GlowColor ("Glow Color", Vector) = (1,1,1,1)
		_ReflectionSmoothness ("Reflection Smoothness", Range(0, 10)) = 0
		_LightOverflowOffset ("MainLightOverflow Offset", Range(0, 10)) = 1
		_Tex0 ("Texture 0", 2D) = "white" {}
		_Tex1 ("Texture 1", 2D) = "white" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
}