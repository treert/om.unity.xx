Shader "Custom/LearnShadow/SimplePBRShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        [Gamma] _Metallic("Metallic", Range(0,1)) = 0
        _Roughness("Roughness", Range(0, 1)) = 0.5
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque"}
            LOD 200
            Pass{
            // 简单PBR实现，平行光+环境光，简单模型
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            float4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Metallic;
            float _Roughness;

            static const float PI = 3.14159265359;
            
            // 全局设置的阴影贴图
            sampler2D _CustomShadowMap_VSM;
            float4x4 _Custom_World2Shadow_VSM;

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
                return o;
            }

            // VSM
            float ShadowCalculation(float3 fragWorldPos)
            {
                float4 fragPosLightSpace = mul(_Custom_World2Shadow_VSM, float4(fragWorldPos, 1));
                float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

                projCoords = projCoords * 0.5 + 0.5;
                //projCoords.y = 1 - projCoords.y;

                // 不幸的，Unity关闭了SetBorderColor的接口
                if (projCoords.x < 0 || projCoords.x > 1) return 1;
                if (projCoords.y < 0 || projCoords.y > 1) return 1;
                if (projCoords.z >= 1) return 1;

                float depth = projCoords.z;

                float2 moments = tex2D(_CustomShadowMap_VSM, projCoords.xy).rg;

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

            float BRDF_D(float3 N, float3 H, float roughness)
            {
                const float PI = 3.14159265359;
                float a2 = roughness * roughness;
                float a4 = a2 * a2;
                float NdotH = max(dot(N, H), 0);
                
                float t1 = (NdotH * NdotH)*(a4 - 1) + 1;
                float t2 = PI * t1 * t1;
                return a4 / t2;
            }

            float BRDF_G1(float3 N, float3 V, float k)
            {
                float NdotV = max(dot(N, V), 0);
                return NdotV / (NdotV * (1 - k) + k);
            }

            float BRDF_G(float3 N, float3 V, float3 L, float roughness)
            {
                float k = (roughness + 1) * (roughness + 1) / 8.0;// Kdirect

                float g1 = BRDF_G1(N, V, k);
                float g2 = BRDF_G1(N, L, k);
                return g1 * g2;
            }

            float3 BRDF_F(float3 H, float3 V, float3 F0)
            {
                float HdotV = max(dot(H, V), 0);
                return F0 + (1 - F0) * pow(1 - HdotV, 5);
            }

            float3 BRDF_All(float3 N, float3 V, float3 L, float3 albedo, float metallic, float roughness)
            {
                float3 H = normalize(V + L);

                float NdotL = max(dot(N, L), 0);
                float NdotV = max(dot(N, V), 0);

                float3 F0 = lerp(0.04, albedo, metallic);

                float D = BRDF_D(N, H, roughness);
                float3 F = BRDF_F(H, V, F0);
                float G = BRDF_G(N, V, L, roughness);

                float3 kD = 1 - F;
                kD *= 1 - metallic;

                float3 t1 = D * F * G;
                float t2 = 4 * NdotV * NdotL;
                float3 specular = t1 / max(t2, 0.001);// avoid divide zero

                float3 brdf = kD * albedo / PI + specular;
                return brdf * NdotL;
            };
           
            float4 frag(v2f i) :SV_TARGET
            {
                float3 worldNormal = normalize(i.worldNormal);
                float3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);// 平行光
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);

                float3 albedo = tex2D(_MainTex, i.uv) ;

                float3 brdf = BRDF_All(worldNormal, viewDir, worldLightDir, albedo, _Metallic, _Roughness);
                float3 L0 = brdf * _LightColor0.rgb * _Color;
                L0 *= PI;// Unity trick, Otherwise, the result is too dark.

                float shadow_pass = ShadowCalculation(i.worldPos);
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                float3 color = ambient + shadow_pass * L0;

                //UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fixed4(1,1,1,1));
                // color = color / (color + 0.001);
                return float4(color,1);
            }
            ENDCG
        }

        }
            //FallBack "Diffuse"
}
