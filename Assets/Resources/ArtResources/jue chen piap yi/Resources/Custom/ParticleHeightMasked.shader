Shader "Custom/ParticleHeightMasked" {
	Properties {
		[KeywordEnum(DISABLED,MASKED)] _Mode ("Heigh Masked Mode", Float) = 0
		_Tint ("Tint", Vector) = (1,1,1,1)
		_Tint2 ("Tint 2", Vector) = (1,1,1,1)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_ZOffset ("Offset", Float) = 0
		_ZOffsetScale ("Offset Scale", Float) = 0
		_FadeOutHeight ("Fade Out Height", Float) = 0
		[InverseFloat] _FadeOutSize ("Fade Out Size", Float) = 1
		_BlendingSource ("Blending Source", Float) = 0
		_BlendingDestination ("Blending Destination", Float) = 0
		_BlendingIsAdditive ("Is Additive", Range(0, 1)) = 0
		_ABlendingSource ("Alpha Blending Source", Float) = 1
		_ABlendingDestination ("Alpha Blending Destination", Float) = 1
		_ZTest ("Z-Test", Float) = 2
		_ZWrite ("Z-Write", Range(0, 1)) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	//CustomEditor "ParticleMaterialEditor"
}