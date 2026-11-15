// Rayons de lumière qui traversent la fenêtre : nappes additives, bords doux, poussière animée,
// fondu au contact de la géométrie (lecture de la profondeur de scène).
Shader "Portfolio/LightShaft"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (1, 0.9, 0.7, 1)
        _Intensity("Intensity", Range(0, 2)) = 0.25
        _SoftDistance("Soft Distance", Range(0.01, 2)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Shaft"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _SoftDistance;
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
                float4 screenPos : TEXCOORD1;
                float eyeDepth : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(pos.positionCS);
                output.eyeDepth = -pos.positionVS.z;
                return output;
            }

            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash21(i), hash21(i + float2(1, 0)), f.x),
                            lerp(hash21(i + float2(0, 1)), hash21(i + float2(1, 1)), f.x), f.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float sides = smoothstep(0.0, 0.18, uv.x) * smoothstep(1.0, 0.82, uv.x);
                float along = smoothstep(0.0, 0.08, uv.y) * (1.0 - uv.y * 0.75);
                float streaks = 0.65 + 0.35 * noise(float2(uv.x * 9.0, uv.y * 1.5 - _Time.y * 0.05));
                float dust = 0.85 + 0.15 * noise(uv * float2(40.0, 60.0) + _Time.y * 0.15);

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float soft = saturate((sceneDepth - input.eyeDepth) / _SoftDistance);

                half alpha = sides * along * streaks * dust * soft * _Intensity;
                return half4(_Color.rgb * alpha, 1);
            }
            ENDHLSL
        }
    }
}
