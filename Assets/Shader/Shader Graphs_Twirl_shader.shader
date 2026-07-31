Shader "Shader Graphs/Twirl_shader"
{
	Properties
	{
		[PerRendererData] [NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_VoronoiScale ("VoronoiScale", Float) = 2
		_VoronoiSpeed ("VoronoiSpeed", Float) = 2
		_TwirlStrenght ("TwirlStrenght", Float) = 2
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
			#pragma target 3.0
			#include "UnityCG.cginc"

			struct appdata_t { float4 vertex : POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };
			struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };

			sampler2D _MainTex;
			float4 _Color;
			float  _VoronoiScale;
			float  _VoronoiSpeed;
			float  _TwirlStrenght;
			fixed4 _RendererColor;

			float2 Hash22(float2 p)
			{
				p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
				return frac(sin(p) * 43758.5453);
			}

			float Voronoi(float2 uv, float t)
			{
				float2 g = floor(uv);
				float2 f = frac(uv);
				float best = 8.0;
				for (int y = -1; y <= 1; y++)
				for (int x = -1; x <= 1; x++)
				{
					float2 lat = float2(x, y);
					float2 off = Hash22(g + lat);
					off = 0.5 + 0.5 * sin(t + 6.2831 * off);
					float d = length(lat + off - f);
					best = min(best, d);
				}
				return best;
			}

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
				float2 uv = IN.texcoord;
				float2 d  = uv - 0.5;
				float  r  = length(d);

				// xoay uv quanh tam, manh nhat o giua
				float ang = _TwirlStrenght * saturate(1.0 - r * 2.0);
				float s, c;
				sincos(ang, s, c);
				float2 tw = float2(d.x * c - d.y * s, d.x * s + d.y * c) + 0.5;

				float v = Voronoi(tw * _VoronoiScale, _Time.y * _VoronoiSpeed);
				v = saturate(1.0 - v);

				fixed4 t = tex2D(_MainTex, tw);
				fixed4 col = t * IN.color;
				col.rgb *= _Color.rgb * (0.5 + v);
				col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
