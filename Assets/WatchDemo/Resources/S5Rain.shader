Shader "Lighthouse/S5 heavy exterior rain" {
 Properties {_Storm("Storm",Range(0,1))=0 _Quiet("Quiet",Range(0,1))=1}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+2" "RenderType"="Transparent"} Pass {
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _RainHead;float _Storm,_Quiet;
 CBUFFER_END
 struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;float2 seed:TEXCOORD1;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half alpha:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 bool Roof(float3 p){float radius=p.y<9.9?4.4:3.1;return (p.y<14.8&&dot(p.xz,p.xz)<radius*radius)||(abs(p.x)<3.95&&p.z>-10.3&&p.z<-4.4&&p.y<4.4)||(abs(p.x)<.8&&p.z>-5.6&&p.z<-3.8&&p.y<2.9)||(p.y>9.4&&p.y<9.9&&dot(p.xz,p.xz)<18.1);}
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
 float speed=lerp(6.5,10,_Storm)*(1+a.seed.x*.3);float3 p=a.positionOS.xyz+float3(_Time.y*(.65+_Storm),-_Time.y*speed,_Time.y*.2);
 float3 head=GetCameraPositionWS();p=head+(frac((p-head+float3(10,7,10))/float3(20,14,20))-.5)*float3(20,14,20);
 float d=distance(p,head);o.alpha=(Roof(p)||a.seed.y>lerp(.6,1,_Storm))?0:saturate((d-.55)*2)*saturate((16-d)/3)*lerp(.22,.38,_Storm)*_Quiet;
 float3 right=float3(UNITY_MATRIX_I_V[0][0],UNITY_MATRIX_I_V[1][0],UNITY_MATRIX_I_V[2][0]);
 p+=right*a.uv.x*(.0035+a.seed.x*.003)+float3(-.12,1,0)*a.uv.y*lerp(.12,.26,a.seed.x);
 o.positionCS=TransformWorldToHClip(p);o.uv=a.uv;return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);half edge=(1-abs(i.uv.x))*saturate((1-abs(i.uv.y))*3);return half4(.54,.65,.69,i.alpha*edge);}
 ENDHLSL
 }}
}
