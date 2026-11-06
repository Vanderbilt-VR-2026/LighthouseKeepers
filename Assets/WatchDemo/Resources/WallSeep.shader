Shader "LighthouseKeepers/WallSeep" {Properties{_Wetness("Wetness",Range(0,1))=.2}
 SubShader {Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent"} Pass {Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)half _Wetness;CBUFFER_END
 struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o;}half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);half edge=sin(i.uv.x*3.14159)*sin(i.uv.y*3.14159);half streak=pow(saturate(sin(i.uv.x*58)*.5+.5),8);half drops=pow(saturate(sin(i.uv.y*17+_Time.y*3+i.uv.x*53)*.5+.5),15)*streak;return half4(half3(.025,.04,.035)+drops*.18,edge*_Wetness*(.16+streak*.22));}
 ENDHLSL}}}
