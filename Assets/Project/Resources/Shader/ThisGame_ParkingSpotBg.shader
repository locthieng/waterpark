Shader "ThisGame/ParkingSpotBg" {
	Properties {
		[Header(Background)] _Color ("Background Color", Vector) = (0.88,0.64,0.64,1)
		_BgHalfWidth ("Background Half Width (world units)", Float) = 5.5
		_BgHalfHeight ("Background Half Height (world units)", Float) = 1.3
		[Header(Spots)] _SpotTex ("Spot Circle Texture", 2D) = "white" {}
		_SpotColor ("Spot Color", Vector) = (1,1,1,0.2)
		_SpotCount ("Spot Count", Float) = 5
		_SpotGap ("Spot Gap", Float) = 2.2
		_SpotSize ("Spot Size (world units)", Float) = 2.56
		[Header(Stencil Hole Punch)] [IntRange] _StencilRef ("Stencil Ref", Range(0, 255)) = 2
		[IntRange] _StencilReadMask ("Stencil Read Mask", Range(0, 255)) = 2
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Unlit/Texture"
}