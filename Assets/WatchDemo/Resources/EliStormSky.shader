// Stormy night skybox for Quest. Per pixel: three value-noise lookups (two-octave main deck + one
// octave of faster low scud), no textures. Lightning (_LK_Lightning, _LK_LightningDir) lights the
// cloud deck from inside; storm intensity (_LK_StormIntensity, 0..1 across the three watches)
// thickens and speeds the clouds. All globals default to 0, so the sky is calm-dark without the driver.
Shader "LighthouseKeepers/Eli Storm Sky (opt in)" {
 Properties {
  [Header(Sky)]
  _ZenithColor("Zenith",Color)=(0.004,0.006,0.009,1)
  _HorizonColor("Horizon (match ocean)",Color)=(0.06,0.07,0.082,1)
  _GlowColor("Horizon glow",Color)=(0.1,0.088,0.074,1)
  _GlowDirection("Glow direction (towards coast)",Vector)=(0.97,0,0.24,0)
  _GlowStrength("Glow strength",Range(0,2))=1.2
  [Header(Clouds)]
  _CloudDark("Cloud underside",Color)=(0.007,0.009,0.012,1)
  _CloudLit("Cloud lit edge",Color)=(0.13,0.14,0.155,1)
  _CloudDensity("Cloud density",Range(0,1))=0.62
  _CloudScale("Cloud scale",Range(0.1,4))=1.1
  _CloudSpeed("Cloud speed",Range(0,0.2))=0.012
  _ScudSpeed("Low scud speed",Range(0,0.5))=0.05
  _WindAzimuth("Wind azimuth (deg)",Range(0,360))=250
  [Header(Lightning)]
  _FlashColor("Flash colour",Color)=(0.5,0.56,0.7,1)
  _FlashStrength("Flash strength",Range(0,2))=0.22
 }
 SubShader {Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"} Cull Off ZWrite Off Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _ZenithColor,_HorizonColor,_GlowColor,_CloudDark,_CloudLit,_FlashColor;
 float4 _GlowDirection;
 float _GlowStrength,_CloudDensity,_CloudScale,_CloudSpeed,_ScudSpeed,_WindAzimuth,_FlashStrength;
 CBUFFER_END
 float _LK_StormIntensity,_LK_Lightning;float4 _LK_LightningDir;
 struct A {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;float3 direction:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.direction=i.positionOS.xyz;return o;}
 // Sine-free hash: stable precision on mobile GPUs.
 float hash12(float2 p){float3 p3=frac(float3(p.xyx)*0.1031);p3+=dot(p3,p3.yzx+33.33);return frac((p3.x+p3.y)*p3.z);}
 float vnoise(float2 p){float2 k=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash12(k),hash12(k+float2(1,0)),f.x),lerp(hash12(k+float2(0,1)),hash12(k+1),f.x),f.y);}
 half4 frag(V i):SV_Target
 {
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
  float3 d=normalize(i.direction);float h=d.y;
  float storm=saturate(_LK_StormIntensity);float lightning=saturate(_LK_Lightning)*_FlashStrength;
  // Base gradient: near-black zenith, a lifted horizon band and a faint warm glow over the coast.
  half3 sky=lerp(_HorizonColor.rgb,_ZenithColor.rgb,sqrt(saturate(h*1.6)));
  float2 flat=normalize(d.xz+1e-5);
  float glow=exp(-abs(h)*9)*_GlowStrength*(0.35+0.65*pow(saturate(dot(flat,normalize(_GlowDirection.xz+1e-5))),3));
  sky+=_GlowColor.rgb*glow;
  // Clouds projected on a flat deck; fade toward the horizon where the projection stretches.
  float up=saturate(h);
  float2 uv=d.xz/(up+0.16)*_CloudScale;
  float wind=radians(_WindAzimuth);float2 w=float2(sin(wind),cos(wind));
  float t=_Time.y*(1+storm*1.5);
  float n=vnoise(uv*1.3+w*t*_CloudSpeed*10)*0.65+vnoise(uv*3.1-w.yx*t*_CloudSpeed*16)*0.35;
  float scud=vnoise(uv*2.2*float2(1,0.6)+w*t*_ScudSpeed*10);
  float cover=_CloudDensity+storm*0.18;
  float deck=smoothstep(1-cover-0.15,1-cover+0.35,n);
  float low=smoothstep(0.55,0.85,scud)*(0.45+0.35*storm);
  float density=saturate(deck+low*(1-deck*0.6));
  float horizonFade=smoothstep(-0.01,0.12,h);
  // Thin edges catch the horizon glow; thick cores stay dark.
  half3 cloud=lerp(_CloudLit.rgb,_CloudDark.rgb,saturate(n*1.2-0.1));
  cloud+=_GlowColor.rgb*glow*0.6;
  half3 col=lerp(sky,cloud,density*horizonFade*0.92);
  // Lightning: light scatters inside the deck around the strike; thick cloud glows brightest.
  float toward=saturate(dot(d,_LK_LightningDir.xyz));
  float flashShape=(0.12+pow(toward,4)*1.4)*(0.35+0.65*density)*(0.5+0.5*up+0.3*horizonFade);
  col+=_FlashColor.rgb*lightning*flashShape*(0.6+0.4*n);
  // Below the horizon the ocean covers the view; keep the colour continuous with its far fade.
  col=lerp(_HorizonColor.rgb+_FlashColor.rgb*lightning*0.1,col,smoothstep(-0.03,0.0,h));
  // Sub-LSB dither: the Quest swapchain is 8-bit and these gradients are very dark.
  // Dither in (approximate) sRGB so the step is one display LSB even in these near-black tones.
  col=max(col,0);float3 g=sqrt(col)+(hash12(i.positionCS.xy)-0.5)*(1.0/255);col=g*g;
  return half4(col,1);
 }
 ENDHLSL
 } }
 Fallback Off
}
