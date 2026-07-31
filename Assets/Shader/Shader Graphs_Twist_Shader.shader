Shader "Shader Graphs/Twist_Shader"
{
	Properties
	{
		[PerRendererData] [NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
		_Speed ("Speed", Float) = 0.01
		[HDR] _Color ("Color", Vector) = (1,1,1,0)
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
			float4 _Color;
			float  _Speed;
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
				float2 d = IN.texcoord - 0.5;
				float  r = length(d);
				float ang = _Time.y * _Speed * (1.0 - saturate(r * 2.0));
				float s, c;
				sincos(ang, s, c);
				float2 uv = float2(d.x * c - d.y * s, d.x * s + d.y * c) + 0.5;

				fixed4 col = tex2D(_MainTex, uv) * IN.color;
				col.rgb *= _Color.rgb;
				col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
