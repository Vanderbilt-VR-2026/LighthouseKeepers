Shader "LighthouseKeepers/Sprint3/Soot stain"
{
    Properties { _BaseColor("Soot",Color)=(.025,.020,.015,.45) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct Input { float4 positionOS:POSITION; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Output { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
            Output Vert(Input i) { Output o; UNITY_SETUP_INSTANCE_ID(i); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.uv=i.positionOS.xy+.5; return o; }
            half4 Frag(Output i):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 p=(i.uv-.5)*2;
                float noise=frac(sin(dot(floor(i.uv*32),float2(12.9898,78.233)))*43758.5453);
                float edge=saturate(1-dot(p,p));
                return half4(_BaseColor.rgb,_BaseColor.a*edge*edge*(.65+.35*noise));
            }
            ENDHLSL
        }
    }
}
