using System.Text;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    public sealed class CodeSource : MonoBehaviour
    {
        [SerializeField] RadioProgram program;
        [SerializeField] TextMesh readout;
        WatchDemoController watch; Vector3 localSite; Quaternion localRotation; bool placed; int stamp=-1; bool relay; Material paperMaterial;
        public static CodeSource Active {get;private set;}
        public RadioProgram Program=>program;
        public string PrintedText=>readout?readout.text:"";
        public bool InHouse=>relay;
        void Awake(){Active=this;if(readout)readout.name="Night telegram ink";localSite=transform.position;localRotation=Quaternion.identity;}
        public static string CodeText(RadioState radio)
        {
            var text=new StringBuilder();
            for(int i=0;i<radio.CodeLength;i++){
                if(i>0)text.Append(i%2==0?"\n":"   >   ");
                int symbol=radio.CodeSymbol(i);text.Append(string.Format(KeeperStrings.Get("code.symbol"),symbol+1,KeeperStrings.Get("colour."+symbol)));
            }
            return text.ToString();
        }
        public void Refresh()
        {
            if(!watch)watch=FindAnyObjectByType<WatchDemoController>();
            if(!watch||watch.State==null)return;
            var s=watch.State;bool house=s.RadioRelay;
            if(stamp==s.Day&&placed&&relay==house)return;
            stamp=s.Day;relay=house;placed=true;
            // Same fixed paper changes location as the crew changes. A lone human retains a local
            // clue until the actual companion can fetch it; never require an absent bot.
            transform.SetPositionAndRotation(house?new Vector3(-3.34f,1.12f,-6.4f):localSite,house?Quaternion.Euler(0,270,0):localRotation);
            transform.localScale=Vector3.one;
            var paper=transform.GetChild(0);paper.localPosition=Vector3.zero;paper.localRotation=Quaternion.identity;paper.localScale=new Vector3(.62f,.32f,.016f);
            var renderer=paper.GetComponent<Renderer>();if(renderer){if(!paperMaterial){paperMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Weathered telegram paper"};paperMaterial.SetColor("_BaseColor",new Color(.57f,.54f,.42f));}renderer.sharedMaterial=paperMaterial;renderer.SetPropertyBlock(null);}
            readout.transform.localPosition=new Vector3(0,0,-.012f);readout.transform.localRotation=Quaternion.identity;readout.transform.localScale=Vector3.one;
            readout.characterSize=.0038f;readout.fontSize=64;readout.anchor=TextAnchor.MiddleCenter;readout.alignment=TextAlignment.Center;readout.color=new Color(.06f,.065f,.05f);
            readout.text=string.Format(KeeperStrings.Get("code.telegram"),s.Day)+"\n"+KeeperStrings.Get(house?"code.share":"code.local")+"\n"+CodeText(s.Radio);
        }
        void Start()=>Refresh();
        void Update(){if(!watch||watch.State==null||stamp!=watch.State.Day||relay!=watch.State.RadioRelay)Refresh();}
        void OnDestroy(){if(Active==this)Active=null;if(paperMaterial)Destroy(paperMaterial);}
    }
}
