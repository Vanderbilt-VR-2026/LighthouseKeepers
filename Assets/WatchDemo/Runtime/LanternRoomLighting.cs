using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Five dim landmarks; portable lanterns provide the usable working light.
    // Fixed illumination is baked. This component creates no realtime lights.
    [DefaultExecutionOrder(1800)]
    public sealed class LanternRoomLighting:MonoBehaviour
    {
        readonly List<Renderer> bulbs=new();MaterialPropertyBlock block;WatchDemoController watch;
        public static readonly Vector3[] RoomLights={new(-3.30f,1.79f,-7.02f),new(3.71f,1.38f,-.85f),new(.75f,5.20f,2.80f),new(2.60f,7.85f,.92f),new(-.50f,11.65f,1.82f)};
        public static bool Keep(string name)=>name=="House briefing radio lamp"||name=="Pipe service bulb 0"||name=="Generator fault service lamp"||name=="Host bunk lamp"||name=="Radio service lamp";
        public static int Room(Vector3 p)=>p.z< -4.4f?0:Mathf.Clamp(Mathf.FloorToInt(p.y/3.3f)+1,1,4);
        public static void Create(WatchDemoController w){var c=w.gameObject.AddComponent<LanternRoomLighting>();c.watch=w;}
        void Awake()=>block=new MaterialPropertyBlock();
        void Start(){
            foreach(var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None)){
                if(r.name=="Frosted service lamp"){
                    if(Keep(r.transform.parent.name))bulbs.Add(r);else r.transform.parent.gameObject.SetActive(false);
                }
                if(r.name.StartsWith("SM_Lamp_")||r.name.StartsWith("SM_Fixture_")||r.name.StartsWith("SM_EmergencyLamp_")||r.name.StartsWith("SM_EmergencyFixture_"))r.enabled=false;
            }
        }
        void LateUpdate(){
            if(!watch||watch.State==null||block==null)return;
            // Slow irregular dimming, no rapid flashes. The failing lamp stays a landmark.
            for(int i=0;i<bulbs.Count;i++){var r=bulbs[i];if(!r)continue;float phase=Time.time*(.8f+i*.07f)+i*2;
                float glow=.13f+.045f*Mathf.Sin(phase)*Mathf.Sin(phase*.63f);
                if(!watch.State.HasPower(LighthouseKeepers.Core.PowerCircuit.Lights))glow*=.35f;
                block.SetColor("_BaseColor",new Color(.12f,.075f,.028f));block.SetColor("_EmissionColor",new Color(1,.52f,.19f)*glow);r.SetPropertyBlock(block);
            }
        }
    }
}
