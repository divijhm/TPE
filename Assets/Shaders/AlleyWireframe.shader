Shader "Custom/AlleyWireframe"
{
    Properties
    {
        _Color         ("Albedo",         Color)              = (0.75, 0.75, 0.75, 1)
        _LineColor     ("Edge Color",     Color)              = (0.06, 0.06, 0.06, 1)
        _LineThickness ("Edge Thickness", Range(0.0, 0.05))   = 0.006
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        // ── Pass 1 : Solid lit surface ──────────────────────────────────────
        Pass
        {
            Name "SOLID"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos        : SV_POSITION;
                float3 worldNormal: TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos         = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 n    = normalize(i.worldNormal);
                float  ndl  = max(0.0, dot(n, _WorldSpaceLightPos0.xyz));
                float3 amb  = ShadeSH9(half4(n, 1));
                float3 lit  = _LightColor0.rgb * ndl + amb;
                return fixed4(_Color.rgb * lit, 1.0);
            }
            ENDCG
        }

        // ── Pass 2 : Wireframe edge lines via geometry shader ───────────────
        Pass
        {
            Name "WIREFRAME"
            Tags  { "LightMode" = "Always" }
            // pull lines slightly in front so they don't z-fight
            Offset -1, -1
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vert_w
            #pragma geometry geom_w
            #pragma fragment frag_w
            #pragma target 4.0
            #include "UnityCG.cginc"

            struct av { float4 vertex : POSITION; };

            struct v2g { float4 pos : SV_POSITION; };

            struct g2f
            {
                float4 pos  : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            fixed4 _LineColor;
            float  _LineThickness;

            v2g vert_w(av v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom_w(triangle v2g IN[3], inout TriangleStream<g2f> stream)
            {
                g2f o;
                o.pos = IN[0].pos; o.bary = float3(1,0,0); stream.Append(o);
                o.pos = IN[1].pos; o.bary = float3(0,1,0); stream.Append(o);
                o.pos = IN[2].pos; o.bary = float3(0,0,1); stream.Append(o);
            }

            fixed4 frag_w(g2f i) : SV_Target
            {
                float  minB  = min(i.bary.x, min(i.bary.y, i.bary.z));
                float  delta = fwidth(minB) * 1.5;
                float  edge  = smoothstep(0.0, delta, minB - _LineThickness);
                // discard fully interior fragments — only draw near edges
                clip(0.999 - edge);
                return fixed4(_LineColor.rgb, 1.0 - edge);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
