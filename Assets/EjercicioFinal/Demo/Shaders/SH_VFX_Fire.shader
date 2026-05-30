Shader "VFX/Fire"
{
    Properties
    {
        _Speed    ("Scroll Speed", Float) = 1.2
        _NoiseScale ("Noise Scale", Float) = 3.5
        _Distortion ("Distortion Amount", Float) = 0.15
        _Intensity ("Emission Intensity", Float) = 2.5
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
                float _Speed;
                float _NoiseScale;
                float _Distortion;
                float _Intensity;
            CBUFFER_END

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Smooth value noise
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

            // Voronoi-ish: layered noise for fire texture
            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    v += a * noise(p);
                    p  = p * 2.0 + float2(1.7, 9.2);
                    a *= 0.5;
                }
                return v;
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

                // Horizontal distortion (fire sway)
                float sway = sin(uv.y * 4.0 + t * 3.0) * _Distortion;
                uv.x += sway;

                // Scroll UVs upward for fire movement
                float2 scrollUV = uv * _NoiseScale + float2(0, -t);
                float f = fbm(scrollUV);

                // Shape mask: narrow at top, wide at base
                float heightMask = 1.0 - uv.y;
                heightMask = pow(heightMask, 1.5);
                float edgeMask = 1.0 - abs(uv.x - 0.5) * 2.0;
                edgeMask = pow(edgeMask, 0.7);
                float shape = heightMask * edgeMask;

                float fire = f * shape;

                // Fire color gradient: black → deep red → orange → yellow → white
                float3 col;
                float h = fire;
                col  = lerp(float3(0,0,0),     float3(0.8,0.05,0),   smoothstep(0.0, 0.25, h));
                col  = lerp(col,                float3(1.0, 0.3, 0),  smoothstep(0.25, 0.5, h));
                col  = lerp(col,                float3(1.0, 0.8, 0),  smoothstep(0.5, 0.75, h));
                col  = lerp(col,                float3(1.0, 1.0, 0.8), smoothstep(0.75, 1.0, h));

                float alpha = smoothstep(0.05, 0.35, fire);

                col *= _Intensity;
                col *= IN.color.rgb;
                alpha *= IN.color.a;

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
