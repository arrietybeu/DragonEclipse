Shader "Shader Graphs/Vegetation_wind_shader"
{
	Properties
	{
		[PerRendererData] [NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
		_WindSpeed ("WindSpeed", Float) = 1
		_WindDirection ("WindDirection", Vector) = (1,0,0,0)
		_WindScale ("WindScale", Float) = 2
		_WindStrenght ("WindStrenght", Float) = 0.01
		_MaskLenght ("MaskLenght", Float) = 20
		_Color ("Tint", Color) = (1,1,1,1)
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[HideInInspector] [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[HideInInspector] [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

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
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _RendererColor;
			float  _WindSpeed;
			float  _WindScale;
			float  _WindStrenght;
			float  _MaskLenght;
			float4 _WindDirection;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_OUTPUT(v2f, OUT);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				float3 wpos = mul(unity_ObjectToWorld, IN.vertex).xyz;

				// chi phan ngon cua sprite duoc dao dong, goc dung yen
				float mask = pow(saturate(IN.texcoord.y), max(_MaskLenght, 0.0001));

				// pha thay doi theo vi tri the gioi -> moi cay lech pha nhau
				float phase = (wpos.x + wpos.y) * _WindScale + _Time.y * _WindSpeed;
				float wave  = sin(phase) * 0.5 + sin(phase * 0.47 + 1.3) * 0.5;

				float2 dir = _WindDirection.xy;
				float  len = max(length(dir), 1e-5);
				dir /= len;

				wpos.xy += dir * (wave * _WindStrenght * mask);

				OUT.vertex   = mul(UNITY_MATRIX_VP, float4(wpos, 1.0));
				OUT.texcoord = IN.texcoord;
				OUT.color    = IN.color * _Color * _RendererColor;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}

	Fallback "Sprites/Default"
}
