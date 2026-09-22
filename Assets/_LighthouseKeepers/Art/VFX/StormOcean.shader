Shader "LighthouseKeepers/Storm Ocean" {
 Properties { _BaseColor("Slate sea",Color)=(0.025,0.09,0.12,1) }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;half fog:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
 half4 frag(V i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float t=_Time.y;float wave=sin(i.world.x*.43+i.world.z*.61+sin(i.world.z*.13)*1.7+t*.6)*.6+sin(i.world.x*.7-i.world.z*.32-t*.7)*.25;float crest=pow(saturate(wave),12);half3 color=_BaseColor.rgb+wave*.018+crest*half3(.04,.06,.07);float fog=saturate(length(_WorldSpaceCameraPos.xz-i.world.xz)/140);return half4(lerp(color,unity_FogColor.rgb,fog*fog),1);}
 ENDHLSL
 } }
}
