using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    // Object-mounted fittings and practical lights only. Does not edit avatars or water renderers.
    public sealed class ImmersionWorld : MonoBehaviour
    {
        WatchDemoController watch;M2StationParts parts;Transform bed;
        Light[] bunk=new Light[2];readonly List<Light> stair=new();readonly List<Light> station=new();
        readonly List<AudioSource> faultAudio=new();
        readonly List<(Light light,Renderer lens)> lenses=new();MaterialPropertyBlock lensBlock;
        Font chalk;int bedMask;HarborRadio narrator;
        public HarborRadio Narrator=>narrator;
        public static ImmersionWorld Create(WatchDemoController watch,Transform head,Transform bed,TextMesh board,Transform wrench,WatchPipeRepair pipe,WatchGenerator generator,WatchRadioConsole radio,WatchBeacon beacon)
        {
            var p=watch.gameObject.AddComponent<ImmersionWorld>();p.watch=watch;p.bed=bed;p.lensBlock=new MaterialPropertyBlock();p.parts=watch.gameObject.AddComponent<M2StationParts>();p.parts.Initialize();
            p.chalk=Resources.Load<Font>("Fonts/Kalam-Regular");
            if(watch.State.Immersion.physicalGuidance)p.Build(head,board,pipe,generator,radio,beacon);
            var cabinet=p.HouseRadio(board);
            foreach(var l in p.GetComponentsInChildren<Light>()){var face=l.transform.parent.Find("Frosted service lamp");if(face)p.lenses.Add((l,face.GetComponent<Renderer>()));}
            if(watch.State.Immersion.radioNarration)p.narrator=HarborRadio.Create(watch,head,wrench,pipe,cabinet);
            if(watch.State.Immersion.faultMist)FaultVapour.Create(watch,pipe,generator);
            if(watch.State.Immersion.atmosphere)p.gameObject.AddComponent<ImmersionAtmosphere>().Initialize(watch,head);
            return p;
        }
        void Build(Transform head,TextMesh board,WatchPipeRepair pipe,WatchGenerator generator,WatchRadioConsole radio,WatchBeacon beacon)
        {
            var face=GameObject.Find("Wall-mounted checklist white face");
            if(face){var b=new MaterialPropertyBlock();b.SetColor("_BaseColor",new Color(.025f,.045f,.035f));b.SetColor("_EmissionColor",new Color(.006f,.009f,.007f));face.GetComponent<Renderer>().SetPropertyBlock(b);}
            if(chalk){board.font=chalk;board.GetComponent<Renderer>().sharedMaterial=chalk.material;board.fontStyle=FontStyle.Normal;}
            board.color=new Color(.87f,.86f,.74f);board.characterSize*=.90f;
            ImmersionPipePresentation.Create(pipe,watch.State);
            var bedMount=M2StationParts.Root("Bunk physical fittings",transform,Vector3.zero);bedMount.SetPositionAndRotation(bed.position,bed.rotation);
            bed=bedMount;
            for(int i=0;i<2;i++)
            {
                bunk[i]=Fixture(parts,bed,new Vector3(i==0?-.63f:.63f,1.05f,.66f),i==0?"Host bunk lamp":"Guest bunk lamp");
                bunk[i].transform.parent.localRotation=Quaternion.Euler(0,180,0);
                var label=parts.Text("Bunk lamp keeper "+i,i==0?"KEEPER 1":"KEEPER 2",bunk[i].transform.parent,new Vector3(0,-.075f,.04f),.0025f);
                parts.Part("Bunk lamp name plate",label.transform,new Vector3(0,0,.007f),new Vector3(.18f,.045f,.012f),parts.Dark);
            }
            var bedPlate=M2StationParts.Root("Bunk rest plate",bed,new Vector3(0,.36f,-1.035f));bedPlate.localRotation=Quaternion.Euler(0,180,0);
            parts.Part("Bunk brass plate",bedPlate,Vector3.zero,new Vector3(.64f,.17f,.02f),parts.Brass);
            parts.Text("Bunk instructions","JOBS DONE? REST TILL DAWN.\nGRIP TO LIE DOWN",bedPlate,new Vector3(0,0,.012f),.003f).color=new Color(.12f,.08f,.03f);
            for(int i=0;i<4;i++)
            {
                float a=320*Mathf.Deg2Rad,r=i<2?3.48f:2.18f;
                var t=M2StationParts.Root("Stair fault lamp F"+(i+1),transform,new Vector3(Mathf.Sin(a)*r,i*3.3f+1.1f,Mathf.Cos(a)*r));
                t.localRotation=Quaternion.LookRotation(new Vector3(-t.localPosition.x,0,-t.localPosition.z));
                parts.Part("Stair lamp mounting post",t,new Vector3(0,-.55f,-.04f),new Vector3(.025f,1.1f,.025f),parts.Metal);
                stair.Add(Fixture(parts,t,Vector3.zero,"Shielded stair lamp"));
            }
            station.Add(Fixture(parts,generator.transform,new Vector3(0,1.9f,.55f),"Generator fault service lamp"));
            station.Add(Fixture(parts,radio.transform,new Vector3(0,2.55f,.20f),"Radio service lamp"));
            station.Add(Fixture(parts,beacon.transform,new Vector3(.65f,.85f,0),"Beacon motor service lamp"));
            faultAudio.Add(Sound(generator.transform,"Generator fault cough","S4Audio/cough",false,.16f));
            faultAudio.Add(Sound(radio.transform,"Unsent broadcast static","S4Audio/hiss",true,.035f));
            faultAudio.Add(Sound(beacon.transform,"Misaligned beacon motor whine","ImmersionAudio/motor",true,.055f));
            // The accepted radio/breaker labels already have physical cabinet faces.
            // The lens hint did not, so replace it with an engraved cleaning tag on the frame.
            var lens=beacon.transform.Find("Lens wipe label");if(lens)lens.gameObject.SetActive(false);
            var tag=M2StationParts.Root("Engraved lens cleaning tag",beacon.transform,new Vector3(.38f,.68f,.40f));
            parts.Part("Lens tag brass",tag,Vector3.zero,new Vector3(.28f,.10f,.015f),parts.Brass);
            parts.Text("Lens tag engraving","WIPE GLASS\nWITH CLOTH",tag,new Vector3(0,0,.009f),.0023f).color=new Color(.1f,.07f,.03f);
            var controls=FindAnyObjectByType<WatchBeaconStation>();
            if(controls)
            {
                var t=controls.transform;
                parts.Part("Beacon instruction plate",t,new Vector3(0,1.53f,.179f),new Vector3(.73f,.10f,.016f),parts.Metal);
                var ring=M2StationParts.Root("Engraved wheel alignment legend",t,new Vector3(-.14f,.91f,.215f));
                parts.Part("Wheel brass legend plate",ring,Vector3.zero,new Vector3(.39f,.09f,.015f),parts.Brass);
                parts.Text("Wheel alignment engraving","ALIGN MARKS ▲",ring,new Vector3(0,0,.01f),.0027f).color=new Color(.1f,.07f,.03f);
                var old=t.Find("Wheel label");if(old)old.gameObject.SetActive(false);
                parts.Part("Rag instruction plate",t,new Vector3(.25f,.64f,.207f),new Vector3(.29f,.13f,.025f),parts.Dark);
            }
        }
        Transform HouseRadio(TextMesh board)
        {
            // The board has its back to the wall; its text's -Z points into the House.
            var t=M2StationParts.Root("House harbor radio",transform,Vector3.zero);
            t.SetPositionAndRotation(board.transform.position+board.transform.right*1.95f+board.transform.up*-.35f+board.transform.forward*.065f,board.transform.rotation*Quaternion.Euler(0,180,0));
            parts.Part("Harbor radio walnut cabinet",t,Vector3.zero,new Vector3(.46f,.27f,.14f),parts.Dark);
            for(int i=0;i<9;i++)parts.Part("Radio speaker grille",t,new Vector3(-.11f+i*.021f,0,.075f),new Vector3(.006f,.17f,.006f),parts.Metal);
            parts.Part("Radio frequency glass",t,new Vector3(.135f,.055f,.075f),new Vector3(.14f,.045f,.01f),parts.Brass);
            for(int i=0;i<2;i++)parts.Part("Radio knob",t,new Vector3(.10f+i*.07f,-.045f,.09f),Vector3.one*.035f,parts.Metal,PrimitiveType.Sphere);
            parts.Text("Harbor radio maker plate","HARBOR CONTROL",t,new Vector3(0,.105f,.076f),.0024f);
            Fixture(parts,t,new Vector3(0,.20f,0),"House briefing radio lamp");return t;
        }
        public static Light Fixture(M2StationParts parts,Transform parent,Vector3 pos,string name)
        {
            var t=M2StationParts.Root(name,parent,pos);
            parts.Part("Service lamp back",t,Vector3.zero,new Vector3(.11f,.16f,.035f),parts.Dark);
            parts.Part("Frosted service lamp",t,new Vector3(0,0,.026f),new Vector3(.055f,.10f,.025f),parts.Brass);
            for(int i=-1;i<=1;i+=2)parts.Part("Lamp guard",t,new Vector3(i*.04f,0,.045f),new Vector3(.009f,.14f,.009f),parts.Metal);
            var l=M2StationParts.Root("Warm practical light",t,new Vector3(0,0,.09f)).gameObject.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.65f,.31f);l.range=2.4f;l.intensity=.35f;l.shadows=LightShadows.None;return l;
        }
        static AudioSource Sound(Transform parent,string name,string clip,bool loop,float volume)
        {var t=M2StationParts.Root(name,parent,Vector3.zero);var s=t.gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=1;s.dopplerLevel=0;s.minDistance=.5f;s.maxDistance=8;s.volume=volume;s.loop=loop;s.clip=Resources.Load<AudioClip>(clip);return s;}
        float nextCough;
        public void SetBeds(int value)=>bedMask=value;
        void Update()
        {
            if(!watch.State.Immersion.physicalGuidance)return;
            var s=watch.State;float pulse=.28f+.10f*Mathf.Sin(Time.time*2.4f);
            for(int i=0;i<2;i++)bunk[i].intensity=(bedMask&(1<<i))!=0?.65f:s.Complete&&!s.Failed?.35f:0;
            for(int i=0;i<stair.Count;i++)
            {bool fault=i==0?!s.Repaired:i==1?s.Power==PowerState.Off:i==3?!s.BeaconShining||!s.IsDone(WatchState.RadioStation):false;stair[i].intensity=fault?pulse:.18f;}
            for(int i=0;i<station.Count;i++)
            {
                bool fault=i==0?s.Power==PowerState.Off:i==1?!s.IsDone(WatchState.RadioStation):!s.BeaconShining;
                station[i].intensity=fault?pulse:.25f;
                var a=faultAudio[i];if(!a.clip)continue;
                if(i==0){if(fault&&!s.Failed&&Time.time>=nextCough){a.Play();nextCough=Time.time+9;}continue;}
                if(fault&&!s.Failed){if(!a.isPlaying)a.Play();}else if(a.isPlaying)a.Stop();
            }
            foreach(var fitting in lenses)
            {lensBlock.Clear();lensBlock.SetColor("_BaseColor",new Color(.7f,.42f,.15f));lensBlock.SetColor("_EmissionColor",fitting.light.color*fitting.light.intensity*2);fitting.lens.SetPropertyBlock(lensBlock);}
        }
    }
}
