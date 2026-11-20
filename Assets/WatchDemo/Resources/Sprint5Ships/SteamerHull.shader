Shader "Lighthouse/Steamer hull and lamps"
{
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" } Pass {
 Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 half _LK_Lightning;
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;half3 normal:TEXCOORD0;half3 ambient:TEXCOORD1;half fog:TEXCOORD2;half4 color:COLOR;float3 world:TEXCOORD3;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.normal=TransformObjectToWorldNormal(a.normalOS);o.ambient=SampleSH(o.normal);o.color=a.color;o.world=TransformObjectToWorld(a.positionOS.xyz);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);Light main=GetMainLight();half3 n=normalize(i.normal);half ndl=saturate(dot(n,main.direction));half3 lighting=max(i.ambient,half3(.10,.13,.16))+main.color*ndl+half3(.12,.16,.20)*saturate(_LK_Lightning)*saturate(n.y*.5+.5);half3 col=i.color.rgb*lighting;col=lerp(col,i.color.rgb*1.25,i.color.a);half3 fogged=MixFog(col,i.fog);fogged=lerp(fogged,max(fogged,i.color.rgb*.25),i.color.a);return half4(fogged,1);}
 ENDHLSL
 }}
}
