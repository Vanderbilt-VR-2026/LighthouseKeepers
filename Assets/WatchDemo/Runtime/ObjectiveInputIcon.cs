using UnityEngine;using UnityEngine.UI;
namespace LighthouseKeepers.WatchDemo {
 // Small controller silhouette. The illuminated side paddle/forefinger trigger identifies the input.
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ObjectiveInputIcon:Graphic {
  public override Texture mainTexture=>Texture2D.whiteTexture;
  bool trigger;public void Set(bool value){if(trigger==value)return;trigger=value;SetVerticesDirty();}
  void Quad(VertexHelper v,Vector2 centre,Vector2 size,Color tint){int i=v.currentVertCount;var half=size*.5f;v.AddVert(centre+new Vector2(-half.x,-half.y),tint,Vector2.zero);v.AddVert(centre+new Vector2(-half.x,half.y),tint,Vector2.zero);v.AddVert(centre+half,tint,Vector2.zero);v.AddVert(centre+new Vector2(half.x,-half.y),tint,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();var idle=new Color(.68f,.69f,.65f);var lit=new Color(.95f,.75f,.32f);Quad(v,new Vector2(0,-5),new Vector2(15,28),idle);Quad(v,new Vector2(0,13),new Vector2(24,10),idle);Quad(v,new Vector2(-9,-5),new Vector2(5,13),trigger?idle:lit);Quad(v,new Vector2(10,8),new Vector2(8,6),trigger?lit:idle);}
 }
}
