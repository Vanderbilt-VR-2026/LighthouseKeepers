Shader "LighthouseKeepers/Tower conduit"
{
 Properties { [MainTexture] _BaseMap("Galvanised metal and stencil atlas",2D)="white"{} }
 SubShader {
 Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass {
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma multi_compile_instancing
 #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _CircuitOn,_CircuitAge,_Feed;
 CBUFFER_END
 struct Input {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;float3 route:TEXCOORD1;float4 colour:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct Vary {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float2 uv:TEXCOORD1;float3 route:TEXCOORD2;float4 colour:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
 Vary Vert(Input i){Vary o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.normalWS=TransformObjectToWorldNormal(i.normalOS);o.uv=i.uv;o.route=i.route;o.colour=i.colour;return o;}
 half4 Frag(Vary i):SV_Target{
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 int circuit=(int)round(i.route.x);float on=circuit==4?_Feed.x:_CircuitOn[min(circuit,3)];float age=circuit==4?_Feed.y:_CircuitAge[min(circuit,3)];
 float arrival=i.route.y*max(.1,_Feed.z);float routed=saturate(i.route.z);float pulse=(1-smoothstep(0,.22,abs(age-arrival)))*on*routed;
 float3 light=max(.08,SampleSH(normalize(i.normalWS)));light=lerp(dot(light,float3(.2126,.7152,.0722)).xxx,light,.45);Light sun=GetMainLight();light+=sun.color*saturate(dot(normalize(i.normalWS),sun.direction))*.3;
 light=clamp(light,.08,.85)*lerp(.18,1,saturate(_CircuitAge.w-min(.75,i.route.y*1.5))*_CircuitOn.w);
 // Repeat the material tile every 35 mm; stencil UVs remain fixed in the atlas.
 float2 atlasUV=i.uv;float2 dx=ddx(i.uv),dy=ddy(i.uv);
 if(i.route.z>.5){float2 tile=frac(i.uv);atlasUV=float2(i.route.z>1.5?.27:.01,.005)+tile*float2(.22,.09);dx*=float2(.22,.09);dy*=float2(.22,.09);}
 float3 metal=SAMPLE_TEXTURE2D_GRAD(_BaseMap,sampler_BaseMap,atlasUV,dx,dy).rgb*i.colour.rgb;
 return half4(metal*light+float3(.5,.24,.055)*(pulse*.22),1);
 }
 ENDHLSL
 }
 }
}
