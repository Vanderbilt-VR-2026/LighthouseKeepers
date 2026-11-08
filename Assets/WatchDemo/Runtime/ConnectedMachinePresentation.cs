using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    // Fixed physical indicators; no lights, HUD panels, or independent gameplay state.
    public sealed class ConnectedMachinePresentation : MonoBehaviour
    {
        WatchDemoController watch;M2StationParts parts;MaterialPropertyBlock block;
        readonly List<Renderer> sleeves=new();Renderer reportLamp;Transform reportFlag;bool wasPowered;float poweredSince;
        public static void Create(WatchDemoController w)
        {if(!w.State.Connected||!w.State.Connected.dependencies)return;var c=w.gameObject.AddComponent<ConnectedMachinePresentation>();c.watch=w;c.Build();}
        void Build()
        {
            parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();block=new MaterialPropertyBlock();
            var root=M2StationParts.Root("Tower power conduit",transform,Vector3.zero);
            var mount=PipeWallLayout.WallSurface(1.7f,out var inward)+inward*.045f;
            var tube=parts.Part("Wall mounted riser",root,new Vector3(mount.x,6,mount.z),new Vector3(.045f,5.2f,.045f),parts.Dark,PrimitiveType.Cylinder);
            for(int i=0;i<12;i++)
            {
                var y=.9f+i*.9f;
                parts.Part("Conduit saddle",root,new Vector3(mount.x,y,mount.z)-inward*.015f,new Vector3(.09f,.04f,.13f),parts.Metal);
                sleeves.Add(parts.Part("Power flow inspection sleeve "+i,root,new Vector3(mount.x,y,mount.z)+inward*.025f,new Vector3(.025f,.15f,.06f),parts.Brass).GetComponent<Renderer>());
            }
            var radio=FindAnyObjectByType<WatchRadioConsole>();
            var plate=M2StationParts.Root("Beacon proof instrument",radio.transform,new Vector3(.66f,1.66f,.35f));
            parts.Part("Proof enamel plate",plate,Vector3.zero,new Vector3(.31f,.14f,.025f),parts.Dark);
            parts.Text("Proof stencil",PlayerStrings.Get("connected.lightProof"),plate,new Vector3(0,.035f,.016f),.0019f);
            reportLamp=parts.Part("Beacon proof lamp",plate,new Vector3(-.09f,-.025f,.025f),new Vector3(.035f,.035f,.025f),parts.Brass).GetComponent<Renderer>();
            reportFlag=parts.Part("Beacon proof mechanical flag",plate,new Vector3(.07f,-.025f,.027f),new Vector3(.06f,.013f,.013f),parts.Metal).transform;
        }
        void LateUpdate()
        {
            var s=watch.State;bool powered=s.Power==PowerState.On&&!s.Failed;
            if(powered&&!wasPowered)poweredSince=Time.time;wasPowered=powered;
            for(int i=0;i<sleeves.Count;i++)
            {float arrival=i/(float)(sleeves.Count-1)*s.Connected.conduitTravelSeconds;bool lit=powered&&Time.time-poweredSince>=arrival;Colour(sleeves[i],lit?new Color(.8f,.48f,.12f):new Color(.06f,.045f,.025f),lit?.65f:0);}
            bool proof=s.BeaconShining&&s.HasPower(PowerCircuit.Radio)&&!s.Failed;
            Colour(reportLamp,proof?new Color(.25f,.75f,.38f):new Color(.12f,.055f,.025f),proof?.8f:0);
            reportFlag.localRotation=Quaternion.Slerp(reportFlag.localRotation,Quaternion.Euler(0,0,proof?45:0),1-Mathf.Exp(-8*Time.deltaTime));
        }
        void Colour(Renderer r,Color c,float emission){block.Clear();block.SetColor("_BaseColor",c);block.SetColor("_EmissionColor",c*emission);r.SetPropertyBlock(block);}
    }
}
