Shader "Unlit/HitRangeCircle"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ProgressAlpha("ProgressAlpha", Range(0,1)) = 0.5
        _BGAlpha("BGAlpha", Range(0,1)) = 0.5
        _Color("Color", Color) = (1,1,1,1)
        _Progress("Progress", Range(0,1)) = 0
    }
    SubShader
    {
        Tags
        { 
            "Queue" = "AlphaTest"
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 posCS : SV_POSITION;
            };

            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _BGAlpha;
            float _ProgressAlpha;
            CBUFFER_END
            float _Progress;
            float4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.posCS = TransformObjectToHClip(v.posOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half alpha = tex2D(_MainTex, i.uv).a;
                clip(alpha - 0.1f);      
                half4 col = half4(_Color.rgb, 0);
                float2 center = float2(0.5f, 0.5f);
                float dis = length(center - i.uv);
                
                col.a = dis < _Progress * 0.5f ? _ProgressAlpha : _BGAlpha;
                return col;
            }
            ENDHLSL
        }
    }
}
