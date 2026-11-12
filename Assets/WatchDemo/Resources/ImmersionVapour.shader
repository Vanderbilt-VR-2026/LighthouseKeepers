Shader "LighthouseKeepers/GPU service vapour"
{
 Properties { _Tint("Tint",Color)=(.7,.75,.75,.15) _Strength("Strength",Range(0,1))=1 _Rise("Rise",Float)=.5 _Size("Size",Float)=.14 _Rate("Rate",Float)=.6 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "DisableBatching"="True" }
  Pass
  {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _Tint;float _Strength,_Rise,_Size,_Rate;
   CBUFFER_END
   struct A {float4 vertex:POSITION;float4 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float age:TEXCOORD1;float4 screen:TEXCOORD2;float eye:TEXCOORD3;UNITY_VERTEX_OUTPUT_STEREO};
   V vert(A input)
   {
    V o;UNITY_SETUP_INSTANCE_ID(input);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    float age=frac(_Time.y*_Rate+input.uv.z);float seed=input.uv.z*113;
    float3 centre=TransformObjectToWorld(float3(0,0,0))+float3(sin(seed+age*4)*age*.16,age*_Rise,cos(seed+age*3)*age*.12);
    float size=_Size*(.25+age);
    float3 p=centre+(UNITY_MATRIX_I_V._m00_m10_m20*input.uv.x+UNITY_MATRIX_I_V._m01_m11_m21*input.uv.y)*size;
    o.positionCS=TransformWorldToHClip(p);o.screen=ComputeScreenPos(o.positionCS);o.eye=-TransformWorldToView(p).z;o.uv=input.uv.xy;o.age=age;return o;
   }
   half4 frag(V i):SV_Target
   {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float radial=saturate(1-dot(i.uv,i.uv));
    // No camera-depth copy on Quest; radial edges and near-eye fade keep these tiny wisps soft.
    float soft=saturate((i.eye-.08)*5);
    half alpha=radial*radial*sin(i.age*3.14159)*_Tint.a*_Strength*soft;
    alpha*=.75+.25*sin(i.uv.x*9+i.uv.y*7+i.age*8);
    return half4(_Tint.rgb,alpha);
   }
   ENDHLSL
  }
 }
}
