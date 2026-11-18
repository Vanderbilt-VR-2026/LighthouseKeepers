using System.Text;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using LighthouseKeepers.Core;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.WatchDemo
{
    // Three local grip switches on the existing F4 set; only the watch host judges a reply.
    public sealed class RadioAuthentication:MonoBehaviour
    {
        WatchDemoController watch;WatchRadioConsole console;HarborRadio narrator;HorrorTuning tuning;
        readonly XRSimpleInteractable[] controls=new XRSimpleInteractable[3];
        readonly Renderer[] keys=new Renderer[3];MaterialPropertyBlock glow;
        GameObject panel;TextMesh readout;int lastSerial=-1;AudioSource taps;float secondTap;
        public static void Create(WatchDemoController watch){var a=watch.gameObject.AddComponent<RadioAuthentication>();a.watch=watch;}
        public Component Control(float command)=>command==(int)command&&command>=0&&command<3?controls[(int)command]:null;
        public bool Visible=>watch&&watch.State.Day==3&&watch.State.Authenticity.Visible&&!watch.State.Failed&&!watch.Sleeping;
        public string Speaker=>watch.State.Authenticity.Kind==2?KeeperStrings.Get("lesson.unknown"):KeeperStrings.Get("auth.navy");
        public string Reply
        {
            get {var state=watch.State;var text=new StringBuilder();for(int i=0;i<state.Radio.CodeLength;i++){if(i>0)text.Append(" · ");int symbol=state.Authenticity.ReplySymbol(i,state.Radio.CodeSymbol(i));text.Append(string.Format(KeeperStrings.Get("code.symbol"),symbol+1,KeeperStrings.Get("colour."+symbol)));}return text.ToString();}
        }
        public string Caption
        {
            get {
                var a=watch.State.Authenticity;
                if(a.Stage==1)return KeeperStrings.Get(a.Kind==1?"auth.real":"auth.false");
                if(a.Stage==2)return string.Format(KeeperStrings.Get(a.Kind==1?"auth.replyReal":"auth.replyFalse"),Reply);
                if(a.Stage==3)return KeeperStrings.Get("auth.correction");
                return KeeperStrings.Get(a.Kind==1?"auth.verified":"auth.rejected");
            }
        }
        void Start()
        {
            tuning=HorrorTuning.Current;console=FindAnyObjectByType<WatchRadioConsole>();narrator=FindAnyObjectByType<HarborRadio>();
            var parts=watch.GetComponent<M2StationParts>();if(!console||!parts){enabled=false;return;}
            panel=new GameObject("Navy authentication switches");panel.transform.SetParent(console.transform,false);
            panel.transform.localPosition=new Vector3(.5f,.83f,.34f);
            parts.Part("Authentication enamel plate",panel.transform,Vector3.zero,new Vector3(.76f,.24f,.045f),parts.Dark);
            for(int i=0;i<3;i++){
                int command=i;
                var g=new GameObject("Navy call "+i);g.SetActive(false);g.transform.SetParent(panel.transform,false);g.transform.localPosition=new Vector3(-.25f+i*.25f,-.035f,.055f);
                keys[i]=parts.Part("Brass grip key",g.transform,Vector3.zero,new Vector3(.08f,.075f,.055f),parts.Brass).GetComponent<Renderer>();
                var c=g.AddComponent<BoxCollider>();c.size=new Vector3(.12f,.11f,.09f);
                var rb=g.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
                var input=controls[i]=g.AddComponent<XRSimpleInteractable>();input.colliders.Add(c);
                input.selectEntered.AddListener(e=>{
                    if(e.interactorObject is XRDirectInteractor){if(watch.State.Coop!=null)watch.State.Coop.Send(CoopAction.AuthenticateCall,command,true);else watch.ApplyCommand(CoopAction.AuthenticateCall,command,true);}
                    else e.manager.SelectExit(e.interactorObject,e.interactableObject);
                });
                parts.Text("Navy key label",KeeperStrings.Get("auth.button."+i),panel.transform,new Vector3(-.25f+i*.25f,.07f,.028f),.0045f);
                g.SetActive(true);
            }
            readout=console.readout;
            // Reuse the decorative service-meter strip; leave all broadcast controls unobstructed.
            foreach(var t in console.GetComponentsInChildren<Transform>(true))if(t.name=="Passive service meter"||t.name.StartsWith("Service meter "))t.gameObject.SetActive(false);
            taps=panel.AddComponent<AudioSource>();taps.playOnAwake=false;taps.spatialBlend=1;taps.minDistance=1;taps.maxDistance=10;taps.volume=.18f;taps.clip=Resources.Load<AudioClip>("S4Audio/bolt-clunk");KeeperAudioMix.Route(taps,"SFX_World");
            glow=new MaterialPropertyBlock();panel.SetActive(watch.State.Day==3&&tuning.radioAuthentication);
        }
        void LateUpdate(){if(Visible&&readout)readout.text=watch.State.Authenticity.Stage==2?Reply:KeeperStrings.Get("auth.plate");}
        void Update()
        {
            if(!panel)return;var s=watch.State;var a=s.Authenticity;
            panel.SetActive(s.Day==3&&tuning.radioAuthentication);
            bool live=s.HasPower(PowerCircuit.Radio)&&s.Radio.MainOn&&s.Radio.Locked;
            bool gone=s.Ships!=null&&(s.Ships.PassedMask&1)!=0;
            bool approaching=s.Ships!=null&&(s.Ships.SeenMask&1)!=0;
            if(!s.ReadOnly&&!s.Failed&&!watch.Sleeping)a.Tick(Time.deltaTime,s.Day==3,tuning.radioAuthentication,live,approaching,gone,s.Recovery.LessonActive||narrator&&narrator.SubtitleAlpha>0);
            for(int i=0;i<controls.Length;i++){controls[i].enabled=live&&a.NeedsDecision&&!s.Failed;bool next=controls[i].enabled&&(a.Stage==2?i>0:i==0);glow.SetColor("_EmissionColor",next?new Color(.4f,.22f,.06f)*(.65f+.25f*Mathf.Sin(Time.time*5.2f)):Color.black);keys[i].SetPropertyBlock(glow);}

            if(lastSerial!=a.Serial){lastSerial=a.Serial;if(Visible){if(narrator)narrator.RecordSilentLine(Caption,Speaker);watch.GetComponent<ObjectiveHud>()?.Reveal();if(a.Kind==1&&a.Stage==2&&taps.clip){taps.Play();secondTap=Time.unscaledTime+.35f;}}}
            if(secondTap>0&&Time.unscaledTime>=secondTap){secondTap=0;if(Visible&&a.Kind==1&&a.Stage==2&&taps.clip)taps.Play();}
        }
    }
}
