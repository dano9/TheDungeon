Shader "URP/UI/PolarShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha ("Transparency", Range (0, 1)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Geometry"
        }

        Pass
        {
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // user defined variables
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _Alpha;

            // vertex input
            struct appdata
            {
                float4 pos : POSITION;
                half2 uv : TEXCOORD0;
            };

            // vertex output
            struct v2f
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.pos);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half2 scale = half2(1, 1);
                half2 position = _MainTex_ST.xy;
                half2 polarScale = half2(1, 1);
                half2 polarPosition = _MainTex_ST.zw;

                // UVs
                half2 offset = half2 (0.5, 0.5) + position;
                half t = atan((offset.x - i.uv.x * scale.x) / (offset.y - i.uv.y * scale.y)) / 3.14159265359f;
                half r = distance(offset, i.uv * scale);

                half4 tex = tex2D(_MainTex, half2 (t, r) * polarScale + polarPosition);

                return half4(tex.rgb, _Alpha);
            }
            ENDHLSL
        }
    }
}