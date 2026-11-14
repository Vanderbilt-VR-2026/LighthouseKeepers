Shader "Lighthouse/S5 Drowned oilskin" {
 Properties {_Visibility("Visibility",Range(0,1))=1 _Breath("Breath",Float)=0}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout"} Pass {
 ZWrite On Cull Off AlphaToMask On
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float _Visibility,_Breath;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 colour:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;half4 colour:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);float3 p=a.positionOS.xyz;p.z+=sin(_Breath*1.7)*.008*saturate(p.y-1);o.world=TransformObjectToWorld(p);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);o.colour=a.colour;return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);clip(_Visibility-.005);half3 n=normalize(i.normal),view=GetWorldSpaceNormalizeViewDir(i.world);half rim=pow(1-saturate(abs(dot(n,view))),4);half3 light=clamp(SampleSH(n),half3(.15,.19,.21),half3(.45,.45,.45));half3 colour=i.colour.rgb*(light*2)+rim*half3(.003,.006,.007);return half4(colour,_Visibility);}
 ENDHLSL
 }}
}
