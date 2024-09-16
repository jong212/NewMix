Shader "Custom/VertexColor" {
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Unlit vertex:vert alpha
		#pragma target 3.0

		struct Input {
			float4 vertColor;
		};

		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertColor = v.color;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.vertColor.rgb;
			o.Alpha = IN.vertColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}