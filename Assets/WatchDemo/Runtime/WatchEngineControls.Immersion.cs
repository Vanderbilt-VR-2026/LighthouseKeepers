using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchEngineControls
    {
        TextMesh primerTally,starterTally;
        Transform primerDrum,starterDrum;
        Renderer finalChokeLamp;
        LineRenderer fuelStream;
        AudioSource pourSound;
        bool physicalGuidance;
        int shownPrimer=-1,shownStarter=-1;
        void BuildPhysicalGuidance()
        {
            physicalGuidance=true;
            // A single bolted service fascia connects every fitting to the engine casing.
            parts.Part("Generator service fascia",transform,new Vector3(0,1.23f,.91f),new Vector3(1.55f,.82f,.16f),parts.Dark);
            foreach(var name in new[]{"Generator sequence panel","Starter pull label","Primer label","Choke label","Filler label"})
            {var t=transform.Find(name);if(t)t.gameObject.SetActive(false);}
            parts.Part("Riveted starting instructions",transform,new Vector3(0,1.56f,1.012f),new Vector3(1.55f,.17f,.035f),parts.Brass);
            parts.Text("Engraved starting order",LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchEngineControls.Immersion.3"),transform,new Vector3(0,1.56f,1.034f),.0036f).color=Color.black;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)
                parts.Part("Plate rivet",transform,new Vector3(x*.745f,1.56f+y*.060f,1.040f),Vector3.one*.016f,parts.Metal,PrimitiveType.Sphere);
            // Existing four lamps still show fuel, priming, choke and running; fifth confirms reopening.
            finalChokeLamp=parts.Part("Choke reopened lamp",transform,new Vector3(-.20f,.88f,1.02f),new Vector3(.065f,.035f,.025f),parts.Green).GetComponent<Renderer>();
            parts.Text("Final choke lamp label","5",transform,new Vector3(-.20f,.93f,1.02f),.0032f);
            MountedLabel("Primer plate",LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchEngineControls.Immersion.4"),new Vector3(.20f,.99f,1.04f),new Vector2(.30f,.13f));
            MountedLabel("Choke plate",LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchEngineControls.Immersion.5"),new Vector3(-.20f,.99f,1.04f),new Vector2(.30f,.13f));
            MountedLabel("Starter plate",LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchEngineControls.Immersion.6"),new Vector3(-.62f,.99f,1.04f),new Vector2(.31f,.13f));
            parts.Part("Primer mounting plate",Primer.transform,new Vector3(0,0,-.07f),new Vector3(.16f,.20f,.16f),parts.Metal);
            var collar=parts.Part("Primer retaining collar",Primer.transform,new Vector3(0,0,.008f),new Vector3(.125f,.026f,.125f),parts.Brass,PrimitiveType.Cylinder);
            collar.transform.localRotation=Quaternion.Euler(90,0,0);
            parts.Part("Primer fuel hose",Primer.transform,new Vector3(0,-.16f,-.045f),new Vector3(.022f,.25f,.022f),parts.Dark,PrimitiveType.Cylinder);
            parts.Part("Choke pivot housing",Choke.transform,new Vector3(0,0,-.025f),new Vector3(.13f,.13f,.08f),parts.Metal);
            primerTally=Counter("Primer tally",new Vector3(.20f,1.40f,1.04f),out primerDrum);
            starterTally=Counter("Starter tally",new Vector3(-.62f,1.40f,1.04f),out starterDrum);
            Filler.localPosition=new Vector3(.62f,1.15f,1.10f);
            parts.Part("Filler neck",Filler,new Vector3(0,-.07f,-.015f),new Vector3(.10f,.07f,.10f),parts.Metal,PrimitiveType.Cylinder);
            parts.Part("Filler neck bracket",Filler,new Vector3(0,-.16f,-.09f),new Vector3(.19f,.04f,.25f),parts.Dark);
            // Dark mouth inside a brass rim makes the receiving opening legible from standing height.
            parts.Part("Open filler mouth",Filler,new Vector3(0,.026f,0),new Vector3(.085f,.004f,.085f),parts.Dark,PrimitiveType.Cylinder);
            MountedLabel("Fuel filling plate",LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchEngineControls.Immersion.7"),new Vector3(.62f,.99f,1.16f),new Vector2(.35f,.13f));
            // Tank bottom sits 1 cm above the floor; no raised shelf or aisle support.
            FuelCan.SetHome(transform.TransformPoint(new Vector3(.98f,.20f,1.16f)),transform.rotation);
            for(int i=0;i<4;i++){lamps[i].transform.localPosition=new Vector3(.62f-i*.4133f,.88f,1.02f);var caption=transform.Find(new[]{"FUEL","PRIMED","CHOKE","RUNNING"}[i]);if(caption)caption.gameObject.SetActive(false);}
            fuelStream=new GameObject("Fuel stream into filler").AddComponent<LineRenderer>();fuelStream.transform.SetParent(transform,false);
            fuelStream.sharedMaterial=parts.Brass;fuelStream.positionCount=2;fuelStream.widthMultiplier=.012f;fuelStream.useWorldSpace=true;fuelStream.enabled=false;
            pourSound=Filler.gameObject.AddComponent<AudioSource>();pourSound.playOnAwake=false;pourSound.loop=true;pourSound.spatialBlend=1;pourSound.minDistance=.3f;pourSound.maxDistance=4;pourSound.volume=.12f;pourSound.clip=Resources.Load<AudioClip>("S4Audio/water");
        }
        void MountedLabel(string name,string caption,Vector3 position,Vector2 size)
        {
            parts.Part(name+" backing",transform,position-Vector3.forward*.014f,new Vector3(size.x,size.y,.025f),parts.Dark);
            parts.Text(name,caption,transform,position,.005f);
        }
        TextMesh Counter(string name,Vector3 pos,out Transform drum)
        {
            parts.Part(name+" metal bezel",transform,pos,new Vector3(.13f,.075f,.035f),parts.Brass);
            parts.Part(name+" black window",transform,pos+Vector3.forward*.02f,new Vector3(.105f,.053f,.008f),parts.Dark);
            drum=M2StationParts.Root(name+" rotating drum",transform,pos+Vector3.forward*.026f);
            var wheel=parts.Part(name+" engraved drum",drum,Vector3.zero,new Vector3(.055f,.045f,.055f),parts.Metal,PrimitiveType.Cylinder);wheel.transform.localRotation=Quaternion.Euler(0,0,90);
            return parts.Text(name+" numbered wheel","0 / 3",transform,pos+Vector3.forward*.058f,.004f);
        }
        void RefreshPhysicalGuidance()
        {
            if(!physicalGuidance)return;
            var e=State.Engine;
            if(shownPrimer!=e.Primer||shownStarter!=e.Pulls)
            {if(shownPrimer>=0&&clunk)sound.PlayOneShot(clunk,.12f);shownPrimer=e.Primer;shownStarter=e.Pulls;primerTally.text=string.Format(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchEngineControls.Immersion.1"),e.Primer,State.PowerTuning.primerSqueezes);starterTally.text=string.Format(LighthouseKeepers.Core.PlayerStrings.Get("legacy.WatchEngineControls.Immersion.2"),e.Pulls,State.PowerTuning.starterPulls);}
            var col=e.Settled?new Color(.05f,.75f,.1f):new Color(.12f,.015f,.01f);block.Clear();block.SetColor("_BaseColor",col);block.SetColor("_EmissionColor",e.Settled?col*.65f:Color.black);if(State.Director!=null)block.SetColor("_EmissionColor",Color.black);finalChokeLamp.SetPropertyBlock(block);
        }
        void AnimateCounters()
        {
            if(!physicalGuidance)return;
            primerDrum.localRotation=Quaternion.Slerp(primerDrum.localRotation,Quaternion.Euler(State.Engine.Primer*36,0,0),1-Mathf.Exp(-18*Time.deltaTime));
            starterDrum.localRotation=Quaternion.Slerp(starterDrum.localRotation,Quaternion.Euler(State.Engine.Pulls*36,0,0),1-Mathf.Exp(-18*Time.deltaTime));
        }
        public bool CanPour(bool held)
        {
            var t=State.Immersion;
            float radius=physicalGuidance?t.fuelFillRadius:.20f;
            float cosine=physicalGuidance?Mathf.Cos(t.fuelPourTiltDegrees*Mathf.Deg2Rad):.35f;
            return held&&!State.Failed&&State.Engine.Fuel<.999f&&Vector3.Distance(FuelCan.Tip.position,Filler.position)<radius&&Vector3.Dot(FuelCan.transform.up,Vector3.up)<cosine;
        }
        void ShowPour(bool pouring)
        {
            if(!physicalGuidance)return;
            fuelStream.enabled=pouring;
            if(pouring){fuelStream.SetPosition(0,FuelCan.Tip.position);fuelStream.SetPosition(1,Filler.position);if(pourSound.clip&&!pourSound.isPlaying)pourSound.Play();}
            else if(pourSound.isPlaying)pourSound.Stop();
        }
    }
}
