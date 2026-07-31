Shader "Shader Graphs/Sprite_HDR_glow"
{
	Properties
	{
		[PerRendererData] [NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
		[HDR] _Color ("Color", Vector) = (0.5357969,1,0.3066038,0)
		[NoScaleOffset] _Emission ("Emission", 2D) = "white" {}
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[HideInInspector] [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[HideInInspector] [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			struct appdata_t { float4 vertex : POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };
			struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };

			sampler2D _MainTex;
			sampler2D _Emission;
			float4 _Color;
			fixed4 _RendererColor;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex   = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color    = IN.color * _RendererColor;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float4 base = tex2D(_MainTex, IN.texcoord);
				float  em   = tex2D(_Emission, IN.texcoord).r;
				float4 c = base * IN.color;
				c.rgb += _Color.rgb * em * base.a;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
