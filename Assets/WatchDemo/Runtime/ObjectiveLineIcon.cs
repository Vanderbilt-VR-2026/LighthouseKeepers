using UnityEngine;
using UnityEngine.UI;
namespace LighthouseKeepers.WatchDemo
{
 public sealed class ObjectiveLineIcon:Graphic
 {
  string kind;public void Set(string value){if(kind==value)return;kind=value;SetVerticesDirty();}
  void Stroke(VertexHelper v,Vector2 a,Vector2 b){var n=new Vector2(-(b-a).y,(b-a).x).normalized*1.4f;int i=v.currentVertCount;v.AddVert(a-n,color,Vector2.zero);v.AddVert(a+n,color,Vector2.zero);v.AddVert(b+n,color,Vector2.zero);v.AddVert(b-n,color,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
  void Path(VertexHelper v,params Vector2[] p){for(int i=1;i<p.Length;i++)Stroke(v,p[i-1],p[i]);}
  protected override void OnPopulateMesh(VertexHelper v)
  {
   v.Clear();
   if(kind==WatchState.PipeStation){Path(v,new Vector2(-10,-16),new Vector2(-3,-5),new Vector2(-8,3),new Vector2(-5,13),new Vector2(1,17),new Vector2(0,7),new Vector2(8,3),new Vector2(14,9),new Vector2(13,0),new Vector2(4,-7),new Vector2(-4,-18),new Vector2(-10,-16));}
   else if(kind==WatchState.GeneratorStation){Path(v,new Vector2(4,18),new Vector2(-10,-1),new Vector2(0,-1),new Vector2(-4,-18),new Vector2(12,4),new Vector2(2,4),new Vector2(4,18));}
   else if(kind==WatchState.RadioStation){Path(v,new Vector2(-15,-12),new Vector2(-15,10),new Vector2(15,10),new Vector2(15,-12),new Vector2(-15,-12));Stroke(v,new Vector2(-8,10),new Vector2(8,20));for(int i=0;i<3;i++)Stroke(v,new Vector2(-9,-6+i*5),new Vector2(3,-6+i*5));}
   else if(kind=="BED"){Path(v,new Vector2(-17,10),new Vector2(-17,-13));Path(v,new Vector2(-17,-3),new Vector2(17,-3),new Vector2(17,-13));Path(v,new Vector2(-14,1),new Vector2(-14,7),new Vector2(-3,7),new Vector2(-3,1),new Vector2(17,1),new Vector2(17,-3));}
   else{Path(v,new Vector2(-11,-15),new Vector2(-8,11),new Vector2(8,11),new Vector2(11,-15),new Vector2(-11,-15));Path(v,new Vector2(-8,11),new Vector2(0,19),new Vector2(8,11));Stroke(v,new Vector2(0,-10),new Vector2(0,7));}
  }
 }
}
