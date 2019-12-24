Shader "Custom/LearnShadow/ESMShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
            Fog{ Mode Off }
            ZWrite On ZTest LEqual Cull Back
            Offset 1, 1
            // ColorMask RG
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 depth: TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // o.pos = UnityApplyLinearShadowBias(o.pos);
                o.depth = o.pos.zw;
                return o;
            }
            float frag(v2f i) : SV_Target
            {
                float depth = i.depth.x / i.depth.y;
                // depth = i.pos.z;
#if defined(UNITY_REVERSED_Z)
                depth = 1 - depth;
#endif
                return depth;
            }
            ENDCG
        }
    }
}
