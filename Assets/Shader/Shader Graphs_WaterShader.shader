Shader "Shader Graphs/WaterShader"
{
	Properties
	{
		[PerRendererData] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
		[NoScaleOffset] _WaterMap ("_WaterMap", 2D) = "white" {}
		_MapScale ("_MapScale", Vector) = (5,5,0,0)
		_Wind ("_Wind", Vector) = (1,1,0,0)
		_ScaleSecond ("_ScaleSecond", Vector) = (0,0,0,0)
		_Power ("_Power", Float) = 0
		_Color ("Tint", Color) = (1,1,1,1)
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
			sampler2D _WaterMap;
			float4 _MapScale;
			float4 _Wind;
			float4 _ScaleSecond;
			float  _Power;
			fixed4 _Color;
			fixed4 _RendererColor;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex   = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color    = IN.color * _Color * _RendererColor;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord;

				// hai lop water map troi nguoc chieu nhau
				float2 uv1 = uv * _MapScale.xy + _Time.y * _Wind.xy * 0.1;
				float2 uv2 = uv * _ScaleSecond.xy - _Time.y * _Wind.xy * 0.06;

				float n1 = tex2D(_WaterMap, uv1).r;
				float n2 = tex2D(_WaterMap, uv2).r;
				float n  = n1 * n2;

				// bien dang nhe mat nuoc
				float2 offset = (float2(n1, n2) - 0.5) * _Power;
				fixed4 c = tex2D(_MainTex, uv + offset) * IN.color;

				// vet sang tren mat nuoc
				float foam = smoothstep(0.55, 0.95, n);
				c.rgb += foam * c.a * 0.35;

				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
