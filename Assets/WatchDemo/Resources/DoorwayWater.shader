Shader "LighthouseKeepers/Doorway Water"
{
 Properties { _BaseColor("Water",Color)=(.14,.28,.30,1) _Flow("Head difference",Float)=0 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10" }
  Pass
  {
   Cull Off ZWrite On
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;float _Flow;
   CBUFFER_END
   struct Attributes{float4 positionOS:POSITION;};
   struct Varyings{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;};
   Varyings vert(Attributes a){Varyings o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);return o;}
   half4 frag(Varyings i):SV_Target
   {
    float speed=clamp(_Flow*5,-.5,.5);
    float wave=sin((i.world.z+_Time.y*speed)*30+sin(i.world.x*11))*sin(i.world.x*5+i.world.z*9);
    return half4(_BaseColor.rgb*(1+wave*.07),1);
   }
   ENDHLSL
  }
 }
}
