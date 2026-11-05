Shader "Lighthouse/ObjectiveOverlay" {
 Properties { [PerRendererData] _MainTex("Texture",2D)="white"{} _AlphaOnly("Alpha-only font",Float)=0 }
 SubShader { Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" } Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct output {float4 vertex:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 sampler2D _MainTex;float _AlphaOnly;
 output vert(appdata v){output o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
 fixed4 frag(output i):SV_Target{fixed4 t=tex2D(_MainTex,i.uv);t.rgb=lerp(t.rgb,fixed3(1,1,1),_AlphaOnly);return t*i.color;}
 ENDCG }
 }
}
