// Contour lumineux de l'objet présenté : copie du maillage gonflée le long des normales, faces arrière seules.
// Il pulse doucement et reste visible même de loin, sur fond clair comme sombre.
Shader "Portfolio/Outline"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (1.6, 1.15, 0.45, 1)
        _Width("Width (m)", Range(0, 0.05)) = 0.014
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+10" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Width;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = normalize(TransformObjectToWorldNormal(input.normalOS));
                // Largeur constante à l'écran : plus épaisse quand l'objet est loin, pour rester lisible.
                float distance = length(GetCameraPositionWS() - positionWS);
                float pulse = 0.85 + 0.15 * sin(_Time.y * 4.0);
                positionWS += normalWS * _Width * pulse * clamp(distance / 4.0, 0.6, 2.5);
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half pulse = 0.75 + 0.25 * sin(_Time.y * 4.0);
                return half4(_Color.rgb * pulse, 1);
            }
            ENDHLSL
        }
    }
}
