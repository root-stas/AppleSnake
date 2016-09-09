Shader "FX/Matte Shadow" {

Properties{
	_Color("Main Color", Color) = (1,1,1,1)
}

SubShader{
	Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
	LOD 200
	Blend Zero SrcColor
	CGPROGRAM
	#pragma surface surf ShadowOnly alphatest:_Cutoff
	fixed4 _Color;
	struct Input { float2 uv_MainTex; };

	inline fixed4 LightingShadowOnly(SurfaceOutput s, fixed3 lightDir, fixed atten)
	{
		fixed4 c;
		c.rgb = s.Albedo*atten;
		c.a = s.Alpha;
		return c;
	}

	void surf(Input IN, inout SurfaceOutput o) {
		fixed4 c = _Color;
		o.Albedo = c.rgb;
		o.Alpha = 1;
	}

	ENDCG
}
	Fallback "Transparent/Cutout/VertexLit"
}