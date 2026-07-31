Shader "Shader Graphs/HCFX_Particle_apb"
{
	Properties
	{
		[NoScaleOffset] Texture2D_F593E37E ("MainTex", 2D) = "white" {}
		[ToggleUI] Boolean_BB495B7B ("Use SoftParticleFactor?", Float) = 1
		[ToggleUI] Boolean_52F3CBA5 ("Boolean_52F3CBA5", Float) = 0
		Vector1_53729B24 ("Intensity", Float) = 1
		_BaseColor ("Base Color", Color) = (1,1,1,1)
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] _BUILTIN_QueueOffset ("Float", Float) = 0
		[HideInInspector] _BUILTIN_QueueControl ("Float", Float) = -1
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			struct appdata_t { float4 vertex : POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };
			struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };

			sampler2D Texture2D_F593E37E;
			float  Vector1_53729B24;
			fixed4 _BaseColor;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex   = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color    = IN.color * _BaseColor;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(Texture2D_F593E37E, IN.texcoord) * IN.color;
				c.rgb *= max(Vector1_53729B24, 0.0);
				return c;
			}
			ENDCG
		}
	}
	Fallback "Particles/Alpha Blended"
}
