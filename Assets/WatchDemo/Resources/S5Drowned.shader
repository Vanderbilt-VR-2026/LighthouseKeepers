Shader "Lighthouse/S5 Drowned oilskin" {
 Properties {_Visibility("Visibility",Range(0,1))=1 _Breath("Breath",Float)=0 _Arrival("Appearance age",Float)=0}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout"} Pass {
 ZWrite On Cull Off AlphaToMask On
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float _Visibility,_Breath,_Arrival;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 colour:COLOR;float2 detail:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;half4 colour:COLOR;half kind:TEXCOORD2;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);float3 p=a.positionOS.xyz;
 float lift=smoothstep(.05,.28,_Arrival)*.075;
 float recoil=saturate(_Arrival/.14)*exp(-max(0,_Arrival-.14)*4);
 p.z+=recoil*(.11+(abs(p.x)>.30&&p.y<1.45?.16:0));
 p.z+=sin(_Breath*1.3)*.006*saturate(p.y-.8);p.y+=a.detail.y*lift;p.x+=a.detail.y*sin(_Breath*.45)*.006;
 o.world=TransformObjectToWorld(p);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);o.colour=a.colour;o.kind=a.detail.x;return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);clip(_Visibility-.005);
 half3 n=normalize(i.normal),view=GetWorldSpaceNormalizeViewDir(i.world);half rim=pow(1-saturate(abs(dot(n,view))),4);
 half3 light=clamp(SampleSH(n),half3(.22,.26,.28),half3(.42,.42,.42));
 half wet=.35+.65*pow(saturate(sin(i.world.x*73+i.world.z*61+sin(i.world.y*5))),8);
 half3 reflection=GlossyEnvironmentReflection(reflect(-view,n),.28,1);
 half3 colour=i.colour.rgb*(light*2);
 if(i.kind<1.5)colour+=min(reflection,half3(.4,.4,.4))*.075*wet+rim*half3(.012,.025,.028);
 return half4(colour,_Visibility);}
 ENDHLSL
 }}
}
