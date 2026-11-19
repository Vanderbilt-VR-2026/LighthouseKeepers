Shader "Lighthouse/S5 Drowned oilskin" {
 Properties {_Burn("Flare burn",Range(0,1))=0 _Visibility("Visibility",Range(0,1))=1 _Breath("Breath",Float)=0 _Arrival("Appearance age",Float)=0 _Style("Encounter",Float)=0 _Height("Height",Float)=1.84 _Walk("Gait",Float)=0 _HeadTurn("Head turn radians",Float)=0}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout"} Pass {
 ZWrite On Cull Off AlphaToMask On
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float _Burn,_Visibility,_Breath,_Arrival,_Style,_Height,_Walk,_HeadTurn;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 colour:COLOR;float2 detail:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;half4 colour:COLOR;half kind:TEXCOORD2;float3 local:TEXCOORD3;half3 vertexLight:TEXCOORD4;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);float3 p=a.positionOS.xyz;
 float head=a.detail.y;
 float gait=sin(_Breath*(_Walk>.8?11:4))*_Walk;
 float leg=(1-smoothstep(_Height*.23,_Height*.46,p.y))*sign(p.x);
 float arm=smoothstep(_Height*.1,_Height*.25,abs(p.x))*(1-head);
 p.z+=gait*(leg*.13-arm*.08);p.y+=abs(gait)*.015;
 float yaw=_HeadTurn*head;float2 face=float2(p.x,p.z-.03);
 p.x=face.x*cos(yaw)+face.y*sin(yaw);p.z=-face.x*sin(yaw)+face.y*cos(yaw)+.03;
 float shoulder=smoothstep(_Height*.50,_Height*.75,p.y)*(1-head);
 // Half-second burst: neck whips once, hands seize, then the body settles or vanishes.
 float burst=exp(-max(0,_Arrival-.08)*9)*smoothstep(0,.05,_Arrival);
 float twitch=sin(_Arrival*71)*burst;
 float bend=sin(_Arrival*8)*burst;
 p.x+=head*(twitch*.026+bend*.018)+shoulder*twitch*.009;
 p.z+=head*bend*.025;
 float lift=smoothstep(.05,.28,_Arrival)*.018;
 float recoil=saturate(_Arrival/.14)*exp(-max(0,_Arrival-.14)*4);
 p.z+=recoil*(.11+(abs(p.x)>.30&&p.y<1.45?.16:0));
 p.z+=sin(_Breath*1.3)*.006*saturate(p.y-.8);p.y+=a.detail.y*lift;p.x+=a.detail.y*sin(_Breath*.45)*.006;
 // The longer watcher turns its head; ceiling limbs brace against the underside.
 if(_Style>.5&&_Style<1.5){float yaw=sin(_Arrival*.75)*.23*head;float2 centred=float2(p.x,p.z-.08);p.x=centred.x*cos(yaw)+centred.y*sin(yaw);p.z=-centred.x*sin(yaw)+centred.y*cos(yaw)+.08;}
 if(_Style>1.5){float contact=max(1-smoothstep(.20,.45,p.y),smoothstep(.26,.40,abs(p.x))*(1-head));p.z=lerp(p.z,-.32,contact);}
 o.local=a.positionOS.xyz;o.world=TransformObjectToWorld(p);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);o.vertexLight=VertexLighting(o.world,normalize(o.normal));o.colour=a.colour;o.kind=a.detail.x;return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);clip(_Visibility-.005);
 half3 n=normalize(i.normal),view=GetWorldSpaceNormalizeViewDir(i.world);half rim=pow(1-saturate(abs(dot(n,view))),4);
 half3 light=clamp(SampleSH(n),half3(.15,.18,.19),half3(.36,.36,.36));
 half wet=.35+.65*pow(saturate(sin(i.local.x*173+i.local.z*161+sin(i.local.y*15))),8);
 half3 reflection=GlossyEnvironmentReflection(reflect(-view,n),.28,1);
 Light key=GetMainLight();
 // Directional form remains legible in a blackout without a new realtime light.
 half diffuse=.22+.52*saturate(dot(n,key.direction))+.26*saturate(dot(n,normalize(half3(-.45,.8,.25))));
 half grain=sin(i.local.x*137+sin(i.local.z*91))*sin(i.local.y*211+i.local.z*79);
 half salt=.97+.03*grain;
 half3 colour=i.colour.rgb*(light*1.55)*diffuse*salt;
 // The handheld flashlight is an additional light. Support the active Quest
 // per-pixel path and the optional vertex fallback without adding any lights.
 #if defined(_ADDITIONAL_LIGHTS_VERTEX)
 colour+=i.colour.rgb*i.vertexLight;
 #elif defined(_ADDITIONAL_LIGHTS)
 uint count=GetAdditionalLightsCount();
 LIGHT_LOOP_BEGIN(count)
     Light lamp=GetAdditionalLight(lightIndex,i.world);
     colour+=i.colour.rgb*LightingLambert(lamp.color*lamp.distanceAttenuation,lamp.direction,n);
 LIGHT_LOOP_END
 #endif
 if(i.kind>.5&&i.kind<1.5){half vein=pow(saturate(sin(i.local.y*48+sin(i.local.x*28)*2+i.local.z*33)),18);colour*=1-vein*.10;}

 if(i.kind<1.5)colour+=min(reflection,half3(.4,.4,.4))*.075*wet+rim*half3(.012,.025,.028);
 colour+=_Burn*half3(1.8,.045,.008)*(0.6+0.4*wet);
 return half4(colour,_Visibility);}
 ENDHLSL
 }}
}
