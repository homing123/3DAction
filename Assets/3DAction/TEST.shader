Shader "Unlit/TEST"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // apply fog
                float2 uv = i.uv;
                float2 center = float2(0.5f,0.5f);
                float2 offset = uv - center;

                float angle = atan2(offset.y, offset.x);
                float radius = length(offset);

                float wave = sin(angle * 10) * 0.02f;
                radius += wave;

                float2 newOffset = float2(cos(angle), sin(angle)) *  radius;
                float2 warpedUV = center + newOffset;
                fixed4 col = tex2D(_MainTex, warpedUV);
                if(col.a <= 0.01)
                {
                    return fixed4(0,0,0,1);
                }

                return col;
            }
            ENDCG
        }
    }
}
