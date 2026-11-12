// Quest-friendly storm sea: single opaque pass, no grab/depth/opaque texture.
// Four summed Gerstner waves in the vertex shader (analytic normals), crest/shore foam,
// fresnel toward a sky tint, beacon-beam and lightning glints, distance fade into the sky horizon.
// Waves are only enabled once StormSeascape has swapped in the radial grid (_LK_OceanWaves=1);
// on the original 10x10 primitive plane the shader stays flat.
Shader "LighthouseKeepers/Storm Ocean Waves" {
 Properties {
  [Header(Colour)]
  _DeepColor("Deep water",Color)=(0.006,0.02,0.027,1)
  _CrestColor("Crest scatter",Color)=(0.035,0.1,0.11,1)
  _SkyTint("Reflected upper sky",Color)=(0.018,0.023,0.03,1)
  _HorizonColor("Horizon (match sky)",Color)=(0.06,0.07,0.082,1)
  _ReliefDirection("Brightest horizon XZ (match sky glow)",Vector)=(0.97,0,0.24,0)
  _Relief("Horizon relief light",Range(0,2))=0.7
  _FoamColor("Foam",Color)=(0.2,0.235,0.255,1)
  [Header(Waves)]
  _WaveAmplitude("Base amplitude (m)",Range(0,1.5))=0.32
  _StormGain("Extra amplitude at worst storm (x)",Range(0,2))=1.1
  _WaveLength("Longest wavelength (m)",Range(6,60))=22
  _WaveSpeed("Speed multiplier",Range(0,2))=0.85
  _Steepness("Gerstner steepness",Range(0,1))=0.55
  _WindAzimuth("Wind azimuth (deg)",Range(0,360))=250
  [Header(Foam)]
  _FoamAmount("Crest foam amount",Range(0,1))=0.45
  _ShoreFoam("Shore foam",Range(0,1))=0.55
  [Header(Island and horizon)]
  _IslandCentre("Island centre XZ",Vector)=(-0.2,-3.2,0,0)
  _IslandRadii("Island radii XZ",Vector)=(7.8,13.7,0,0)
  _ShoreFalloff("Shore falloff (radii)",Range(0.2,4))=1.6
  _ShoreAmplitude("Amplitude at shore",Range(0,1))=0.06
  _CalmStart("Calm start (m)",Float)=80
  _CalmEnd("Calm end (m)",Float)=240
  _CalmAmplitude("Amplitude far out",Range(0,1))=0.4
  _FadeStart("Horizon fade start (m)",Float)=110
  _FadeEnd("Horizon fade end (m)",Float)=315
  _RingRatio("Mesh radial spacing / radius",Float)=0.0602
 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"} Pass {
 Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _DeepColor,_CrestColor,_SkyTint,_HorizonColor,_FoamColor;float4 _ReliefDirection;float _Relief;
 float _WaveAmplitude,_StormGain,_WaveLength,_WaveSpeed,_Steepness,_WindAzimuth,_FoamAmount,_ShoreFoam;
 float4 _IslandCentre,_IslandRadii;
 float _ShoreFalloff,_ShoreAmplitude,_CalmStart,_CalmEnd,_CalmAmplitude,_FadeStart,_FadeEnd,_RingRatio;
 CBUFFER_END
 // Globals written by StormSeascape (all default to 0 when absent).
 float _LK_OceanWaves,_LK_StormIntensity,_LK_Lightning;
 float4 _LK_LightningDir,_LK_BeamPos,_LK_BeamDir; // BeamPos.w=intensity 0..1, BeamDir.w=cos(half angle)
 struct A {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;half4 data:TEXCOORD2;UNITY_VERTEX_OUTPUT_STEREO};
 // data: x crest (0..1), y shore band, z detail fade, w horizon fade

 void Gerstner(float2 dir,float wavelength,float amplitude,float q,float2 xz,float t,inout float3 offset,inout float3 n,inout float crest)
 {
  float k=6.2831853/wavelength;float w=sqrt(9.81*k)*_WaveSpeed;float f=k*dot(dir,xz)-w*t;
  float s,c;sincos(f,s,c);float wa=k*amplitude;
  // Per-wave Q bounded so horizontal drift never exceeds the (attenuated) amplitude and the sum cannot loop.
  q=min(q/max(wa,1e-4),1);
  offset+=float3(q*amplitude*dir.x*c,amplitude*s,q*amplitude*dir.y*c);
  n+=float3(-dir.x*wa*c,-q*wa*s,-dir.y*wa*c);crest+=amplitude*s;
 }
 V vert(A i)
 {
  V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  float3 world=TransformObjectToWorld(i.positionOS.xyz);float2 xz=world.xz;
  float storm=saturate(_LK_StormIntensity);
  float e=length((xz-_IslandCentre.xy)/max(_IslandRadii.xy,0.1));
  float shore=lerp(_ShoreAmplitude,1,smoothstep(1,1+_ShoreFalloff,e));
  float r=length(xz);
  float calm=lerp(1,_CalmAmplitude,smoothstep(_CalmStart,_CalmEnd,r));
  float amp=_WaveAmplitude*(1+_StormGain*storm)*shore*calm*saturate(_LK_OceanWaves);
  float t=_Time.y*(1+0.25*storm);
  float wind=radians(_WindAzimuth);float2 d0=float2(sin(wind),cos(wind));
  float2 d1=float2(sin(wind+0.7),cos(wind+0.7)),d2=float2(sin(wind-1.05),cos(wind-1.05)),d3=float2(sin(wind+2.2),cos(wind+2.2));
  float spacing=max(r*_RingRatio,0.05);
  // Each wave fades out where the radial grid has fewer than ~4 samples per wavelength.
  float L0=_WaveLength,L1=_WaveLength*0.67,L2=_WaveLength*0.43,L3=_WaveLength*0.29;
  float4 lod=saturate((float4(L0,L1,L2,L3)/spacing-3)*0.5);
  float4 a=amp*float4(1,0.7,0.42,0.25)*lod;
  float q=_Steepness*(0.8+0.2*storm)/4;
  float3 offset=0,n=float3(0,1,0);float crest=0;
  Gerstner(d0,L0,a.x,q,xz,t,offset,n,crest);
  Gerstner(d1,L1,a.y,q,xz,t*1.07,offset,n,crest);
  Gerstner(d2,L2,a.z,q,xz,t*0.93,offset,n,crest);
  Gerstner(d3,L3,a.w,q,xz,t*1.11,offset,n,crest);
  world+=offset;
  o.world=world;o.positionCS=TransformWorldToHClip(world);o.normal=(half3)normalize(n);
  float total=max(dot(a,1),1e-3);
  o.data.x=(half)saturate(crest/total*0.5+0.5);
  o.data.y=(half)(1-smoothstep(1.0,1.45,e))*(half)step(0.85,e);
  float dist=length(world-_WorldSpaceCameraPos);
  o.data.z=(half)(1-smoothstep(25,90,dist));
  o.data.w=(half)smoothstep(_FadeStart,_FadeEnd,r);
  return o;
 }
 float hash12(float2 p){float3 p3=frac(float3(p.xyx)*0.1031);p3+=dot(p3,p3.yzx+33.33);return frac((p3.x+p3.y)*p3.z);}
 float vnoise(float2 p){float2 k=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash12(k),hash12(k+float2(1,0)),f.x),lerp(hash12(k+float2(0,1)),hash12(k+1),f.x),f.y);}
 half4 frag(V i):SV_Target
 {
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
  float storm=saturate(_LK_StormIntensity);float t=_Time.y;
  // Two cheap ripples add near-field detail; faded with distance to avoid shimmer.
  float2 p=i.world.xz;
  float r1=sin(dot(p,float2(0.83,0.41))*1.9+t*2.1),r2=sin(dot(p,float2(-0.37,0.91))*2.6+t*2.7);
  half3 n=normalize(i.normal+half3(r1*0.035+r2*0.015,0,r2*0.03-r1*0.01)*i.data.z);
  n=normalize(lerp(n,half3(0,1,0),i.data.w*0.7));
  float3 v=normalize(_WorldSpaceCameraPos-i.world);
  half ndv=saturate(dot(n,v));half fres=0.02+0.98*pow(1-ndv,5);
  float lightning=saturate(_LK_Lightning);
  // Linear-space constant (project uses linear colour): ~sRGB 0.35 at full flash.
  half3 flash=half3(0.1,0.115,0.15)*lightning;
  float3 refl=reflect(-v,n);
  // Night storm sky is brighter at the horizon: faces toward the viewer reflect the dark deck, back faces the horizon.
  half3 sky=lerp(_HorizonColor.rgb,_SkyTint.rgb,saturate(refl.y*1.6))+flash*(0.08+0.22*pow(saturate(dot(refl,_LK_LightningDir.xyz)),3));
  half3 body=lerp(_DeepColor.rgb,_CrestColor.rgb,i.data.x*i.data.x);
  half3 col=lerp(body,sky,fres);
  // Slopes facing the glowing horizon catch a little of it: reads wave relief without a light.
  col+=_HorizonColor.rgb*_Relief*saturate(dot(n.xz,normalize(_ReliefDirection.xz+1e-5))*2.5+0.1)*i.data.z;
  // Lightning washes the wave faces that turn toward the strike.
  col+=flash*(0.015+0.05*saturate(dot(n,_LK_LightningDir.xyz)));
  // Beacon beam: lit sheen where the cone sweeps the water and a sharp glint on faces toward it.
  float beamOn=_LK_BeamPos.w;
  if(beamOn>0.001)
  {
   float3 toBeam=_LK_BeamPos.xyz-i.world;float dist=length(toBeam);float3 l=toBeam/dist;
   float cone=smoothstep(_LK_BeamDir.w,lerp(_LK_BeamDir.w,1,0.6),dot(-l,_LK_BeamDir.xyz));
   float reach=beamOn*cone/(1+dist*0.025);
   float3 h=normalize(l+v);half spec=pow(saturate(dot(n,h)),96);
   col+=half3(1,0.6,0.25)*reach*(0.08*saturate(dot(n,l))+spec*3);
  }
  // Faint moonless sky sheen gives distant crests some sparkle without a light source.
  float3 hs=normalize(float3(0.3,0.9,0.3)+v);col+=pow(saturate(dot(n,hs)),180)*0.025*(1+3*lightning);
  // Foam: crest whitecaps broken up by one octave of value noise, plus a surf band at the island.
  float noise=vnoise(p*0.45+float2(t*0.12,-t*0.07));
  // Only the highest few percent of crests break, patchy rather than in parallel stripes.
  float thr=1-_FoamAmount*0.32*(0.55+0.45*storm);
  float foam=smoothstep(thr,thr+0.07,i.data.x)*smoothstep(0.35,0.8,noise);
  float surf=i.data.y*_ShoreFoam*smoothstep(0.35,0.75,vnoise(p*0.9+t*0.3)*0.7+0.5*sin(t*0.9+dot(p,float2(0.3,0.2))));
  foam=saturate(foam*(1-i.data.w)+surf);
  half3 foamCol=_FoamColor.rgb*(0.45+0.55*noise)+flash*0.3;
  col=lerp(col,foamCol,foam*0.8);
  col=lerp(col,_HorizonColor.rgb+flash*0.12,i.data.w);
  return half4(col,1);
 }
 ENDHLSL
 } }
 Fallback Off
}
