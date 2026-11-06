Shader "LighthouseKeepers/PracticalHaze" {Properties{_BaseColor("Tint",Color)=(.65,.55,.3,.06) _MainTex("Gradient",2D)="white"{} }
 SubShader{Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent"} Pass{Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)half4 _BaseColor;CBUFFER_END
 struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.uv=i.uv;return o;}half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);half edge=pow(saturate(sin(i.uv.x*3.14159)*sin(i.uv.y*3.14159)),2);half nearFade=saturate((distance(i.world,_WorldSpaceCameraPos)-.3)*1.5);return half4(_BaseColor.rgb,_BaseColor.a*edge*nearFade);}
 ENDHLSL}}}
