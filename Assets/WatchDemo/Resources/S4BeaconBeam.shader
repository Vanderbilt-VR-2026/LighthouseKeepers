Shader "Lighthouse/S4 Beacon Beam"
{
 Properties { _BaseColor("Amber",Color)=(1,.55,.16,.12) }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass
  {
   Blend SrcAlpha One
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct Varyings {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;half4 color:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
   Varyings vert(Attributes input){Varyings output;UNITY_SETUP_INSTANCE_ID(input);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);output.positionCS=TransformObjectToHClip(input.positionOS.xyz);output.world=TransformObjectToWorld(input.positionOS.xyz);output.normal=TransformObjectToWorldNormal(input.normalOS);output.color=input.color;return output;}
   half4 frag(Varyings input):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);half facing=abs(dot(normalize(input.normal),normalize(_WorldSpaceCameraPos-input.world)));half edge=.25h+.75h*pow(1-facing,2);half nearFade=saturate((distance(input.world,_WorldSpaceCameraPos)-.25)*2);return input.color*_BaseColor*half4(1,1,1,edge*nearFade);}
   ENDHLSL
  }
 }
}
