// Quest adaptation of the imported procedural-water approach: two vertex waves,
// opaque forward shading and stereo support. No scene texture/depth reads.
Shader "LighthouseKeepers/Quest Procedural Ocean"
{
 Properties { _BaseColor("Deep slate",Color)=(.035,.075,.09,1) _CrestColor("Storm crest",Color)=(.15,.23,.25,1) _Amplitude("Wave height",Range(0,1))=.35 }
 SubShader {
 Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
 Pass {
 Tags { "LightMode"="UniversalForward" }
 ZWrite On Cull Back
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor, _CrestColor; float _Amplitude;
 CBUFFER_END
 float _LKStormWetness;
 struct Attributes { float4 positionOS:POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
 struct Varyings { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float crest:TEXCOORD1; float fog:TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };
 Varyings vert(Attributes i) {
 Varyings o; UNITY_SETUP_INSTANCE_ID(i); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
 float3 p=TransformObjectToWorld(i.positionOS.xyz);
 float a=dot(p.xz,float2(.17,.11))-_Time.y*.85;
 float b=dot(p.xz,float2(-.09,.23))-_Time.y*1.15;
 float amp=_Amplitude*lerp(.5,1.4,saturate(_LKStormWetness));
 p.y+=amp*(sin(a)+.5*sin(b));
 o.normalWS=normalize(float3(-amp*(.17*cos(a)-.045*cos(b)),1,-amp*(.11*cos(a)+.115*cos(b))));
 o.crest=saturate((sin(a)+.5*sin(b)-.6)*1.3);
 o.positionCS=TransformWorldToHClip(p);o.fog=ComputeFogFactor(o.positionCS.z);return o;
 }
 half4 frag(Varyings i):SV_Target {
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 Light light=GetMainLight();half3 n=normalize(i.normalWS);
 half3 c=lerp(_BaseColor.rgb,_CrestColor.rgb,i.crest*.55);
 c*=.55+max(0,dot(n,light.direction))*.45;
 return half4(MixFog(c,i.fog),1);
 }
 ENDHLSL
 }
 }
}
