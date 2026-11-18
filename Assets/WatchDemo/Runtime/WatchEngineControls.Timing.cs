using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchEngineControls
    {
        Transform timingNeedle;TextMesh timingCaption;
        void BuildTimedStarter()
        {
            Primer.gameObject.SetActive(false);Choke.gameObject.SetActive(false);
            foreach(Transform t in transform){
                if(t.name.Contains("Primer")||t.name.Contains("Choke")||t.name=="Final choke lamp label"||t.name=="PRIMED lamp"||t.name=="CHOKE lamp")t.gameObject.SetActive(false);
            }
            var instructions=transform.Find("Engraved starting order");if(instructions){instructions.name="Timed starter order";instructions.GetComponent<TextMesh>().text=KeeperStrings.Get("starter.timed.plate");}
            var label=transform.Find("Starter plate");if(label){label.name="Timed starter grip";label.GetComponent<TextMesh>().text=KeeperStrings.Get("starter.timed.hint");}
            parts.Part("Starter timing brass rim",transform,new Vector3(-.16f,1.20f,1.01f),new Vector3(.57f,.25f,.045f),parts.Brass);
            parts.Part("Starter timing face",transform,new Vector3(-.16f,1.20f,1.038f),new Vector3(.53f,.21f,.012f),parts.Dark);
            float width=State.PowerTuning.starterWindowWidth*.46f;
            parts.Part("Starter marked success band",transform,new Vector3(-.16f,1.23f,1.049f),new Vector3(width,.09f,.005f),parts.Green);
            // Not colour alone: raised brackets outline the success zone.
            foreach(int side in new[]{-1,1})parts.Part("Timing band bracket",transform,new Vector3(-.16f+side*width*.5f,1.23f,1.054f),new Vector3(.008f,.11f,.005f),parts.Brass);
            timingNeedle=parts.Part("Starter timing needle",transform,Vector3.zero,new Vector3(.012f,.125f,.006f),parts.Brass).transform;
            timingCaption=parts.Text("Starter timing instruction","",transform,new Vector3(-.16f,1.145f,1.058f),.004f);
        }
        void UpdateTimedStarter()
        {
            if(!timingNeedle)return;
            timingNeedle.localPosition=new Vector3(-.39f+State.StarterPhase*.46f,1.23f,1.061f);
            timingCaption.text=KeeperStrings.Get(State.Engine.Running?"starter.timed.running":State.StarterInWindow?"starter.timed.ready":"starter.timed.wait");
        }
    }
}
