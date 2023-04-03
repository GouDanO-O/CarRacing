Shader "Custom/FastLightingShaderNormalGradient" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_ColorTop ("ColorTop", Vector) = (1,1,1,1)
		_Slope2 ("Gradient Slope", Range(0, 15)) = 1
		_Offset ("Gradient Offset", Range(-20, 20)) = 0
		_ColorBottom ("ColorBottom", Vector) = (1,1,1,1)
		[InverseFloat] _Height ("Height", Float) = 1
		_ZeroHeight ("Zero Height", Float) = 0
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Emissive ("Emissive Color", Vector) = (0,0,0,0)
		_FresnePow ("Fresnel power", Range(0, 2)) = 1
		_Specularity ("Specularity", Range(0.01, 1)) = 0.5
		_SpecularPower ("SpecularPower", Range(0, 1)) = 0.5
		_ReflectionSmoothness ("Reflection Smoothness", Range(0, 10)) = 0
		_LightOverflowOffset ("MainLightOverflow Offset", Range(0, 10)) = 1
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
	Fallback "Custom/FastLightingShadows"
}