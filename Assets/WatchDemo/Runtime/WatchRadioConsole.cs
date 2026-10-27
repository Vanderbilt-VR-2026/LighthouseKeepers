using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class WatchRadioConsole : MonoBehaviour
    {
        public RadioProgram program;
        public TextMesh readout;
        public Transform frequencyNeedle;
        public Transform[] meterNeedles;
        public Renderer lockLamp,transmitLamp;
        public Renderer[] panelLamps;
        public RadioControl[] controls;
        public Renderer[] stepLamps,controlHighlights;
        public TextMesh[] stepLabels;
        public Renderer microphoneHighlight;
        public RadioMicrophone microphone;
        public AudioSource effects,hiss,tone,chatter;
        public AudioClip click,buzzer,reply;
        WatchState watch;
        MaterialPropertyBlock dark,green,red,current,codeBlock,stepDone;
        MaterialPropertyBlock[] panelOn,panelOff;
        float nextReadout;int currentDay;bool completed;
        public RadioState State=>watch.Radio;
        void Awake()
        {
            codeBlock=new MaterialPropertyBlock();stepDone=Block(new Color(.035f,.20f,.065f),new Color(.02f,.16f,.035f));
            current=Block(new Color(.95f,.65f,.12f),new Color(1,.55f,.08f));
            dark=Block(new Color(.035f,.04f,.035f),Color.black);
            green=Block(new Color(.05f,.7f,.15f),new Color(.06f,1,.12f));red=Block(new Color(.8f,.03f,.015f),new Color(1,.03f,.01f));
            panelOn=new MaterialPropertyBlock[panelLamps.Length];panelOff=new MaterialPropertyBlock[panelLamps.Length];
            for(int i=0;i<panelLamps.Length;i++){var c=panelLamps[i].sharedMaterial.GetColor("_BaseColor");panelOn[i]=Block(c,c*.9f);panelOff[i]=Block(c*.25f,Color.black);}
        }
        static MaterialPropertyBlock Block(Color b,Color e){var p=new MaterialPropertyBlock();p.SetColor("_BaseColor",b);p.SetColor("_EmissionColor",e);return p;}
        public void Bind(WatchState value)
        {
            if(watch!=null){watch.Changed-=Changed;watch.Radio.BroadcastCompleted-=Complete;}
            watch=value;watch.Changed+=Changed;watch.Radio.BroadcastCompleted+=Complete;Changed();
        }
        void Changed()
        {
            if(currentDay!=watch.Day || (watch.Elapsed==0&&!State.MainOn))
            {currentDay=watch.Day;foreach(var control in controls)control.ResetControl();completed=false;}
            nextReadout=0;
        }
        void Complete(){effects.PlayOneShot(reply,.7f);completed=true;nextReadout=0;}
        public bool ToggleMain(){bool changed=State.ToggleMain();nextReadout=0;return changed;}
        public void Tune(float normalized)=>State.Tune(Mathf.Lerp(program.minimumMHz,program.maximumMHz,normalized));
        public void Press(RadioControl control)
        {
            if(control.codeColour>=0)
            {int result=State.PressCode(control.codeColour);if(result==0)effects.PlayOneShot(buzzer,.35f);else Click();}
            else Click();
            nextReadout=0;
        }
        public void Click(float volume=.5f)=>effects.PlayOneShot(click,volume);
        void Update()
        {
            if(watch==null)return;
            State.SetMicrophone(microphone.Held,microphone.TriggerHeld);
            bool live=State.MainOn&&watch.Power==PowerState.On;
            float distance=Mathf.Abs(State.FrequencyMHz-program.targetMHz);
            float quality=1-Mathf.Clamp01(distance/.45f);
            hiss.volume=live&&!completed?.09f*(1-quality):0;
            tone.volume=live&&!completed?.055f*quality:0;
            bool sending=State.Transmitting&&!watch.Failed;
            if(sending&&!chatter.isPlaying)chatter.Play();else if(!sending&&chatter.isPlaying)chatter.Stop();
            frequencyNeedle.localPosition=new Vector3(Mathf.Lerp(-.04f,-.92f,Mathf.InverseLerp(program.minimumMHz,program.maximumMHz,State.FrequencyMHz)),1.54f,.32f);
            for(int i=0;i<meterNeedles.Length;i++)
            {float value=sending?.5f+.4f*Mathf.Sin(Time.time*(13+i*4)):(live?.10f+.06f*Mathf.Sin(Time.time*3+i):0);meterNeedles[i].localRotation=Quaternion.Euler(0,0,Mathf.Lerp(50,-50,value));}
            if(Time.unscaledTime<nextReadout)return;nextReadout=Time.unscaledTime+.1f;
            foreach(var control in controls)control.SyncMain(State.MainOn);
            lockLamp.SetPropertyBlock(live&&State.Locked?green:dark);transmitLamp.SetPropertyBlock(sending?red:dark);
            // Utility and out-of-step controls still move/click, but do not light or undo progress.
            for(int i=0;i<panelLamps.Length;i++)panelLamps[i].SetPropertyBlock(dark);
            float pulse=.55f+.20f*Mathf.Sin(Time.unscaledTime*Mathf.PI);
            current.SetColor("_EmissionColor",new Color(1,.55f,.08f)*pulse);
            for(int i=0;i<stepLamps.Length;i++)
            {stepLamps[i].SetPropertyBlock(i<State.Step?stepDone:i==State.Step?current:dark);stepLabels[i].color=i<State.Step?new Color(.3f,1,.4f):i==State.Step?new Color(1,.85f,.35f):new Color(.38f,.4f,.38f);}
            for(int i=0;i<controls.Length;i++)
            {
                var control=controls[i];bool relevant=control.mainPower&&State.Step==0||control.frequency&&State.Step==1||control.codeColour>=0&&State.Step==2;
                controlHighlights[i].SetPropertyBlock(relevant?current:dark);
                if(control.codeColour>=0)
                {var face=control.visual.GetComponentInChildren<Renderer>();var color=face.sharedMaterial.GetColor("_BaseColor");codeBlock.SetColor("_BaseColor",color*(relevant?1:.25f));codeBlock.SetColor("_EmissionColor",relevant?color*pulse:Color.black);face.SetPropertyBlock(codeBlock);}
            }
            microphoneHighlight.SetPropertyBlock(State.Step==3?current:dark);
            if(watch.Failed)readout.text="WATCH LOST\nPRESS TRIGGER TO RETRY";
            else if(watch.Power==PowerState.Off)readout.text="NO POWER\nSTART GENERATOR ON F2";
            else if(!State.MainOn)readout.text="1  FLIP MAIN POWER";
            else if(State.Completed)readout.text="COPY, KEEPER\nWARNING RECEIVED";
            else if(sending)readout.text=$"TRANSMITTING  {State.TransmitElapsed:F1}s\nKEEP TRIGGER HELD";
            else if(State.Step==1)readout.text=$"2  TURN DIAL TO {program.targetMHz:F1}\n{State.FrequencyMHz:F2} MHz  /  HOLD TO LOCK";
            else if(State.Step==2)readout.text=$"3  ENTER CALL CARD CODE\n{State.CodeProgress}/{program.callCode.Length} BUTTONS";
            else readout.text="4  GRAB MIC + HOLD TRIGGER\nSEND FOR 3 SECONDS";
        }
        void OnDestroy(){if(watch!=null){watch.Changed-=Changed;watch.Radio.BroadcastCompleted-=Complete;}}
    }
}
