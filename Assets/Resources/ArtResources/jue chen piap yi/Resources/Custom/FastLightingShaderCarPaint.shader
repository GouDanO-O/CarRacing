Shader "Custom/FastLightingShaderCarPaint" {
	Properties {
		_Color ("Color", Vector) = (0.5,0.5,0.5,1)
		_ColorLit ("Color Lit", Vector) = (1,1,1,0.57)
		_FresnelColor ("Fresnel Color", Vector) = (0,0,0,0.25)
		_ColoredReflection1 ("Colored Reflection 1", Vector) = (1,1,1,1)
		_ColoredReflection2 ("Colored Reflection 2", Vector) = (1,1,1,1)
		_ColoredReflectionCoat ("Colored Reflection Coat", Vector) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0, 1)) = 1
		_Emissive ("Emissive Color", Vector) = (0,0,0,0)
		_FresnePow ("Reflection Fresnel power", Range(0, 1)) = 1
		_ReflectionAdd ("Reflection Add Modifier", Range(0, 1)) = 0.05
		_ReflectionSmoothness ("Reflection Smoothness", Range(0, 10)) = 0
		_ReflectionSmoothnessCoat ("Reflection Smoothness Coat", Range(0, 10)) = 0
		_Specularity ("Specularity", Range(0.01, 1)) = 0.2
		_SpecularPower ("SpecularPower", Range(0, 1)) = 0.3
		_NormalModifier ("Normal Modifier", Vector) = (1,1,1,1)
		_LightOverflowOffset ("MainLightOverflow Offset", Range(0, 10)) = 1
		_ReflectionNormalSmoothing ("Reflection Normal Smoothing", Range(0, 1)) = 0.3
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