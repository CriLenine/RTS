Shader "Hidden/FogOfWar"
{
    Properties
    {
        _PersistentTex ("Persistent Texture", 2D) = "white" {}
        _CurrentTex ("Current Texture", 2D) = "white" {}

        _PersistentColor("Persistent Color", Color) = (0, 0, 0, 0.95)
        _CurrentColor("Current Color", Color) = (0, 0, 0, 0)
        _DefaultColor("Default Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+1"
        }

        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _PersistentTex;
            sampler2D _CurrentTex;

            fixed4 _PersistentColor;
            fixed4 _CurrentColor;
            fixed4 _DefaultColor;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_PersistentTex, i.uv) + tex2D(_CurrentTex, i.uv);

                if (col.b > 0.5f)
                {
                    return _CurrentColor;
                }
                else if (col.r > 0.5f)
                {
                    return _PersistentColor;
                }
                else
                {
                    return _DefaultColor;
                }
            }
            ENDCG
        }
    }
}
