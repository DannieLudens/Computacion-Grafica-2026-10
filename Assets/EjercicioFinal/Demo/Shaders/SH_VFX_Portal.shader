Shader "VFX/Portal"
{
    Properties
    {
        _NoiseTex  ("Noise Texture", 2D) = "white" {}
        _RotSpeed  ("Rotation Speed", Float) = 0.6
        _InnerColor ("Inner Color", Color) = (0.4, 0.0, 0.8, 1.0)
        _RimColor   ("Rim Color",   Color) = (0.0, 1.0, 0.8, 1.0)
        _RimPower   ("Rim Power",   Range(1.0, 8.0)) = 3.0
        _Intensity  ("Emission Intensity", Float) = 2.0
        _NoiseScale ("Noise Scale", Float) = 2.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 viewDirWS  : TEXCOORD2;
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                float  _RotSpeed;
                float4 _InnerColor;
                float4 _RimColor;
                float  _RimPower;
                float  _Intensity;
                float  _NoiseScale;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            // Rotate UV around center (0.5, 0.5)
            float2 RotateUV(float2 uv, float angle)
            {
                float2 centered = uv - 0.5;
                float s = sin(angle);
                float c = cos(angle);
                float2 rotated;
                rotated.x = centered.x * c - centered.y * s;
                rotated.y = centered.x * s + centered.y * c;
                return rotated + 0.5;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS  = TransformObjectToWorldNormal(IN.normalOS);
                float3 posWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - posWS);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;
                float2 uv = IN.uv;

                // Swirl: rotate UV with increasing speed toward center
                float dist = length(uv - 0.5);
                float swirlAngle = t * _RotSpeed + (1.0 - dist) * 4.0;
                float2 swirlUV = RotateUV(uv, swirlAngle);

                // Sample noise with swirled UVs
                float2 noiseUV = swirlUV * _NoiseScale;
                float n1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;
                // Second layer counter-rotating
                float2 swirlUV2 = RotateUV(uv, -swirlAngle * 0.7 + 1.5);
                float n2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, swirlUV2 * _NoiseScale * 1.4).r;
                float noiseVal = (n1 + n2) * 0.5;

                // Circular mask
                float circle = 1.0 - smoothstep(0.42, 0.5, dist);

                // Fresnel rim
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(N, V)), _RimPower);

                // Combine colors
                float3 inner = _InnerColor.rgb * noiseVal;
                float3 rim   = _RimColor.rgb * fresnel;
                float3 col   = (inner + rim) * _Intensity;

                // Alpha: portal interior + bright rim
                float alpha = circle * (noiseVal * 0.7 + 0.3) + fresnel * 0.8;
                alpha = saturate(alpha);

                col *= IN.color.rgb;
                alpha *= IN.color.a;

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
