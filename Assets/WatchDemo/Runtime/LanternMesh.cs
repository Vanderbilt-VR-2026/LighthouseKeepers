using UnityEngine;
namespace LighthouseKeepers.WatchDemo {
 public static class LanternMesh {
  public static readonly Vector3 Handle=new(0,.37f,0),Filler=new(.08f,.08f,0);
  public static Mesh Metal(){var m=new WorkshopMesh();m.Tube(new Vector3(0,.018f,0),new Vector3(0,.06f,0),.085f,16);m.Tube(new Vector3(0,.06f,0),new Vector3(0,.082f,0),.068f,16);m.Tube(new Vector3(0,.22f,0),new Vector3(0,.245f,0),.068f,16);m.Tube(new Vector3(0,.245f,0),new Vector3(0,.27f,0),.046f,12);
   for(int side=-1;side<=1;side+=2)m.Pipe(.009f,new Vector3(side*.075f,.045f,0),new Vector3(side*.09f,.1f,0),new Vector3(side*.083f,.235f,0),new Vector3(side*.04f,.27f,0));
   for(int i=0;i<10;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;m.Tube(new Vector3(Mathf.Cos(a)*.084f,.245f+Mathf.Sin(a)*.13f,0),new Vector3(Mathf.Cos(b)*.084f,.245f+Mathf.Sin(b)*.13f,0),.004f,8);}
   for(int z=-1;z<=1;z+=2)m.Pipe(.0025f,new Vector3(-.06f,.09f,z*.035f),new Vector3(0,.075f,z*.065f),new Vector3(.06f,.09f,z*.035f));
   m.Tube(new Vector3(.075f,.067f,0),Filler,.016f,12);return m.Finish("Hurricane lantern pressed tin and wire bail");}
  public static Mesh Flame(){var m=new Mesh{name="Eight triangle oil flame"};m.vertices=new[]{new Vector3(0,.095f,0),new Vector3(0,.153f,0),new Vector3(.01f,.115f,0),new Vector3(0,.115f,.01f),new Vector3(-.01f,.115f,0),new Vector3(0,.115f,-.01f)};m.triangles=new[]{0,3,2,0,4,3,0,5,4,0,2,5,1,2,3,1,3,4,1,4,5,1,5,2};m.RecalculateNormals();m.RecalculateBounds();return m;}
  public static Mesh Globe(){const int sides=16;float[] y={.082f,.105f,.17f,.215f,.23f},r={.04f,.059f,.061f,.044f,.035f};var v=new Vector3[y.Length*(sides+1)];var uv=new Vector2[v.Length];var indices=new int[(y.Length-1)*sides*6];int k=0;for(int j=0;j<y.Length;j++)for(int i=0;i<=sides;i++){int n=j*(sides+1)+i;float a=i*Mathf.PI*2/sides;v[n]=new Vector3(Mathf.Cos(a)*r[j],y[j],Mathf.Sin(a)*r[j]);uv[n]=new Vector2(i/(float)sides,j/(float)(y.Length-1));if(j>0&&i<sides){int p=n-(sides+1);indices[k++]=p;indices[k++]=n;indices[k++]=n+1;indices[k++]=p;indices[k++]=n+1;indices[k++]=p+1;}}var m=new Mesh{name="Curved hurricane glass globe"};m.vertices=v;m.uv=uv;m.triangles=indices;m.RecalculateNormals();m.RecalculateBounds();return m;}
 }
}
