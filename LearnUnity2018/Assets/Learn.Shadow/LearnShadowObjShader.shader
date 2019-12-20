Shader "Custom/LearnShadowObjShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Gloss("Gloss", Range(8,256)) = 20
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 200
        Pass{
            // 简单实现一个lambert 显示，哎
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Gloss;

            float4 _Color;

            // 全局设置的阴影贴图
            sampler2D _CustomShadowMapTexture;
            float4x4 _Custom_World2Shadow;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                // UNITY_FOG_COORDS(2)
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);// 法线特殊处理
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // o.depth = o.vertex.zw;
                return o;
            }

            float ShadowCalculation(float3 fragWorldPos) 
            {
                float4 fragPosLightSpace = mul(_Custom_World2Shadow, float4(fragWorldPos, 1));
                float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

                projCoords = projCoords * 0.5 + 0.5;
                //projCoords.y = 1 - projCoords.y;

                // 不幸的，Unity关闭了SetBorderColor的接口
                if (projCoords.x < 0 || projCoords.x > 1) return 1;
                if (projCoords.y < 0 || projCoords.y > 1) return 1;
                if (projCoords.z >= 1) return 1;

                float depth = projCoords.z;

                float2 moments = tex2D(_CustomShadowMapTexture, projCoords.xy).rg;

                // if (depth <= moments.x) return 1;
                // else return 0;// 直接对普通的深度贴图做插值，阴影的边缘像油墨一样，比较怪异。理论上说，ShadowMap是不能做 pre-fliter 的
                
                // min_variance和bleeding_reduce，场景相关，参考下面的文章
                // > http://www.klayge.org/2013/10/07/切换到esm/

                float p_other = (depth < moments.x);

                float min_variance = 0.000002;// 减少诡异的条纹
                float variance = moments.y - (moments.x * moments.x);
                variance = max(variance, min_variance);
                
                float depth_minus_mean = depth - moments.x;
                float p_max = variance / (variance + depth_minus_mean * depth_minus_mean);

                float bleeding_reduce = 0.1;// 减小漏光
                p_max = smoothstep(bleeding_reduce, 1, p_max);

                return max(p_other, p_max);
            }

            float4 frag(v2f i) :SV_TARGET
            {
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

                float3 worldNormal = normalize(i.worldNormal);
                float3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);// 平行光

                float halfLambert = dot(worldNormal, worldLightDir) * 0.5 + 0.5;

                //halfLambert = saturate(dot(worldNormal, worldLightDir));

                float3 diffuse = tex2D(_MainTex, i.uv);

                diffuse = _LightColor0.rgb * diffuse * halfLambert;

                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                float3 halfDir = normalize(worldLightDir + viewDir);

                float3 specular = _LightColor0.rgb * pow(max(0, dot(worldNormal, halfDir)), _Gloss);

                // shadow
                float shadow_pass = ShadowCalculation(i.worldPos);

                float4 color = float4(ambient + min(shadow_pass+0.01,1)*diffuse + shadow_pass*specular, 1)*_Color;

                //UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fixed4(1,1,1,1));

                return color;
            }
            ENDCG
        }

    }
    //FallBack "Diffuse"
}
