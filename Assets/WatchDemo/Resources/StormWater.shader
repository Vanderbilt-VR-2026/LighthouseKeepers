Shader "LighthouseKeepers/StormWater" {
 Properties {_BaseColor("Water",Color)=(.035,.12,.14,1) _NormalMap("Ripples",2D)="bump"{} _StormAmplitude("Wave height",Float)=.1 _Ocean("Ocean",Float)=0 _Night("Night",Float)=1 _Flow("Flow",Float)=0}
 SubShader {Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10"} Pass {Name "Forward" Tags{"LightMode"="UniversalForward"} Cull Off ZWrite On
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_NormalMap);SAMPLER(sampler_NormalMap);
 CBUFFER_START(UnityPerMaterial)half4 _BaseColor;float _StormAmplitude,_Ocean,_Night,_Flow;CBUFFER_END
 float4 _KeeperRipple0,_KeeperRipple1,_KeeperRipple2,_KeeperRipple3;
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;float height:TEXCOORD1;float fog:TEXCOORD2;UNITY_VERTEX_OUTPUT_STEREO};
 float3 wave(float2 p,float2 d,float frequency,float speed,float amplitude){d=normalize(d);float phase=dot(p,d)*frequency+_Time.y*speed;return float3(d.x*cos(phase)*amplitude*.35,sin(phase)*amplitude,d.y*cos(phase)*amplitude*.35);}
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);float3 p=TransformObjectToWorld(i.positionOS.xyz);float distanceToView=distance(p.xz,_WorldSpaceCameraPos.xz);float fade=1-smoothstep(65,160,distanceToView);float a=_Ocean*_StormAmplitude*fade;
 float3 displacement=wave(p.xz,float2(.8,.6),.7,1.4,a*.55)+wave(p.xz,float2(-.5,.86),1.3,1.8,a*.25)+wave(p.xz,float2(.2,-.98),2.1,2.4,a*.13)+wave(p.xz,float2(.94,-.34),3.7,2.9,a*.07);float h=displacement.y;p+=displacement;o.height=h;o.positionWS=p;o.positionCS=TransformWorldToHClip(p);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
 float ripple(float2 p,float4 r){float age=_Time.y-r.z;float d=distance(p,r.xy);return sin(d*38-age*9)*exp(-d*4)*saturate(1-age/2)*step(0,age)*r.w;}
 half4 frag(V i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float2 p=i.positionWS.xz;float2 uv=p*.22;half3 a=UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,uv+_Time.y*float2(.017,.009)));half3 b=UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,uv*1.73+_Time.y*float2(-.013,.019)));float rr=ripple(p,_KeeperRipple0)+ripple(p,_KeeperRipple1)+ripple(p,_KeeperRipple2)+ripple(p,_KeeperRipple3);half3 n=normalize(half3((a.x+b.x)*.32+rr,1,(a.y+b.y)*.32+rr));half3 v=normalize(GetWorldSpaceViewDir(i.positionWS));half fresnel=pow(1-saturate(dot(n,v)),4);half3 reflected=GlossyEnvironmentReflection(reflect(-v,n),.28,1);half3 depthTint=_BaseColor.rgb*lerp(1,.55,saturate(i.positionWS.y*.8)*(1-_Ocean));half3 colour=lerp(depthTint,reflected,fresnel*.65+.08);float crest=smoothstep(_StormAmplitude*.30,max(.02,_StormAmplitude*.65),i.height)*_Ocean;float wallFoam=(1-smoothstep(.03,.13,abs(length(p)-3.65)))*(1-_Ocean);float streak=pow(saturate(sin(p.x*3+p.y*.8+_Time.y)*.5+.5),14);colour+=half3(.24,.28,.27)*(crest*streak+wallFoam*.23);colour+=abs(rr)*.07;colour=MixFog(colour,i.fog);return half4(colour,1);}
 ENDHLSL }
 }
}
