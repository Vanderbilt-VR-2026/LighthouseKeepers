using System;using TMPro;using UnityEngine;using UnityEngine.UI;using LighthouseKeepers.Core;
namespace LighthouseKeepers.Net
{
    // Personal duty folder: no network fields, gameplay timers, audio or readiness requirement.
    public sealed class CrewBriefing:MonoBehaviour
    {
        public const string Preference="LK.Briefing.Wartime1942.v1";
        public static bool Seen=>PlayerPrefs.GetInt(Preference,0)==1;
        public int Page {get;private set;}
        public event Action Closed;
        TMP_Text title,body,footer,count;Button back,next;BriefingIllustration drawing;
        static string S(string key)=>PlayerStrings.Get("briefing."+key);
        public static CrewBriefing Create(Transform parent,Action closed)
        {var c=new GameObject("Harbor duty folder",typeof(RectTransform)).AddComponent<CrewBriefing>();c.transform.SetParent(parent,false);c.Closed+=closed;c.Build();return c;}
        public void Open(){Page=0;gameObject.SetActive(true);Refresh();}
        public void Advance(){if(Page<2){Page++;Refresh();}else Finish();}
        public void Back(){if(Page>0){Page--;Refresh();}}
        public void Skip()=>Finish();
        void Finish(){PlayerPrefs.SetInt(Preference,1);PlayerPrefs.Save();Closed?.Invoke();}
        RectTransform Rect(string name,Vector2 pos,Vector2 size)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(transform,false);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;}
        TMP_Text Text(string name,Vector2 pos,Vector2 size,int fontSize)
        {var t=Rect(name,pos,size).gameObject.AddComponent<TextMeshProUGUI>();t.font=TMP_Settings.defaultFontAsset;t.fontSize=fontSize;t.color=new Color(.10f,.16f,.17f);t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.alignment=TextAlignmentOptions.TopLeft;return t;}
        Button Button(string name,string text,Vector2 pos,Action action)
        {var r=Rect(name,pos,new Vector2(220,58));var image=r.gameObject.AddComponent<Image>();image.color=new Color(.15f,.24f,.25f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(()=>action());var t=Text(name+" label",pos,new Vector2(220,58),26);t.alignment=TextAlignmentOptions.Center;t.text=text;t.color=new Color(.96f,.9f,.75f);return b;}
        void Build()
        {
            var r=(RectTransform)transform;r.sizeDelta=new Vector2(1120,650);
            var paper=gameObject.AddComponent<Image>();paper.color=new Color(.83f,.78f,.64f);paper.raycastTarget=true;
            var spine=Rect("Cloth binding",new Vector2(-546,0),new Vector2(28,650));spine.gameObject.AddComponent<Image>().color=new Color(.12f,.21f,.22f);
            Text("Board stamp",new Vector2(0,280),new Vector2(1010,42),20).text=S("stamp");
            title=Text("Briefing title",new Vector2(0,222),new Vector2(1010,66),37);
            drawing=Rect("Ink illustration",new Vector2(-368,-8),new Vector2(236,330)).gameObject.AddComponent<BriefingIllustration>();drawing.color=new Color(.12f,.23f,.25f);drawing.raycastTarget=false;
            body=Text("Briefing instructions",new Vector2(128,-12),new Vector2(710,360),27);
            footer=Text("Briefing footer",new Vector2(0,-214),new Vector2(1010,50),21);footer.alignment=TextAlignmentOptions.Center;
            back=Button("BriefingBack",S("back"),new Vector2(-360,-282),Back);
            Button("BriefingSkip",S("skip"),new Vector2(0,-282),Skip);
            next=Button("BriefingNext",S("next"),new Vector2(360,-282),Advance);
            count=Text("Briefing progress",new Vector2(422,280),new Vector2(145,38),20);count.alignment=TextAlignmentOptions.TopRight;
            Refresh();
        }
        void Refresh()
        {
            if(!title)return;title.text=S("title."+Page);body.text=S("body."+Page);footer.text=S("footer."+Page);count.text=(Page+1)+" / 3";
            back.interactable=Page>0;transform.Find("BriefingNext label").GetComponent<TMP_Text>().text=S(Page==2?"finish":"next");drawing.Page=Page;drawing.SetAllDirty();
        }
    }
    // Original vector ink, kept in the UI batch. No imported textures, extra camera or realtime light.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BriefingIllustration:Graphic
    {
        public int Page;
        void Quad(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d){int i=v.currentVertCount;v.AddVert(a,color,Vector2.zero);v.AddVert(b,color,Vector2.zero);v.AddVert(c,color,Vector2.zero);v.AddVert(d,color,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
        Vector2 P(float x,float y)=>new((x-.5f)*rectTransform.rect.width,(y-.5f)*rectTransform.rect.height);
        void Line(VertexHelper v,float x,float y,float xx,float yy,float width=.008f){var a=P(x,y);var b=P(xx,yy);var n=new Vector2(-(b-a).y,(b-a).x).normalized*rectTransform.rect.width*width;Quad(v,a-n,a+n,b+n,b-n);}
        void Box(VertexHelper v,float x,float y,float w,float h){Line(v,x,y,x+w,y);Line(v,x+w,y,x+w,y+h);Line(v,x+w,y+h,x,y+h);Line(v,x,y+h,x,y);}
        void Circle(VertexHelper v,float x,float y,float radius){for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;Line(v,x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius*.715f,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius*.715f);}}
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();if(Page==0){
                // Convoy approaching the light across an ink-chart sea.
                for(int j=0;j<4;j++)for(int i=0;i<8;i++)Line(v,.05f+i*.12f,.08f+j*.06f,.10f+i*.12f,.10f+j*.06f);
                Line(v,.52f,.34f,.64f,.82f);Line(v,.64f,.82f,.8f,.82f);Line(v,.8f,.82f,.92f,.34f);Line(v,.52f,.34f,.92f,.34f);Box(v,.61f,.82f,.22f,.08f);Line(v,.58f,.9f,.72f,.97f);Line(v,.72f,.97f,.86f,.9f);Line(v,.61f,.86f,.03f,.58f);Line(v,.61f,.84f,.03f,.43f);Box(v,.68f,.4f,.08f,.13f);
                Line(v,.06f,.40f,.39f,.40f);Line(v,.39f,.4f,.34f,.31f);Line(v,.34f,.31f,.12f,.31f);Line(v,.12f,.31f,.06f,.4f);Box(v,.15f,.4f,.13f,.07f);Line(v,.22f,.47f,.22f,.58f);
            }else if(Page==1){
                // Pipe, engine, beam and radio aligned with the four written responsibilities.
                Box(v,.12f,.78f,.7f,.05f);Circle(v,.45f,.81f,.12f);Line(v,.45f,.71f,.45f,.66f);
                Box(v,.15f,.48f,.65f,.14f);Box(v,.23f,.62f,.25f,.045f);Circle(v,.3f,.54f,.05f);Circle(v,.65f,.54f,.05f);
                Circle(v,.3f,.35f,.14f);Line(v,.45f,.39f,.88f,.44f);Line(v,.45f,.31f,.88f,.25f);
                Box(v,.16f,.04f,.66f,.15f);Circle(v,.32f,.11f,.05f);for(int i=0;i<3;i++)Line(v,.5f,.08f+i*.035f,.72f,.08f+i*.035f);
            }else{
                // Pair of Touch-style grips, rings, sticks and face buttons.
                for(int i=0;i<2;i++){float x=.25f+i*.5f;Circle(v,x,.76f,.22f);Circle(v,x,.76f,.065f);Line(v,x-.12f,.64f,x-.09f,.3f);Line(v,x-.09f,.3f,x+.07f,.3f);Line(v,x+.07f,.3f,x+.13f,.65f);Circle(v,x+.1f,.62f,.027f);Circle(v,x+.11f,.55f,.027f);Line(v,x-.13f,.5f,x-.20f,.46f);Line(v,x-.20f,.46f,x-.13f,.42f);}
                // Recall is the upper right face button (B).
                Circle(v,.86f,.62f,.052f);Line(v,.85f,.615f,.85f,.633f);Line(v,.85f,.633f,.87f,.633f);Line(v,.87f,.633f,.87f,.615f);Line(v,.85f,.625f,.87f,.625f);Line(v,.85f,.615f,.87f,.615f);
            }
        }
    }
}
