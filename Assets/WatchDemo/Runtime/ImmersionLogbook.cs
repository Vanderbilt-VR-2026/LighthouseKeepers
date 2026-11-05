using UnityEngine;
using UnityEngine.UI;
namespace LighthouseKeepers.WatchDemo
{
    // A world-anchored book page, revealed only for deliberate menu or night transition.
    // Overlay material keeps ink readable during the existing comfort fade; no camera movement.
    public sealed class ImmersionLogbook:MonoBehaviour
    {
        GameObject page;Text ink;Transform head;Material material;Texture2D paper;Sprite sprite;
        bool showing;
        public static ImmersionLogbook Create(Transform head,Transform parent)
        {
            var c=parent.gameObject.AddComponent<ImmersionLogbook>();c.head=head;
            c.page=new GameObject("Survived logbook page",typeof(RectTransform),typeof(Canvas),typeof(Image));c.page.transform.SetParent(parent,false);
            var rect=(RectTransform)c.page.transform;rect.sizeDelta=new Vector2(540,360);rect.localScale=Vector3.one*.0015f;
            var canvas=c.page.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=head.GetComponent<Camera>();canvas.overrideSorting=true;canvas.sortingOrder=100;
            c.material=new Material(Resources.Load<Shader>("ImmersionLogbook"));
            Decorate(c.page,c.material,out c.paper,out c.sprite);
            var text=new GameObject("Night written in ink",typeof(RectTransform),typeof(Text));text.transform.SetParent(rect,false);
            c.ink=text.GetComponent<Text>();c.ink.rectTransform.sizeDelta=new Vector2(470,300);c.ink.font=Resources.Load<Font>("Fonts/Kalam-Regular");c.ink.fontSize=31;c.ink.alignment=TextAnchor.MiddleCenter;c.ink.color=new Color(.12f,.08f,.045f);c.ink.material=c.material;c.ink.raycastTarget=false;
            c.page.SetActive(false);return c;
        }
        public static void Decorate(GameObject page,Material overlay,out Texture2D paper,out Sprite sprite)
        {
            paper=new Texture2D(64,64,TextureFormat.RGB24,false);paper.name="Original weathered logbook paper";
            var pixels=new Color[4096];var random=new System.Random(1938);
            for(int y=0;y<64;y++)for(int x=0;x<64;x++){float edge=Mathf.Min(x,63-x,y,63-y)/8f;float v=.85f+(float)random.NextDouble()*.1f;v*=Mathf.Lerp(.78f,1,Mathf.Clamp01(edge));pixels[y*64+x]=new Color(v,v*.91f,v*.73f);}
            paper.SetPixels(pixels);paper.Apply();sprite=Sprite.Create(paper,new Rect(0,0,64,64),new Vector2(.5f,.5f));
            var image=page.GetComponent<Image>();image.sprite=sprite;image.color=Color.white;image.material=overlay;
            var spine=new GameObject("Logbook stitched spine",typeof(RectTransform),typeof(Image));spine.transform.SetParent(page.transform,false);
            var r=(RectTransform)spine.transform;r.anchorMin=new Vector2(0,0);r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,.5f);r.sizeDelta=new Vector2(18,0);r.anchoredPosition=Vector2.zero;
            var leather=spine.GetComponent<Image>();leather.color=new Color(.19f,.095f,.045f);leather.material=overlay;leather.raycastTarget=false;
            for(int i=0;i<8;i++){var stitch=new GameObject("Binding stitch",typeof(RectTransform),typeof(Image));stitch.transform.SetParent(spine.transform,false);var t=(RectTransform)stitch.transform;t.anchorMin=t.anchorMax=new Vector2(.5f,(i+.5f)/8);t.sizeDelta=new Vector2(10,2);var a=stitch.GetComponent<Image>();a.color=new Color(.6f,.42f,.23f);a.material=overlay;a.raycastTarget=false;}
        }
        public void Night(bool sleeping,int day,float fade)
        {
            bool show=sleeping&&day>0&&fade>.60f;
            if(show&&!showing)
            {
                Vector3 forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;if(forward.sqrMagnitude<.5f)forward=Vector3.forward;
                page.transform.SetPositionAndRotation(head.position+forward*.95f-Vector3.up*.08f,Quaternion.LookRotation(forward));
                ink.text="KEEPER'S LOG\n\nNIGHT "+day+" — SURVIVED\n\nDawn. Another watch awaits.";
                var watch=GetComponent<WatchDemoController>();if(watch&&watch.State.PressureEnabled&&(watch.State.PeakDanger>.8f||watch.State.PreviousCloseCall))ink.text+="\n"+KeeperStrings.Get("log.close");
            }
            showing=show;page.SetActive(show);
        }
        void OnDestroy(){if(material)Destroy(material);if(sprite)Destroy(sprite);if(paper)Destroy(paper);if(page)Destroy(page);}
    }
}
