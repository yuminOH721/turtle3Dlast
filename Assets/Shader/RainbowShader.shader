Shader "Unlit/Rainbow"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 HueToRGB(float h)
            {
                float r = abs(h * 6 - 3) - 1;
                float g = 2 - abs(h * 6 - 2);
                float b = 2 - abs(h * 6 - 4);
                return saturate(float3(r, g, b));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float hue = frac(i.uv.x * 5.0);  // 정적 무지개
                float3 rgb = HueToRGB(hue);
                return fixed4(rgb, 1.0);
            }
            ENDCG
        }
    }
}
