Shader "UI/Override" {
	Properties {
		[KeywordEnum(BASIC,SVCONTROL,SDF)] _Mode ("Material mode", Float) = 0
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_HLTint ("Highlight Tint", Vector) = (1,1,1,1)
		_AlphaScale ("AlphaScale", Range(0, 10)) = 1
		[Header(Only for SDF Mode)] _GlowColorIn ("Glow Color In", Vector) = (1,1,1,1)
		_GlowColorOut ("Glow Color Out", Vector) = (1,1,1,1)
		_GlowStrength ("Glow strength", Range(0, 10)) = 1
		_GlowCurve ("Glow curve", Float) = 1
		_GlowOffset ("Glow Offset", Float) = 0
		[HideInInspector] _GlowAmount ("Glow Amount", Range(1E-05, 0.5)) = 0.2
		[Space] _StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}