// Écrans entièrement procéduraux (aucune texture) : cours de marché, jeu, code, tableau de bord, appli mobile.
// _Power anime l'allumage de l'écran de 0 à 1.
Shader "Portfolio/Screen"
{
    Properties
    {
        [Enum(Chart,0,Game,1,Code,2,Dashboard,3,App,4)] _Mode("Mode", Float) = 0
        _Power("Power", Range(0, 1)) = 1
        _BgColor("Background", Color) = (0.05, 0.06, 0.08, 1)
        _ColorA("Color A", Color) = (0.5, 0.8, 0.75, 1)
        _ColorB("Color B", Color) = (0.85, 0.7, 0.45, 1)
        _Intensity("Intensity", Range(0, 4)) = 1.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Screen"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BgColor;
                half4 _ColorA;
                half4 _ColorB;
                half _Mode;
                half _Power;
                half _Intensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half fogFactor : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 normalWS : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float hash11(float n) { return frac(sin(n * 127.1) * 43758.5453); }

            float vnoise(float x)
            {
                float i = floor(x);
                float f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(hash11(i), hash11(i + 1.0), f);
            }

            float sdBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            float fillBox(float2 p, float2 center, float2 halfSize, float r)
            {
                return smoothstep(0.004, -0.004, sdBox(p - center, halfSize, r));
            }

            float price(float x)
            {
                float n = 0.55 * vnoise(x * 3.0) + 0.3 * vnoise(x * 7.3 + 4.0) + 0.15 * vnoise(x * 17.0 + 9.0);
                return 0.2 + 0.5 * n;
            }

            half3 chart(float2 uv, float t)
            {
                half3 col = _BgColor.rgb;
                float grid = max(step(frac(uv.x * 8.0), 0.012), step(frac(uv.y * 6.0), 0.02));
                col += _ColorA.rgb * grid * 0.07;

                float x = uv.x * 1.4 + t * 0.03;
                float y = clamp(price(x) + uv.x * 0.22, 0.14, 0.86);
                float under = step(uv.y, y);
                col += _ColorA.rgb * under * saturate(0.3 - (y - uv.y) * 0.75);
                col += _ColorA.rgb * smoothstep(0.014, 0.004, abs(uv.y - y)) * 1.4;

                float column = floor(uv.x * 30.0);
                float barHeight = 0.03 + 0.09 * hash11(column + floor(t * 0.4));
                float bar = step(uv.y, barHeight) * step(0.2, frac(uv.x * 30.0));
                col += _ColorB.rgb * bar * 0.55;

                col += _ColorB.rgb * fillBox(uv, float2(0.14, 0.9), float2(0.09, 0.028), 0.02) * 0.8;
                col += _ColorA.rgb * fillBox(uv, float2(0.32, 0.9), float2(0.06, 0.018), 0.015) * 0.5;
                return col;
            }

            half3 game(float2 uv, float t)
            {
                half3 col = _BgColor.rgb;
                float horizon = 0.46;
                if (uv.y < horizon)
                {
                    float depth = 0.12 / (horizon - uv.y + 0.012);
                    float gx = abs(frac((uv.x - 0.5) * depth * 2.6) - 0.5);
                    float gz = abs(frac(depth * 1.1 + t * 0.7) - 0.5);
                    float lines = smoothstep(0.43, 0.5, gx) + smoothstep(0.43, 0.5, gz);
                    col += _ColorA.rgb * saturate(lines) * saturate((horizon - uv.y) * 3.2) * 0.9;
                }
                else
                {
                    col += _ColorB.rgb * (1.0 - uv.y) * 0.22;
                    float d = length((uv - float2(0.5, 0.63)) * float2(1.55, 1.0));
                    float sun = smoothstep(0.2, 0.19, d) * step(0.3, frac(uv.y * 24.0) + (uv.y - 0.55) * 2.0);
                    col += _ColorB.rgb * sun * 0.95;
                }
                col += _ColorA.rgb * fillBox(uv, float2(0.16, 0.92), float2(0.11, 0.016), 0.012) * 0.7;
                col += _ColorB.rgb * fillBox(uv, float2(0.86, 0.92), float2(0.08, 0.016), 0.012) * 0.7;
                return col;
            }

            half3 code(float2 uv, float t)
            {
                half3 col = _BgColor.rgb;
                float rows = 17.0;
                float fy = (1.0 - uv.y) * rows;
                float row = floor(fy);
                float band = step(0.3, frac(fy)) * step(frac(fy), 0.7);
                float typed = step(row, fmod(t * 1.6, rows + 6.0));

                float indent = 0.07 + floor(hash11(row * 3.1) * 4.0) * 0.05;
                float len = 0.16 + 0.5 * hash11(row * 7.7);
                float inside = step(indent, uv.x) * step(uv.x, indent + len);
                float wordPos = (uv.x - indent) * 10.0 + hash11(row) * 5.0;
                float word = step(0.2, frac(wordPos));
                float kind = hash11(floor(wordPos) + row * 13.0);
                half3 wordColor = kind < 0.4 ? _ColorA.rgb : (kind < 0.7 ? _ColorB.rgb : half3(0.62, 0.65, 0.7));
                col += wordColor * band * inside * word * typed * 0.95;

                float gutter = step(0.02, uv.x) * step(uv.x, 0.04);
                col += half3(0.3, 0.32, 0.36) * band * gutter;
                return col;
            }

            half3 dashboard(float2 uv, float t)
            {
                half3 col = _BgColor.rgb;
                for (int i = 0; i < 3; i++)
                {
                    float2 center = float2(0.19 + i * 0.31, 0.86);
                    col += lerp(_ColorA.rgb, _ColorB.rgb, i * 0.5) * fillBox(uv, center, float2(0.13, 0.06), 0.03) * 0.5;
                }

                float2 b = (uv - float2(0.06, 0.1)) / float2(0.5, 0.6);
                if (b.x > 0.0 && b.x < 1.0 && b.y > 0.0 && b.y < 1.0)
                {
                    float index = floor(b.x * 6.0);
                    float height = 0.25 + 0.65 * hash11(index * 1.7 + 2.0) * (0.88 + 0.12 * sin(t * 0.8 + index));
                    float bar = step(b.y, height) * step(0.22, frac(b.x * 6.0)) * step(frac(b.x * 6.0), 0.82);
                    col += lerp(_ColorA.rgb, _ColorB.rgb, index / 6.0) * bar * 0.85;
                }

                float2 d = (uv - float2(0.79, 0.4)) * float2(1.0, 1.25);
                float radius = length(d);
                float ring = smoothstep(0.2, 0.19, radius) * smoothstep(0.1, 0.11, radius);
                float angle = atan2(d.y, d.x) / 6.2831853 + 0.5;
                col += (angle < 0.62 ? _ColorA.rgb : _ColorB.rgb) * ring * 0.9;
                return col;
            }

            half3 app(float2 uv, float t)
            {
                half3 col = _BgColor.rgb;
                col += _ColorA.rgb * step(0.9, uv.y) * 0.5;
                col += _ColorB.rgb * step(0.86, uv.y) * step(uv.y, 0.875) * step(uv.x, frac(t * 0.05) * 0.8 + 0.2) * 0.9;
                col += half3(0.75, 0.75, 0.72) * fillBox(uv, float2(0.5, 0.66), float2(0.36, 0.11), 0.04) * 0.55;

                float selected = floor(fmod(t * 0.45, 3.0));
                for (int i = 0; i < 3; i++)
                {
                    float2 center = float2(0.5, 0.4 - i * 0.13);
                    half3 pill = (i == (int)selected) ? _ColorB.rgb : _ColorA.rgb * 0.55;
                    col += pill * fillBox(uv, center, float2(0.36, 0.045), 0.045) * 0.9;
                }
                return col;
            }

            half4 frag(Varyings input, bool front : SV_IsFrontFace) : SV_Target
            {
                float2 uv = input.uv;
                float t = _Time.y;

                half3 content;
                if (_Mode < 0.5) content = chart(uv, t);
                else if (_Mode < 1.5) content = game(uv, t);
                else if (_Mode < 2.5) content = code(uv, t);
                else if (_Mode < 3.5) content = dashboard(uv, t);
                else content = app(uv, t);

                // Allumage façon tube cathodique : une ligne s'ouvre puis l'image apparaît.
                float open = saturate(_Power * 1.6);
                float mask = smoothstep(0.0, 0.03, 0.5 * open - abs(uv.y - 0.5));
                float flash = (1.0 - saturate(_Power * 2.5)) * step(0.001, _Power) * smoothstep(0.03, 0.0, abs(uv.y - 0.5));

                float2 edge = min(uv, 1.0 - uv);
                float vignette = smoothstep(0.0, 0.06, min(edge.x, edge.y));
                float scan = 0.94 + 0.06 * sin(uv.y * 420.0);

                half3 off = half3(0.012, 0.014, 0.018);
                half3 col = lerp(off, content * _Intensity * scan * (0.75 + 0.25 * vignette), mask * saturate(_Power * 1.4));
                col += _ColorA.rgb * flash * 2.0;

                // Vitre de l'écran : elle reflète la pièce (sonde de réflexion), surtout vue de biais.
                half3 normal = normalize(input.normalWS) * (front ? 1.0 : -1.0);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half fresnel = 0.04 + 0.96 * pow(1.0 - saturate(dot(normal, view)), 5.0);
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                col += GlossyEnvironmentReflection(reflect(-view, normal), input.positionWS, 0.12, 1.0, screenUV) * fresnel;

                col = MixFog(col, input.fogFactor);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
