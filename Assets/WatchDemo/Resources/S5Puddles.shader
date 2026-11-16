Shader "Lighthouse/S5 outdoor rain puddles" {
 Properties {_Storm("Storm",Range(0,1))=0}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-2" "RenderType"="Transparent"} Pass {
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float _Storm;
 CBUFFER_END
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float2 uv:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.uv=a.uv;return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float2 cell=frac(i.world.xz*3.1)-.5;float seed=frac(sin(dot(floor(i.world.xz*3.1),float2(12.9898,78.233)))*43758.5453);
 cell-=float2(seed-.5,frac(seed*17)-.5)*.42;float life=frac(_Time.y*1.1+seed),r=length(cell);float ring=exp(-abs(r-life*.55)*75)*(1-life)*step(.35,seed);float2 ripple=normalize(cell+.001)*ring*.025;
 half3 n=normalize(half3(ripple.x+.008*sin(i.world.z*29+_Time.y),1,ripple.y));half3 view=GetWorldSpaceNormalizeViewDir(i.world);half fresnel=pow(1-saturate(dot(n,view)),4);
 half3 reflected=GlossyEnvironmentReflection(reflect(-view,n),.22,1);half3 base=half3(.002,.004,.005);
 half3 colour=lerp(base,min(reflected,half3(.10,.14,.15)),.04+fresnel*.24)+ring*half3(.006,.008,.009);
 half edge=saturate((1-length(i.uv))*8);return half4(colour,edge*lerp(.72,.86,_Storm));}
 ENDHLSL
 }}
}
