using System.Collections.Generic;
using UnityEngine;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    // Fixed physical indicators; no lights, HUD panels, or independent gameplay state.
    public sealed class ConnectedMachinePresentation : MonoBehaviour
    {
        WatchDemoController watch;M2StationParts parts;MaterialPropertyBlock block;
        Renderer reportLamp;Transform reportFlag;
        public static void Create(WatchDemoController w)
        {if(!w.State.Connected||!w.State.Connected.dependencies)return;var c=w.gameObject.AddComponent<ConnectedMachinePresentation>();c.watch=w;c.Build();}
        void Build()
        {
            parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();block=new MaterialPropertyBlock();
            TowerWiringPresentation.Create(watch);
            var radio=FindAnyObjectByType<WatchRadioConsole>();
            var plate=M2StationParts.Root("Beacon proof instrument",radio.transform,new Vector3(.66f,1.66f,.35f));
            parts.Part("Proof enamel plate",plate,Vector3.zero,new Vector3(.31f,.14f,.025f),parts.Dark);
            parts.Text("Proof stencil",PlayerStrings.Get("connected.lightProof"),plate,new Vector3(0,.035f,.016f),.0019f);
            reportLamp=parts.Part("Beacon proof lamp",plate,new Vector3(-.09f,-.025f,.025f),new Vector3(.035f,.035f,.025f),parts.Brass).GetComponent<Renderer>();
            reportFlag=parts.Part("Beacon proof mechanical flag",plate,new Vector3(.07f,-.025f,.027f),new Vector3(.06f,.013f,.013f),parts.Metal).transform;
        }
        void LateUpdate()
        {
            var s=watch.State;
            bool proof=s.BeaconShining&&s.HasPower(PowerCircuit.Radio)&&!s.Failed;
            Colour(reportLamp,proof?new Color(.25f,.75f,.38f):new Color(.12f,.055f,.025f),proof?.8f:0);
            reportFlag.localRotation=Quaternion.Slerp(reportFlag.localRotation,Quaternion.Euler(0,0,proof?45:0),1-Mathf.Exp(-8*Time.deltaTime));
        }
        void Colour(Renderer r,Color c,float emission){block.Clear();block.SetColor("_BaseColor",c);block.SetColor("_EmissionColor",c*emission);r.SetPropertyBlock(block);}
    }
}
