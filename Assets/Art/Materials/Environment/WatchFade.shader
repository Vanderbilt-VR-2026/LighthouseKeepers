Shader "LighthouseKeepers/Watch Fade" {
 Properties { _Color("Color",Color)=(0,0,0,0) }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Overlay+10" "RenderType"="Transparent"} Pass {
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest Always Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 float4 _Color;
 struct A {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(i.positionOS.xyz);return o;}
 half4 frag(V i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);return _Color;}
 ENDHLSL
 }} }
