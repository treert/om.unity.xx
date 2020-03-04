Shader "SimplePBRShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        [Gamma] _Metallic("Metallic", Range(0,1)) = 0
        _Glossiness("Smothness", Range(0, 1)) = 0.5 // 方便对比Standard
        //_Roughness("Roughness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque"}
        LOD 200
        Pass{
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // shadow on 不加默认是关闭的
            #pragma multi_compile_fwdbase
                
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            float4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Metallic;
            float _Glossiness;

            static const float PI = 3.14159265359;

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
                SHADOW_COORDS(3)
                UNITY_FOG_COORDS(4)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);// 法线特殊处理
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            float BRDF_D(float3 N, float3 H, float roughness)
            {
                const float PI = 3.14159265359;
                float a2 = roughness * roughness;
                float a4 = a2 * a2;
                float NdotH = max(dot(N, H), 0);

                float t1 = (NdotH * NdotH) * (a4 - 1) + 1;
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
                //kD *= 1 - metallic;

                float3 DFG = D * F * G;
                float3 specular = DFG / max(4 * NdotV * NdotL, 0.001);// avoid divide zero

                float3 brdf = kD * albedo / PI + specular;
                return brdf * NdotL;
            };

            float3 Get_EnvColor(float3 N, float3 V, float3 albedo, float metallic, float roughness)
            {
                // https://github.com/Arcob/UnityPbrRendering/blob/master/Assets/unity%20pbr/Height2.shader
                // 又瞎调了一遍参数，效果勉强有些的样子，Orz。得找找关于天空盒子之类的文章。
                // 发现竟然和Unity的Standard差不多，赞一个。

                float3 F0 = lerp(0.04, albedo, metallic);
                float NdotV = max(dot(N, V), 0);
                float3 F = F0 + (max(1.0 - roughness, F0) - F0) * pow(1.0 - NdotV, 5.0);
                // float3 F = F0 + (1 - F0) * pow(1.0 - NdotV, 5.0);

                float3 reflectViewDir = reflect(-V, N);
                float4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectViewDir, roughness*10);
                float3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);

                //float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                float3 ambient = ShadeSH9(float4(N, 1));
                float3 kD = 1 - F;
                // kD *= 1 - metallic;
                return skyColor * F + ambient * kD * albedo/* / PI*/;
            }

            float4 frag(v2f i) :SV_TARGET
            {
                float3 worldNormal = normalize(i.worldNormal);
                float3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);// 平行光
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);

                float3 albedo = tex2D(_MainTex, i.uv) * _Color;

                float _Roughness = 1 - _Glossiness*0.95;// 没有绝对光滑的物体，这样就和Unity更接近了

                float3 brdf = BRDF_All(worldNormal, viewDir, worldLightDir, albedo, _Metallic, _Roughness);
                float3 L0 = brdf * _LightColor0.rgb;
                L0 *= PI;// Unity trick, Otherwise, the result is too dark.

                float shadow_pass = SHADOW_ATTENUATION(i);

                float3 envColor = Get_EnvColor(worldNormal, viewDir, albedo, _Metallic, _Roughness);
                float3 color = envColor + shadow_pass * L0;

                UNITY_APPLY_FOG(i.fogCoord, color);
                return float4(color,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
