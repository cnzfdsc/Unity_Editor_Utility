Shader "Unlit/UV4Test"
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
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                float2 uv4 : TEXCOORD3;
                float2 uv5 : TEXCOORD4;
                float2 uv6 : TEXCOORD5;
                float2 uv7 : TEXCOORD6;
            };

            struct v2f
            {
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                float2 uv4 : TEXCOORD3;
                float2 uv5 : TEXCOORD4;
                float2 uv6 : TEXCOORD5;
                float2 uv7 : TEXCOORD6;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv1 = TRANSFORM_TEX(v.uv1, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv2, _MainTex);
                o.uv3 = TRANSFORM_TEX(v.uv3, _MainTex);
                o.uv4 = TRANSFORM_TEX(v.uv4, _MainTex);
                o.uv5 = TRANSFORM_TEX(v.uv5, _MainTex);
                o.uv6 = TRANSFORM_TEX(v.uv6, _MainTex);
                o.uv7 = TRANSFORM_TEX(v.uv7, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {							
                fixed4 col = tex2D(_MainTex, i.uv1) * 0.1
							+ tex2D(_MainTex, i.uv2) * 0.1
							+ tex2D(_MainTex, i.uv3) * 0.1
							+ tex2D(_MainTex, i.uv4) * 0.1
							+ tex2D(_MainTex, i.uv5) * 0.1
							+ tex2D(_MainTex, i.uv6) * 0.1
							+ tex2D(_MainTex, i.uv7) * 0.1;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
