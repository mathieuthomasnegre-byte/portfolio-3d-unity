// Ciel visible par la fenêtre : dégradé, astre (soleil ou lune) et étoiles optionnelles.
Shader "Portfolio/Sky"
{
    Properties
    {
        _Top("Top", Color) = (0.55, 0.7, 0.85, 1)
        _Bottom("Bottom", Color) = (0.95, 0.88, 0.75, 1)
        [HDR] _DiscColor("Disc Color", Color) = (1, 0.95, 0.8, 1)
        _DiscPos("Disc Position (uv)", Vector) = (0.6, 0.7, 0, 0)
        _DiscSize("Disc Size", Range(0, 0.3)) = 0.06
        _Stars("Stars", Range(0, 1)) = 0
        _Intensity("Intensity", Range(0, 4)) = 1.2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Sky"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Top;
                half4 _Bottom;
                half4 _DiscColor;
                float4 _DiscPos;
                half _DiscSize;
                half _Stars;
                half _Intensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                half3 col = lerp(_Bottom.rgb, _Top.rgb, smoothstep(0.0, 1.0, uv.y));

                float d = length(uv - _DiscPos.xy);
                col += _DiscColor.rgb * smoothstep(_DiscSize, _DiscSize * 0.85, d);
                col += _DiscColor.rgb * 0.25 * smoothstep(_DiscSize * 4.0, 0.0, d);

                float2 cell = floor(uv * 70.0);
                float star = step(0.985, hash21(cell)) * smoothstep(0.35, 0.0, length(frac(uv * 70.0) - 0.5));
                float twinkle = 0.7 + 0.3 * sin(_Time.y * 1.5 + hash21(cell + 3.0) * 6.28);
                col += star * twinkle * _Stars;

                return half4(col * _Intensity, 1);
            }
            ENDHLSL
        }
    }
}
