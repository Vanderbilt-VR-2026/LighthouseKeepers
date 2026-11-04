using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class ImmersionPipePresentation : MonoBehaviour
    {
        WatchPipeRepair pipe;WatchState state;M2StationParts parts;Transform needle;
        readonly Transform[] bolts=new Transform[3];readonly Vector3[] rests=new Vector3[3];
        readonly int[] chalkState={-1,-1,-1};
        readonly TextMesh[] chalk=new TextMesh[3];readonly Light[] bulbs=new Light[3];
        public static void Create(WatchPipeRepair pipe,WatchState state)
        {var p=pipe.gameObject.AddComponent<ImmersionPipePresentation>();p.pipe=pipe;p.state=state;p.Build();}
        void Build()
        {
            parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();
            for(int i=0;i<3;i++)
            {
                var t=pipe.bolts[i];bolts[i]=t.Find("Ratchet bolt");rests[i]=bolts[i].localPosition;
                var old=t.Find("Coupling marker");if(old)old.gameObject.SetActive(false);
                // Chalk sits on the coupling's flanged metal face, not beside it in space.
                chalk[i]=parts.Text("Chalk coupling "+(char)('A'+i),((char)('A'+i))+" ↻",t,new Vector3(0,.075f,.016f),.0035f);
                chalk[i].color=new Color(.85f,.84f,.72f);
                bulbs[i]=ImmersionWorld.Fixture(parts,t,new Vector3(0,.27f,-.055f),"Pipe service bulb "+i);
                if(state.Immersion.leverPipe)
                {
                    var fixture=bulbs[i].transform.parent;
                    fixture.SetPositionAndRotation(t.position+Vector3.up*.36f+Vector3.right*.13f,Quaternion.LookRotation(Vector3.left));
                }
                parts.Part("Bulb support",t,new Vector3(0,.18f,-.075f),new Vector3(.025f,.18f,.025f),parts.Metal);
            }
            var gauge=M2StationParts.Root("Mechanical pipe pressure gauge",pipe.bolts[1],new Vector3(.23f,.24f,-.06f));
            var rim=parts.Part("Gauge brass surround",gauge,Vector3.zero,new Vector3(.20f,.025f,.20f),parts.Brass,PrimitiveType.Cylinder);rim.transform.localRotation=Quaternion.Euler(90,0,0);
            parts.Part("Gauge dial face",gauge,new Vector3(0,0,.026f),new Vector3(.16f,.16f,.012f),parts.Dark);
            parts.Text("Pressure dial scale","LOW        HIGH",gauge,new Vector3(0,.055f,.035f),.0017f);
            needle=M2StationParts.Root("Pressure gauge needle",gauge,new Vector3(0,-.025f,.039f));
            parts.Part("Pressure pointer",needle,new Vector3(0,.045f,0),new Vector3(.007f,.09f,.006f),parts.Brass);
            parts.Part("Gauge pipe stem",gauge,new Vector3(0,-.16f,-.03f),new Vector3(.03f,.15f,.03f),parts.Metal);
        }
        void Update()
        {
            var p=state.Pipe;
            for(int i=0;i<3;i++)
            {
                bool done=false;for(int j=0;j<p.BoltsDone;j++)if(p.Order(j)==i)done=true;
                bool active=p.ActiveBolt==i;
                float turns=done?p.Profile.quarterTurns[i]:active?p.TurnsDone+p.StrokeDegrees/p.Profile.quarterTurnDegrees:0;
                bolts[i].localRotation=Quaternion.Euler(0,0,(state.Immersion.leverPipe?0:45)+turns*90*(p.Direction==0?1:p.Direction));
                bolts[i].localPosition=rests[i]-Vector3.forward*(.022f*turns/p.Profile.quarterTurns[i]);
                int mark=done?2:active?1:0;if(chalkState[i]!=mark){chalkState[i]=mark;chalk[i].text=done?((char)('A'+i))+" ✓":active?((char)('A'+i))+" ↻":((char)('A'+i)).ToString();}
                bulbs[i].intensity=done?.18f:active?.45f+.12f*Mathf.Sin(Time.time*2.4f):.12f;
            }
            needle.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(60,-60,p.InflowFraction));
            pipe.leakAudio.transform.position=pipe.ActivePosition;
        }
    }
}
