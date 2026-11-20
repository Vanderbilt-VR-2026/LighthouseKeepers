Shader "Lighthouse/Steamer GPU effects"
{
 Properties { _ShipClock("Host clock",Float)=0 _Storm("Storm",Range(0,1))=0 _Flare("Distress flare",Float)=0 }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" } Pass {
 Blend One OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float _ShipClock,_Storm,_Flare;
 CBUFFER_END
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;float2 phase:TEXCOORD1;half4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;float3 normal:TEXCOORD1;float3 world:TEXCOORD2;half age:TEXCOORD3;half fog:TEXCOORD4;half beamT:TEXCOORD5;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);float3 p=a.positionOS.xyz;o.age=0;o.beamT=saturate(p.z/38);o.color=a.color;o.uv=a.uv;o.normal=TransformObjectToWorldNormal(a.normalOS);
 if(a.color.a>.1){bool smoke=a.color.a<.8;float age=frac(_ShipClock/(smoke?5.5:1.6)+a.phase.x);o.age=age;
 if(smoke){p+=float3((a.phase.y-.5)*age*5,age*7,-age*9);}
 else{p+=float3(sign(p.x)*age*3.5,3.5*age*(1-age),-age*4);}
 float3 world=TransformObjectToWorld(p);float size=smoke?lerp(.65,3.2,age):lerp(.12,.7,age);
 // View-facing quads; all simulation is bounded vertex work, no compute/CPU particle system.
 world+=UNITY_MATRIX_I_V._m00_m10_m20*(a.uv.x-.5)*size+UNITY_MATRIX_I_V._m01_m11_m21*(a.uv.y-.5)*size;
 o.world=world;o.positionCS=TransformWorldToHClip(world);}
 else{o.world=TransformObjectToWorld(p);o.positionCS=TransformWorldToHClip(o.world);}
 o.fog=ComputeFogFactor(o.positionCS.z);return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 if(_Flare>.5){half r=length(i.uv-.5)*2;half a=pow(saturate(1-r),3);return half4(half3(1,.12,.015)*a,a*.2);}
 if(i.color.a<.1){half facing=abs(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world)));half alpha=(.15+.85*pow(1-facing,2))*.018*(1-i.beamT);return half4(i.color.rgb*alpha,0);}
 half radius=length(i.uv-.5)*2;half edge=saturate(1-radius*radius);half life=sin(i.age*3.14159);bool smoke=i.color.a<.8;half a=edge*edge*life*(smoke?.24:(_Storm>.55?.45:0));half3 c=MixFog(i.color.rgb,i.fog);return half4(c*a,a);}
 ENDHLSL
 }}
}
