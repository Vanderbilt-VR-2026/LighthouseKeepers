using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;
namespace LighthouseKeepers.StormWatch
{
    public sealed class StormWatchBoard : MonoBehaviour
    {
        [SerializeField] StormWatchDirector director;
        [SerializeField,Range(0,4)] int floor;
        TMP_Text title, body, status;
        Button begin, proceed, repair, rescue, leave;
        Button[] calls;
        Canvas canvas;
        float refresh;
        Vector3 home;Quaternion homeRotation;
        bool result;
        void Awake(){home=transform.position;homeRotation=transform.rotation;Build();}
        void Update()
        {
            if(Time.unscaledTime<refresh)return;refresh=Time.unscaledTime+.1f;
            if(!canvas.worldCamera)canvas.worldCamera=Camera.main;
            if(!director||!director.Ready){body.text="Preparing the lighthouse…";return;}
            var s=director.State;
            bool showResult=s.Terminal||director.SessionEnded;
            if(floor==0&&result!=showResult)
            {
                result=showResult;
                if(result){var f=Vector3.ProjectOnPlane(director.Head.forward,Vector3.up).normalized;transform.SetPositionAndRotation(director.Head.position+f*1.8f,Quaternion.LookRotation(f));}
                else transform.SetPositionAndRotation(home,homeRotation);
            }
            title.text=director.SessionEnded?"CREW DISCONNECTED":s.Terminal?(s.act==WatchAct.Won?"BOTH KEEPERS SAFE":"WATCH LOST"):floor==0?"THREE WATCHES / TWO KEEPERS":floor==1?"PLUMBING / MANUAL PUMP":floor==2?"GENERATOR / FIRST REFUGE":floor==3?"RADIO / SECOND REFUGE":"LANTERN / BEACON";
            status.text=$"DAY {s.Day}  •  {Mathf.FloorToInt(s.elapsed/60):00}:{Mathf.FloorToInt(s.elapsed%60):00}  •  WATER {s.water:0.00}m\n"+
                (director.Down?$"GRAB THE LIFELINE • RESCUE {s.Rescue(director.LocalSlot):0}s":s.Air(director.LocalSlot)>0?$"GET ABOVE WATER • AIR {Mathf.Max(0,director.Tuning.airSeconds-s.Air(director.LocalSlot)):0}s":!director.CrewReady?director.Connection:s.power?"POWER ON • PUMP RUNNING":"BLACKOUT • MANUAL PUMP NEEDED");
            bool refuge=s.act==WatchAct.FirstRefuge&&floor==2||s.act==WatchAct.SecondRefuge&&floor==3;
            body.text=director.SessionEnded?"The watch is paused. Return to the crew lobby to reconnect.":s.Terminal?s.message+$"\n{s.mistakes} setbacks • {s.elapsed/60:0.0} minutes together":
                floor==0?"One continuous climb. Roughly ten minutes.\nBrace + wrench → pump + generator → radio + beacon.\nYour crew starts together from the watch room.":
                refuge?"A BREATH BEFORE THE NEXT WATCH\nPack faster repairs OR longer rescue time.\nWater keeps approaching. Both keepers regroup here.":
                floor==1?$"PIPE SEAL STAGES {s.pipeSteps}/3 • NEXT {s.pipeProgress*100:0}%\nOne keeper grips BRACE. Other grips the wrench,\ntouches the coupling and twists. Regrip freely.":
                floor==2?$"GENERATOR STEPS {s.generatorSteps}/3 • NEXT {s.generatorProgress*100:0}%\nGrip and turn the starter. Partner pumps below.\n{(s.unsupported>0?$"PUMP SUPPORT LOST • {Mathf.Max(0,director.Tuning.unsupportedGrace-s.unsupported):0}s TO SETBACK":"The handle below must keep moving to pump.")}":
                floor==3?(s.act==WatchAct.RadioRelay?$"BEACON ORDER: {Symbol(s.order0)} → {Symbol(s.order1)} → {Symbol(s.order2)}\nHold the radio. Call the next symbol to your partner.\nUse the call buttons if you are not on voice chat.":"RADIO RELAY\nRestore plumbing and power below first.\nThen one keeper stays here while the other climbs."):
                $"BEACON SWITCHES {s.beaconSteps}/3 • NEXT {s.beaconProgress*100:0}%\nListen for the radio keeper's symbol.\nGrip and twist the matching switch. Both climb to finish.";
            if(floor>0&&!refuge&&!showResult)body.text+="\n"+s.message;
            Show(begin,s.Terminal&&floor==0&&!director.SessionEnded);begin.interactable=director.CrewReady;
            begin.GetComponentInChildren<TMP_Text>().text="RESTART TOGETHER";
            Show(repair,refuge&&!s.supplyChosen);Show(rescue,refuge&&!s.supplyChosen);Show(proceed,refuge&&s.supplyChosen);Show(leave,true);
            for(int i=0;i<3;i++)Show(calls[i],floor==3&&s.act==WatchAct.RadioRelay);
        }
        public static string Symbol(int index)=>index==0?"SUN":index==1?"WAVE":"STAR";
        static void Show(Button b,bool show){if(b.gameObject.activeSelf!=show)b.gameObject.SetActive(show);}
        void Build()
        {
            if(!FindAnyObjectByType<EventSystem>())new GameObject("Watch pointers",typeof(EventSystem),typeof(XRUIInputModule));
            var root=Rect("Watch board",transform,Vector2.zero,new(1200,850));root.localScale=Vector3.one*.0013f;
            canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            root.gameObject.AddComponent<GraphicRaycaster>();root.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            root.gameObject.AddComponent<Image>().color=new(.025f,.065f,.08f,.98f);
            title=Text(root,"Title",new(0,350),new(1120,85),44);title.color=new(.95f,.7f,.28f);
            body=Text(root,"Instructions",new(0,95),new(1120,380),32);
            status=Text(root,"Status",new(0,-135),new(1120,90),30);
            begin=Button(root,"BEGIN FIRST WATCH",new(0,-265),new(1050,85),()=>director.Command(director.State.Terminal?WatchAction.Retry:WatchAction.Begin));
            repair=Button(root,"FASTER REPAIRS",new(-270,-270),new(510,85),()=>director.Command(WatchAction.RepairSupply));
            rescue=Button(root,"LONGER RESCUES",new(270,-270),new(510,85),()=>director.Command(WatchAction.RescueSupply));
            proceed=Button(root,"OPEN NEXT WATCH",new(0,-270),new(1050,85),()=>director.Command(WatchAction.Continue));
            leave=Button(root,"RETURN TO LOBBY",new(0,-390),new(350,45),()=>director.Leave());
            calls=new Button[3];for(int i=0;i<3;i++){int symbol=i;calls[i]=Button(root,"CALL "+Symbol(i),new((i-1)*350,-285),new(330,90),()=>director.Command((WatchAction)((int)WatchAction.CallSun+symbol)));}
            foreach(var b in new[]{begin,proceed,repair,rescue,leave})b.gameObject.SetActive(false);
            foreach(var b in calls)b.gameObject.SetActive(false);
        }
        static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=pos;return r;}
        static TMP_Text Text(Transform parent,string name,Vector2 pos,Vector2 size,float font){var t=Rect(name,parent,pos,size).gameObject.AddComponent<TextMeshProUGUI>();t.font=TMP_Settings.defaultFontAsset;t.fontSize=font;t.color=new(.95f,.91f,.78f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;}
        static Button Button(Transform parent,string text,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action){var r=Rect(text,parent,pos,size);var image=r.gameObject.AddComponent<Image>();image.color=new(.85f,.6f,.25f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);var label=Text(r,"Label",Vector2.zero,size,29);label.text=text;label.color=new Color(.06f,.07f,.07f);return b;}
    }
}
