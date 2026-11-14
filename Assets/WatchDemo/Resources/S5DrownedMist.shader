Shader "Lighthouse/S5 Drowned localized mist" {
 Properties {_Visibility("Visibility",Range(0,1))=1 _Clock("Host watch time",Float)=0}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"} Pass {
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float _Visibility,_Clock;
 CBUFFER_END
 struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;float2 seed:TEXCOORD1;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float2 info:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
 float seed=a.seed.x,kind=a.seed.y,phase=frac(_Clock*.18+seed);float3 p;float2 size;
 if(kind<.5){p=float3(sin(seed*30)*.38+sin(_Clock*.3+seed*6)*.06,.08+phase*.16,cos(seed*30)*.28);size=float2(.3,.12);}
 else if(kind<1.5){phase=frac(_Clock*(.8+seed*.5)+seed);p=float3(sin(seed*40)*.32,1.55-phase*1.5,cos(seed*40)*.16);size=float2(.0025,.027);}
 else{p=float3(.055,1.72+phase*.10,.22+phase*.25);size=float2(.12,.07)*(1+phase);}
 float3 world=TransformObjectToWorld(p);float3 right=float3(UNITY_MATRIX_I_V[0][0],UNITY_MATRIX_I_V[1][0],UNITY_MATRIX_I_V[2][0]);
 world+=right*a.uv.x*size.x+float3(0,1,0)*a.uv.y*size.y;
 o.positionCS=TransformWorldToHClip(world);o.uv=a.uv;o.info=float2(kind,phase);return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 half round=saturate(1-dot(i.uv,i.uv));half noise=.65+.35*sin(i.uv.x*6+sin(i.uv.y*8+_Clock*.6));
 half fade=sin(i.info.y*3.14159);half alpha=round*round*noise*fade*_Visibility*(i.info.x>.5&&i.info.x<1.5?.26:.12);
 return half4(.065,.09,.095,alpha);}
 ENDHLSL
 }}
}
