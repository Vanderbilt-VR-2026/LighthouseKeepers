Shader "LighthouseKeepers/BeaconGlass" {
 Properties {_BaseColor("Glass tint",Color)=(.65,.78,.8,1) _EmissionColor("Lamp glow",Color)=(0,0,0,1) _Window("Rain streaks",Float)=0}
 SubShader {Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"} Pass {Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)half4 _BaseColor,_EmissionColor;float _Window;CBUFFER_END
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;half fog:TEXCOORD2;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.world=TransformObjectToWorld(i.positionOS.xyz);o.normal=TransformObjectToWorldNormal(i.normalOS);o.positionCS=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);half3 v=normalize(GetWorldSpaceViewDir(i.world));half3 n=normalize(i.normal);half f=pow(1-abs(dot(n,v)),5);half3 reflection=GlossyEnvironmentReflection(reflect(-v,n),.08,1);half3 colour=lerp(_BaseColor.rgb*.4,reflection,.65)+_EmissionColor.rgb*.12;float streak=pow(saturate(sin((i.world.x+i.world.z)*57)*.5+.5),18)*pow(saturate(sin(i.world.y*19+_Time.y*2)*.5+.5),12)*_Window;colour+=streak*.2;return half4(MixFog(colour,i.fog),.12+f*.48+streak*.12);}
 ENDHLSL }
 }
}
