Shader "Hidden/CompositeEffects"
{
    HLSLINCLUDE

        #pragma target 2.0
        #pragma editor_sync_compilation
        #pragma multi_compile _ DISABLE_TEXTURE2D_X_ARRAY
        #pragma multi_compile _ BLIT_SINGLE_SLICE
        // Core.hlsl for XR dependencies
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        texture2D _CameraDepthTexture;

    ENDHLSL
    
    SubShader 
    {
        Tags{ "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment fp

            float4 fp(Varyings input) : SV_Target {
                return _CameraDepthTexture.Sample(sampler_PointClamp, input.texcoord.xy).r;
            }

            ENDHLSL
        }

        // 9-Tap Catmull-Rom filtering from: https://gist.github.com/TheRealMJP/c83b8c0f46b63f3a88a5986f4fa982b1
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment fp

            float4 fp(Varyings input) : SV_Target {
                float2 samplePos = input.texcoord.xy * _BlitTextureSize.zw;
                float2 texPos1 = floor(samplePos - 0.5f) + 0.5f;

                float2 f = samplePos - texPos1;

                float2 w0 = f * (-0.5f + f * (1.0f - 0.5f * f));
                float2 w1 = 1.0f + f * f * (-2.5f + 1.5f * f);
                float2 w2 = f * (0.5f + f * (2.0f - 1.5f * f));
                float2 w3 = f * f * (-0.5f + 0.5f * f);

                float2 w12 = w1 + w2;
                float2 offset12 = w2 / (w1 + w2);

                float2 texPos0 = texPos1 - 1;
                float2 texPos3 = texPos1 + 2;
                float2 texPos12 = texPos1 + offset12;

                texPos0 /= _BlitTextureSize.zw;
                texPos3 /= _BlitTextureSize.zw;
                texPos12 /= _BlitTextureSize.zw;

                float4 result = 0.0f;
                result += tex2D(_BlitTexture, float2(texPos0.x, texPos0.y)) * w0.x * w0.y;
                result += tex2D(_BlitTexture, float2(texPos12.x, texPos0.y)) * w12.x * w0.y;
                result += tex2D(_BlitTexture, float2(texPos3.x, texPos0.y)) * w3.x * w0.y;

                result += tex2D(_BlitTexture, float2(texPos0.x, texPos12.y)) * w0.x * w12.y;
                result += tex2D(_BlitTexture, float2(texPos12.x, texPos12.y)) * w12.x * w12.y;
                result += tex2D(_BlitTexture, float2(texPos3.x, texPos12.y)) * w3.x * w12.y;

                result += tex2D(_BlitTexture, float2(texPos0.x, texPos3.y)) * w0.x * w3.y;
                result += tex2D(_BlitTexture, float2(texPos12.x, texPos3.y)) * w12.x * w3.y;
                result += tex2D(_BlitTexture, float2(texPos3.x, texPos3.y)) * w3.x * w3.y;

                return result;
            }

            ENDHLSL
        }

        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment fp

            sampler2D _SmokeTex, _SmokeMaskTex;
            Texture2D _DepthTex;
            int _DebugView;
            float _Sharpness;

            float4 fp(Varyings input) : SV_Target {
                float4 col = tex2D(_BlitTexture, input.texcoord.xy);
                
                float2 smokeUV = float2(input.texcoord.x, 1.0 - input.texcoord.y);
    
                float4 smokeAlbedo = tex2D(_SmokeTex, smokeUV);
                float smokeMask = saturate(tex2D(_SmokeMaskTex, smokeUV).r);
    
                //Apply Sharpness
                float neighbor = _Sharpness * -1;
                float center = _Sharpness * 4 + 1;

                float4 n = tex2D(_SmokeTex, smokeUV + _BlitTextureSize.xy * float2(0, 1));
                float4 e = tex2D(_SmokeTex, smokeUV + _BlitTextureSize.xy * float2(1, 0));
                float4 s = tex2D(_SmokeTex, smokeUV + _BlitTextureSize.xy * float2(0, -1));
                float4 w = tex2D(_SmokeTex, smokeUV + _BlitTextureSize.xy * float2(-1, 0));

                float4 sharpenedSmoke = n * neighbor + e * neighbor + smokeAlbedo * center + s * neighbor + w * neighbor;

                switch (_DebugView) {
                    case 0:
                        //return col + smokeAlbedo;
                        return lerp(col, saturate(sharpenedSmoke), 1 - smokeMask);
                    case 1:
                        return saturate(sharpenedSmoke);
                    case 2:
                        return 1 - smokeMask;
                    case 3:
                        return _DepthTex.Sample(sampler_PointClamp, input.texcoord.xy);
                }

                return float4(1, 0, 1, 0);
            }

            ENDHLSL
        }
    }
}