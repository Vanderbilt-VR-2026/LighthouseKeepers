// Unlit distant headland silhouette + faint shore lights in one draw (vertex colour a=1 marks a light).
// Haze blends rock into the sky horizon colour; lightning briefly reveals the cliff faces.
Shader "LighthouseKeepers/Distant Coast" {
 Properties {
  _RockColor("Rock",Color)=(0.012,0.014,0.017,1)
  _HazeColor("Haze (match sky horizon)",Color)=(0.06,0.07,0.082,1)
  _HazeNear("Haze at 180 m",Range(0,1))=0.35
  _HazeFar("Haze at 300 m",Range(0,1))=0.62
  _LightGain("Shore light brightness",Range(0,3))=1
 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10"} Cull Off Pass {
 Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _RockColor,_HazeColor;float _HazeNear,_HazeFar,_LightGain;
 CBUFFER_END
 float _LK_Lightning;float4 _LK_LightningDir;
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 color:COLOR;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;half4 color:COLOR;half3 lit:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i)
 {
  V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  float3 world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(world);
  float r=length(world.xz);half haze=(half)lerp(_HazeNear,_HazeFar,saturate((r-180)/120));
  half isLight=i.color.a;
  half3 n=(half3)normalize(TransformObjectToWorldNormal(i.normalOS));
  half lightning=(half)saturate(_LK_Lightning);
  half3 rock=_RockColor.rgb*i.color.rgb+half3(0.1,0.115,0.15)*lightning*(0.1+0.25*abs(dot(n,(half3)_LK_LightningDir.xyz)));
  // Slow, independent twinkle through rain; never a hard blink.
  half twinkle=0.8+0.2*sin(_Time.y*(1.3+i.uv.x*0.2)+i.uv.x*7);
  half3 lamp=i.color.rgb*_LightGain*twinkle*0.9;
  o.lit=lerp(lerp(rock,_HazeColor.rgb,haze),lerp(lamp,_HazeColor.rgb,haze*0.4),isLight);
  o.color=i.color;return o;
 }
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);return half4(i.lit,1);}
 ENDHLSL
 } }
 Fallback Off
}
