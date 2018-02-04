// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Created by mouurusai
//Topic for reference:   ttp://forum.unity3d.com/threads/shader-that-swaps-a-textures-pixels-colors-color-palette-swapper.212576/
Shader "Custom/SwapPallits"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_PallitTex ("_PallitTex Texture", 2D) = "white" {}
		_PallitCount("PallitCount", float) = 2.0
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _PallitTex;
			float _PallitCount;

			fixed4 frag(v2f IN) : COLOR
			{
				float4 pCoord = tex2Dlod(_MainTex, float4(IN.texcoord.xy, 0.0, 0.0));
				float palSize = 1.0/_PallitCount;
			float palShift = IN.color.g*255.0;
			float4 c = tex2Dlod(_PallitTex, float4(pCoord.g*palSize+palSize*(palShift-1), 0.5, 0.0, 0.0));
			//&&				float4 c = tex2Dlod(_PallitTex, float4(pCoord.g*palSize, 0.5, 0.0, 0.0));
				//$$return c*_Color;
				return c*_Color;
			}
		ENDCG
		}
	}
}

