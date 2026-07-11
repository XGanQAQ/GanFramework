Shader "GanFramework/UI/BackgroundBlur"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 0.85)

        _BlurStrength("Blur Strength", Range(0, 8)) = 1.5
        _Saturation("Saturation", Range(0, 2)) = 1.0
        _Brightness("Brightness", Range(0, 2)) = 1.0

        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
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
            Name "UIBackgroundBlur"

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_ST;
                float _BlurStrength;
                float _Saturation;
                float _Brightness;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color * _Color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half3 AdjustSaturation(half3 col, half saturation)
            {
                half luma = dot(col, half3(0.299h, 0.587h, 0.114h));
                return lerp(luma.xxx, col, saturation);
            }

            half4 SampleSceneBlur(float2 uv, float2 texelSize)
            {
                // 9-tap Gaussian-like blur in screen space.
                const half w0 = 0.227027h;
                const half w1 = 0.1945946h;
                const half w2 = 0.1216216h;
                const half w3 = 0.054054h;
                const half w4 = 0.016216h;

                float2 offset = texelSize * _BlurStrength;

                half4 c = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv) * w0;
                c += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(offset.x, 0)) * w1;
                c += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv - float2(offset.x, 0)) * w1;
                c += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(offset.x * 2, 0)) * w2;
                c += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv - float2(offset.x * 2, 0)) * w2;
                c += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(0, offset.y)) * w3;
                c += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv - float2(0, offset.y)) * w3;
                c += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(0, offset.y * 2)) * w4;
                c += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv - float2(0, offset.y * 2)) * w4;
                return c;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionHCS);
                float2 texelSize = 1.0 / _ScreenParams.xy;

                half4 blurred = SampleSceneBlur(screenUV, texelSize);
                blurred.rgb = AdjustSaturation(blurred.rgb, (half)_Saturation);
                blurred.rgb *= (half)_Brightness;

                half spriteAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a;
                half alpha = i.color.a * spriteAlpha;

                return half4(blurred.rgb * i.color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}