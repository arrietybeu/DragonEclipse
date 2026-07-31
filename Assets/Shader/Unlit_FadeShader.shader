// Reconstructed by hand. AssetRipper could not recover the original HLSL and left a
// dummy-exporter body here that blitted _MainTex opaquely, so the fullscreen Fade
// canvas painted over the whole game.
//
// Modelled on Unity's built-in UI/Default (same tags, queue, stencil and clipping) so it
// behaves like a normal Canvas graphic under URP, with the dissolve added on top.
//
// Contract, from FadeMaterial.mat and Awaken.UI.Fade (which drives _Value via SetFloat):
//   _Value = 0 -> screen fully clear,  _Value = 1 -> screen fully covered
//   _MainTex is FadeTex.png, a greyscale spiral used as the wipe mask
Shader "Unlit/FadeShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _FirstColor ("First color", Vector) = (1,1,1,1)
        _SecondColor ("Second color", Vector) = (0,0,0,1)
        _Value ("Value", Float) = 0
        _EdgeSoftness ("Edge softness", Range(0.001,0.5)) = 0.12

        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _FirstColor;
            fixed4 _SecondColor;
            float  _Value;
            float  _EdgeSoftness;

            v2f vert (appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // spiral wipe mask, clamped and NaN-guarded
                float rawMask = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd).r;
                float mask    = (rawMask == rawMask) ? saturate(rawMask) : 0.5;

                float soft = max(_EdgeSoftness, 1e-4);
                float v    = saturate(_Value);
                float wipe = saturate((v * (1.0 + soft) - mask) / soft);

                // hard contract regardless of the mask
                float a = wipe * step(0.0005, v);
                a = max(a, step(0.9995, v));

                fixed4 color = fixed4(lerp(_FirstColor.rgb, _SecondColor.rgb, mask), a) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
    Fallback Off
}
