Shader "VFX/Lightning"
{
    Properties
    {
        _Color ("Color", Color) = (0.3, 0.8, 2.0, 1.0)
        _Speed ("Flicker Speed", Float) = 8.0
        _NoiseScale ("Noise Scale", Float) = 12.0
        _Threshold ("Alpha Threshold", Range(0.0, 1.0)) = 0.4
        _Intensity ("Emission Intensity", Float) = 3.0
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
                float4 _Color;
                float  _Speed;
                float  _NoiseScale;
                float  _Threshold;
                float  _Intensity;
            CBUFFER_END

            // Pseudo-random hash
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Simple value noise
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash(i + float2(0,0)), hash(i + float2(1,0)), u.x),
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

                // Animate noise vertically (bolt scrolling)
                float t = _Time.y * _Speed;
                float2 animUV = float2(uv.x, uv.y - t * 0.1);

                // Multi-octave noise for bolt shape
                float n  = noise(animUV * _NoiseScale);
                     n += noise(animUV * _NoiseScale * 2.0) * 0.5;
                     n  = n / 1.5;

                // Center falloff: bright in middle, fade at edges
                float edge = 1.0 - abs(uv.x - 0.5) * 2.0;
                edge = pow(edge, 3.0);

                float bolt = n * edge;

                // Flicker
                float flicker = 0.8 + 0.2 * sin(t * 30.0);
                bolt *= flicker;

                float alpha = smoothstep(_Threshold, _Threshold + 0.3, bolt);
                float3 col = _Color.rgb * _Intensity * (bolt + 0.2);

                // Vertex color modulation (from particle system)
                col *= IN.color.rgb;
                alpha *= IN.color.a;

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
