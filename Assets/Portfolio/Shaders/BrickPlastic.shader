// Matière des briques, du mobilier et des murs : PBR URP + liseré de lumière + pulsation de mise en avant.
// _Surface ajoute une texture calculée (aucune image) : 1 parquet, 2 veinage du bois, 3 tissu, 4 peinture murale.
Shader "Portfolio/BrickPlastic"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.8, 0.8, 0.8, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0.45
        _Metallic("Metallic", Range(0, 1)) = 0
        [HDR] _EmissionColor("Emission", Color) = (0, 0, 0, 1)
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.5, 8)) = 3
        _RimStrength("Rim Strength", Range(0, 1)) = 0.12
        [HDR] _HighlightColor("Highlight Color", Color) = (1, 0.78, 0.45, 1)
        _Highlight("Highlight", Range(0, 1)) = 0
        _Surface("Surface (0 uni, 1 parquet, 2 bois, 3 tissu, 4 mur)", Float) = 0
        _SurfaceStrength("Surface Strength", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _EmissionColor;
            half4 _RimColor;
            half4 _HighlightColor;
            half _Smoothness;
            half _Metallic;
            half _RimPower;
            half _RimStrength;
            half _Highlight;
            half _Surface;
            half _SurfaceStrength;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
                float3 patternPos : TEXCOORD3;
                half3 normalOS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash21(i), Hash21(i + float2(1, 0)), f.x),
                            lerp(Hash21(i + float2(0, 1)), Hash21(i + float2(1, 1)), f.x), f.y);
            }

            // Efface un détail quand il devient plus fin qu'un pixel (évite le scintillement de loin).
            float Fine(float coordinate)
            {
                return saturate(1.0 - fwidth(coordinate) * 1.4);
            }

            // uv en mètres sur la face. Renvoie la teinte, le relief (en mètres) et la brillance de la matière.
            void SurfacePattern(float2 uv, out half tint, out float height, out half gloss)
            {
                tint = 1.0;
                height = 0.0;
                gloss = 1.0;

                if (_Surface < 0.5) return;

                if (_Surface < 1.5)
                {
                    // Parquet : lames de 24 cm sur 1,5 m, décalées d'un rang à l'autre.
                    const float plankWidth = 0.24, plankLength = 1.5;
                    float row = floor(uv.y / plankWidth);
                    float2 q = float2(uv.x + Hash21(float2(row, 3.1)) * 1.7, uv.y);
                    float id = Hash21(float2(floor(q.x / plankLength), row));
                    float2 f = float2(frac(q.x / plankLength) * plankLength, frac(q.y / plankWidth) * plankWidth);
                    float edge = min(min(f.x, plankLength - f.x), min(f.y, plankWidth - f.y));
                    // Joint élargi à la taille d'un pixel et éclairci d'autant : pas de pointillés de loin.
                    float pixel = max(fwidth(uv.x), fwidth(uv.y));
                    float jointWidth = max(0.007, pixel * 1.6);
                    float jointDepth = saturate(0.007 / jointWidth);
                    float joint = lerp(1.0, smoothstep(0.0, jointWidth, edge), jointDepth);
                    float grain = Noise(float2(q.x * 2.6 + id * 20.0, q.y * 46.0)) * 0.6
                                  + Noise(float2(q.x * 8.0, q.y * 150.0 + id * 7.0)) * 0.4 * Fine(q.y * 150.0);
                    tint = (0.9 + 0.2 * id) * (0.93 + 0.14 * grain) * lerp(0.7, 1.0, joint);
                    height = joint * 0.0025 + grain * 0.0003;
                    gloss = lerp(0.5, 1.0, joint) * (0.88 + 0.24 * grain);
                }
                else if (_Surface < 2.5)
                {
                    // Bois des meubles : veinage étiré dans la longueur.
                    float grain = Noise(float2(uv.x * 3.5, uv.y * 60.0)) * 0.6
                                  + Noise(float2(uv.x * 10.0, uv.y * 170.0)) * 0.4 * Fine(uv.y * 170.0);
                    tint = 0.92 + 0.16 * grain;
                    height = grain * 0.0003;
                    gloss = 0.86 + 0.28 * grain;
                }
                else if (_Surface < 3.5)
                {
                    // Tissu : trame croisée et légères variations de teinte.
                    float2 w = uv * 70.0;
                    float weave = sin(w.x * 6.2832) * sin(w.y * 6.2832) * Fine(w.x) * Fine(w.y);
                    float cloud = Noise(uv * 5.0) * 0.65 + Noise(uv * 17.0) * 0.35;
                    tint = 0.94 + 0.1 * cloud + 0.05 * weave;
                    height = weave * 0.0007 + cloud * 0.0006;
                    gloss = 0.75;
                }
                else
                {
                    // Peinture murale : grandes nuances douces et grain fin.
                    float cloud = Noise(uv * 1.1) * 0.6 + Noise(uv * 3.7) * 0.4;
                    float grain = (Noise(uv * 120.0) - 0.5) * Fine(uv.x * 120.0);
                    tint = 0.955 + 0.075 * cloud + 0.03 * grain;
                }

                tint = lerp(1.0, tint, _SurfaceStrength);
                gloss = lerp(1.0, gloss, _SurfaceStrength);
                height *= _SurfaceStrength;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(pos.positionCS.z);

                // Coordonnées de la texture en mètres, attachées à l'objet : elle le suit quand il bouge.
                float3 objectScale = float3(length(UNITY_MATRIX_M._m00_m10_m20), length(UNITY_MATRIX_M._m01_m11_m21),
                    length(UNITY_MATRIX_M._m02_m12_m22));
                output.patternPos = input.positionOS.xyz * objectScale;
                output.normalOS = input.normalOS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalize(input.normalWS);

                // Face dominante de l'objet : on y projette la texture à plat.
                half3 facing = abs(input.normalOS);
                float2 uv = facing.y > max(facing.x, facing.z) ? input.patternPos.xz
                    : (facing.x > facing.z ? input.patternPos.zy : input.patternPos.xy);
                half tint;
                float height;
                half gloss;
                SurfacePattern(uv, tint, height, gloss);

                // Relief : la normale s'incline selon la pente de la hauteur (dérivées écran).
                float3 dpx = ddx(input.positionWS), dpy = ddy(input.positionWS);
                float3 r1 = cross(dpy, inputData.normalWS), r2 = cross(inputData.normalWS, dpx);
                float det = dot(dpx, r1);
                // Pente bornée : aux arêtes, la projection change de face et la hauteur saute.
                float slopeX = clamp(ddx(height), -0.6 * length(dpx), 0.6 * length(dpx));
                float slopeY = clamp(ddy(height), -0.6 * length(dpy), 0.6 * length(dpy));
                float3 gradient = sign(det) * (slopeX * r1 + slopeY * r2);
                inputData.normalWS = normalize(abs(det) * inputData.normalWS - gradient);

                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                half fresnel = pow(1.0 - saturate(dot(inputData.normalWS, inputData.viewDirectionWS)), _RimPower);
                half pulse = 0.6 + 0.4 * sin(_Time.y * 3.2);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = _BaseColor.rgb * tint;
                surface.metallic = _Metallic;
                surface.specular = half3(0, 0, 0);
                surface.smoothness = saturate(_Smoothness * gloss);
                surface.occlusion = 1;
                surface.alpha = 1;
                surface.emission = _EmissionColor.rgb
                    + _HighlightColor.rgb * _Highlight * pulse * (0.07 + fresnel * 0.9);

                half4 color = UniversalFragmentPBR(inputData, surface);

                // Liseré : une fine lumière sur les arêtes, teintée par la lumière qui atteint la surface.
                Light mainLight = GetMainLight(inputData.shadowCoord);
                half3 rimLight = inputData.bakedGI + mainLight.color * mainLight.shadowAttenuation * 0.5;
                color.rgb += _RimColor.rgb * rimLight * fresnel * _RimStrength;

                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return half4(color.rgb, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(positionCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return input.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(normalize(input.normalWS), 0);
            }
            ENDHLSL
        }
    }
}
