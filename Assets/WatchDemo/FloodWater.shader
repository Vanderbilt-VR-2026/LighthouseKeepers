Shader "LighthouseKeepers/Interior Flood Water"
{
    Properties
    {
        [MainColor] _BaseColor("Water tint", Color) = (.16,.20,.17,1)
        _RippleMap("Ripple height texture (repeat)", 2D) = "gray" {}
        _RippleTiling("Ripple repeats per metre", Range(.1,3)) = .8
        _RippleHeight("Ripple shading height (metres)", Range(0,.03)) = .008
        _FlowSpeed("Surface drift (metres per second)", Range(0,.1)) = .018
        _Smoothness("Wet surface smoothness", Range(0,1)) = .88
        _Opacity("Looking-down opacity", Range(.1,1)) = .62
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "FloodWater"
            Tags { "LightMode"="UniversalForwardOnly" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            // Keep wet highlights visible through the partially transparent diffuse tint.
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _ALPHAPREMULTIPLY_ON 1
            #define _SPECULAR_SETUP 1
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_RippleMap);
            SAMPLER(sampler_RippleMap);
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _RippleTiling, _RippleHeight, _FlowSpeed;
                half _Smoothness, _Opacity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half4 fogAndVertexLight : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.fogAndVertexLight.x = ComputeFogFactor(position.positionCS.z);
                output.fogAndVertexLight.yzw = VertexLighting(position.positionWS, half3(0,1,0));
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // The flood disc has no UVs. World coordinates keep detail at metre scale
                // while the existing flood component moves the surface vertically.
                float2 drift = _Time.y * _FlowSpeed * float2(1,.6);
                float2 uv = (input.positionWS.xz + drift) * _RippleTiling;
                float2 crossedUV = float2(.5*uv.x - .8660254*uv.y, .8660254*uv.x + .5*uv.y);
                half first = SAMPLE_TEXTURE2D(_RippleMap, sampler_RippleMap, uv).r;
                half second = SAMPLE_TEXTURE2D(_RippleMap, sampler_RippleMap, crossedUV*1.73 - drift*_RippleTiling*2).r;
                half height = first*.6h + second*.4h;

                // Derive a surface normal from two scrolling height samples. This works
                // on the existing coarse mesh without displacing the gameplay waterline.
                float3 rippledPosition = input.positionWS;
                rippledPosition.y += (height-.5h)*_RippleHeight;
                float3 normal = cross(ddy(rippledPosition), ddx(rippledPosition));
                normal *= normal.y < 0 ? -1 : 1;
                half3 normalWS = normalize(normal + float3(0,1e-8,0));
                half3 viewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
                if (viewDirection.y < 0) normalWS = -normalWS;
                half fresnel = Pow4(1-saturate(dot(normalWS, viewDirection)));

                InputData lighting = (InputData)0;
                lighting.positionWS = input.positionWS;
                lighting.normalWS = normalWS;
                lighting.viewDirectionWS = viewDirection;
                lighting.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                lighting.fogCoord = input.fogAndVertexLight.x;
                lighting.vertexLighting = input.fogAndVertexLight.yzw;
                lighting.bakedGI = SampleSH(normalWS);
                lighting.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                lighting.shadowMask = half4(1,1,1,1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = _BaseColor.rgb * lerp(.85h,1.15h,height);
                surface.specular = half3(.02h,.02h,.02h);
                surface.smoothness = _Smoothness;
                surface.normalTS = half3(0,0,1);
                surface.occlusion = 1;
                // More transparent looking down; more reflective at a shallow angle.
                surface.alpha = lerp(_Opacity,1,fresnel);
                half4 color = UniversalFragmentPBR(lighting, surface);
                color.rgb = MixFog(color.rgb, lighting.fogCoord);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
