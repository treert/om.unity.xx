Shader "Custom/LearnShadow/VSMShader"
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
                float4 worldPos: var_1;
                float4 clipPos: var_2;
            };

            #define SW1 0
            #define SW2 1

            float4x4 _Custom_World2Shadow_VSM;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld,float4(v.vertex.xyz, 1.0));
                o.clipPos = mul(_Custom_World2Shadow_VSM, o.worldPos);
                o.pos = mul(UNITY_MATRIX_VP, o.worldPos);
                // o.pos = UnityApplyLinearShadowBias(o.pos);
                o.depth = o.pos.zw;
                return o;
            }
            float2 frag(v2f i) : SV_Target
            {
                float depth = 0;
                #if SW1
                    depth = i.pos.z;
                    #if defined(UNITY_REVERSED_Z)
                        depth = 1 - depth;
                    #endif
                #elif SW2
                    depth = i.depth.x;
                    depth = 1-depth;
                #else
                    depth = i.clipPos.z;
                    depth = depth * 0.5 + 0.5;
                #endif

                float2 dxdy = float2(ddx(depth), ddy(depth));

                return float2(depth, depth*depth + 0.25 * dot(dxdy,dxdy));
                // return EncodeFloatRGBA(depth);
            }
            ENDCG
        }
    }
}
