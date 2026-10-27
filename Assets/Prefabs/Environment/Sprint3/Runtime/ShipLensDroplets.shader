Shader "LighthouseKeepers/Ship Lens Droplets"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZWrite Off ZTest Always Cull Off Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            float _ShipLensWetness;
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float2 cell = floor(uv * float2(17, 10));
                float seed = frac(sin(dot(cell, float2(127.1,311.7))) * 43758.5453);
                float2 p = frac(uv * float2(17,10)) - .5;
                p.y += frac(_Time.y * (.035 + seed * .025) + seed) - .5;
                p.x += (seed - .5) * .4;
                float d = length(p * float2(1.5, .85));
                float rim = smoothstep(.15,.12,d) * smoothstep(.08,.11,d);
                float core = smoothstep(.12,.02,d) * .1;
                float alpha = (rim * .3 + core) * step(.55,seed) * _ShipLensWetness;
                return half4(.55,.62,.66,alpha);
            }
            ENDHLSL
        }
    }
}
