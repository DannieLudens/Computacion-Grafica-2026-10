Shader "VFX/Holy"
{
    Properties
    {
        _CoreColor  ("Core Color",  Color) = (1.0, 0.95, 0.6, 1.0)
        _RimColor   ("Rim Color",   Color) = (1.0, 1.0,  1.0, 1.0)
        _Speed      ("Pulse Speed", Float) = 2.0
        _Intensity  ("Emission Intensity", Float) = 3.0
        _RimPower   ("Rim Power",   Range(1,8)) = 2.5
        _NoiseScale ("Noise Scale", Float) = 4.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha One
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
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _CoreColor;
                float4 _RimColor;
                float  _Speed;
                float  _Intensity;
                float  _RimPower;
                float  _NoiseScale;
            CBUFFER_END

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash(i), hash(i + float2(1,0)), u.x),
                    lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), u.x),
                    u.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float t = _Time.y * _Speed;

                // Animated shimmer noise
                float n  = noise(uv * _NoiseScale + float2(0, t));
                     n += noise(uv * _NoiseScale * 2.0 - float2(t * 0.5, 0)) * 0.5;
                     n  = n / 1.5;

                // Soft radial mask (bright center)
                float edge    = 1.0 - abs(uv.x - 0.5) * 2.0;
                float rimGlow = pow(1.0 - edge, _RimPower);

                // Pulsing core
                float pulse = 0.75 + 0.25 * sin(t * 3.0 + uv.y * 6.0);

                // Color mix: golden core + white rim
                float3 col = lerp(_CoreColor.rgb, _RimColor.rgb, rimGlow);
                col *= (n * 0.6 + 0.4) * pulse * _Intensity;

                float alpha = smoothstep(0.05, 0.4, n * edge * pulse + rimGlow * 0.5);
                alpha *= IN.color.a;
                col   *= IN.color.rgb;

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
