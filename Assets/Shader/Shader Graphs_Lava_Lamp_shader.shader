Shader "Shader Graphs/Lava_Lamp_shader"
{
	Properties
	{
		_Speed ("Speed", Float) = -0.5
		_AlphaClipping ("AlphaClipping", Float) = 0
		[HDR] _Color ("Color", Vector) = (0,0,0,0)
		_NoiseScale ("NoiseScale", Float) = 2
		_NoiseStrenght ("NoiseStrenght", Float) = 0.05
		[NoScaleOffset] _Mask_Tex ("Mask_Tex", 2D) = "white" {}
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			struct appdata_t { float4 vertex : POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };
			struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };

			sampler2D _Mask_Tex;
			float4 _Color;
			float  _Speed;
			float  _AlphaClipping;
			float  _NoiseScale;
			float  _NoiseStrenght;

			float Hash21(float2 p)
			{
				p = frac(p * float2(123.34, 456.21));
				p += dot(p, p + 45.32);
				return frac(p.x * p.y);
			}

			float ValueNoise(float2 p)
			{
				float2 i = floor(p);
				float2 f = frac(p);
				f = f * f * (3.0 - 2.0 * f);
				float a = Hash21(i);
				float b = Hash21(i + float2(1, 0));
				float c = Hash21(i + float2(0, 1));
				float d = Hash21(i + float2(1, 1));
				return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
			}

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex   = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color    = IN.color;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float t = _Time.y * _Speed;
				float2 uv = IN.texcoord;

				float n  = ValueNoise(uv * _NoiseScale + float2(0.0, t));
				float n2 = ValueNoise(uv * _NoiseScale * 1.7 - float2(t * 0.6, 0.0));
				float blob = saturate(n * 0.6 + n2 * 0.6);

				float2 warped = uv + (blob - 0.5) * _NoiseStrenght;
				float mask = tex2D(_Mask_Tex, warped).r;

				float alpha = mask * IN.color.a;
				clip(alpha - _AlphaClipping);

				float3 rgb = _Color.rgb * IN.color.rgb * (0.6 + blob * 0.8);
				return float4(rgb, alpha);
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
