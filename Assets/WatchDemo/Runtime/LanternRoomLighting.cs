using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    // Five dim landmarks; portable lanterns provide the usable working light.
    // Fixed illumination is baked. This component creates no realtime lights.
    [DefaultExecutionOrder(1800)]
    public sealed class LanternRoomLighting:MonoBehaviour
    {
        readonly List<Renderer> bulbs=new();readonly List<(ReflectionProbe probe,float intensity)> probes=new();float oldReflection;Material bulbMaterial;MaterialPropertyBlock block;WatchDemoController watch;
        public static readonly Vector3[] RoomLights={new(-3.30f,2.20f,-8.20f),new(3.71f,2.20f,-.85f),new(.75f,5.50f,2.80f),new(2.60f,8.80f,.92f),new(-.50f,12.10f,1.82f)};
        // Undersides measured against the authored ceilings/floor panels and dome mesh.
        public static readonly float[] MountHeights={2.70f,3.30f,6.25f,9.55f,14.34f};
        public static int Room(Vector3 p)=>p.z< -4.4f?0:Mathf.Clamp(Mathf.FloorToInt(p.y/3.3f)+1,1,4);
        public static void Create(WatchDemoController w){var c=w.gameObject.AddComponent<LanternRoomLighting>();c.watch=w;}
        void Awake()=>block=new MaterialPropertyBlock();
        public static readonly string[] Messages={"HELP ME","HELP ME","666","YOULL DIE HERE","DEATH"};
        static readonly MorseFilament[] Patterns={new(Messages[0],.48f),new(Messages[1],.40f),new(Messages[2],.43f),new(Messages[3],.46f),new(Messages[4],.38f)};
        public static bool Powered(int room,bool mains)=>room<=1||mains;
        public static float Filament(float time,int room)=>Patterns[Mathf.Clamp(room,0,4)].Sample(time+room*2.17f,room==0?.60f:room==1?.28f:.06f);
        void Start(){
            oldReflection=RenderSettings.reflectionIntensity;RenderSettings.reflectionIntensity=.025f;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientIntensity=0;RenderSettings.ambientLight=new Color(.0002f,.00025f,.0003f);
            foreach(var p in FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){probes.Add((p,p.intensity));p.intensity=.025f;}
            foreach(var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None)){
                if(r.name=="Frosted service lamp")r.transform.parent.gameObject.SetActive(false);
                if(r.name.StartsWith("SM_Lamp_")||r.name.StartsWith("SM_Fixture_")||r.name.StartsWith("SM_EmergencyLamp_")||r.name.StartsWith("SM_EmergencyFixture_"))r.enabled=false;
            }
            gameObject.AddComponent<RoomFilamentLightmaps>();
            var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();
            bulbMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Frosted warm room bulbs"};
            for(int i=0;i<RoomLights.Length;i++){
                var pos=RoomLights[i];var root=M2StationParts.Root("Single hanging room bulb "+i,transform,Vector3.zero);root.position=pos;
                float ceiling=MountHeights[i];
                // The drops sit beside the room's wall-mounted station, outside the walking line.
                parts.Part("Cloth pendant cord",root,Vector3.up*((ceiling-pos.y)*.5f),new Vector3(.008f,ceiling-pos.y,.008f),parts.Dark);
                parts.Part("Ceiling fixing",root,Vector3.up*(ceiling-pos.y),new Vector3(.075f,.02f,.075f),parts.Metal);
                parts.Part("Ceramic bulb socket",root,Vector3.up*.060f,new Vector3(.042f,.05f,.042f),parts.Metal);
                var bulb=parts.Part("Weak filament bulb",root,Vector3.zero,new Vector3(.058f,.085f,.058f),bulbMaterial,PrimitiveType.Sphere).GetComponent<Renderer>();bulbs.Add(bulb);
            }
        }
        void LateUpdate(){
            if(!watch||watch.State==null||block==null)return;
            bool powered=watch.State.HasPower(LighthouseKeepers.Core.PowerCircuit.Lights);
            Shader.SetGlobalVector("_KeeperRoomPulse",new Vector4(Filament(Time.time,1),powered?Filament(Time.time,2):0,powered?Filament(Time.time,3):0,powered?Filament(Time.time,4):0));
            Shader.SetGlobalFloat("_KeeperHousePulse",Filament(Time.time,0));
            for(int i=0;i<bulbs.Count;i++){var r=bulbs[i];if(!r)continue;float glow=Powered(i,powered)?Filament(Time.time,i)*(i==0?1.6f:.65f):0;
                block.Clear();block.SetColor("_BaseColor",glow>0?new Color(1,.65f,.3f)*glow:new Color(.025f,.02f,.015f));block.SetColor("_EmissionColor",new Color(1,.52f,.19f)*glow);r.SetPropertyBlock(block);
            }
        }
        void OnDestroy(){if(bulbMaterial)Destroy(bulbMaterial);RenderSettings.reflectionIntensity=oldReflection;foreach(var p in probes)if(p.probe)p.probe.intensity=p.intensity;}
    }
}
