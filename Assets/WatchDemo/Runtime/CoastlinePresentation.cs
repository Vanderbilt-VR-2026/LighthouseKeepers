using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo {
 public sealed class CoastlinePresentation:MonoBehaviour {
  Mesh combined,rock;Material wet;readonly List<Mesh> owned=new();
  public static void Create(WatchDemoController w){if(!VisualPassSettings.Current||!VisualPassSettings.Current.water)return;w.gameObject.AddComponent<CoastlinePresentation>();}
  void Start(){wet=Resources.Load<Material>("VisualMaterials/WetStone");var temp=GameObject.CreatePrimitive(PrimitiveType.Sphere);rock=Instantiate(temp.GetComponent<MeshFilter>().sharedMesh);temp.SetActive(false);Destroy(temp);var v=rock.vertices;for(int i=0;i<v.Length;i++){float n=1+.18f*Mathf.Sin(v[i].x*31+v[i].y*47+v[i].z*53);v[i]=Vector3.Scale(v[i]*n,new Vector3(1,.75f,1));}rock.vertices=v;rock.RecalculateNormals();rock.RecalculateBounds();owned.Add(rock);
   var pieces=new List<CombineInstance>();for(int n=0;n<34;n++){float a=n*Mathf.PI*2/34;float radius=7.1f+Mathf.Sin(n*2.1f)*1.7f;var p=new Vector3(Mathf.Sin(a)*radius,-.9f,Mathf.Cos(a)*radius);if(p.z< -3.5f&&Mathf.Abs(p.x)<4.5f)continue;var scale=new Vector3(2.4f+Mathf.Sin(n)*.8f,1.7f+Mathf.Cos(n*1.3f)*.6f,2.6f);pieces.Add(new CombineInstance{mesh=rock,transform=Matrix4x4.TRS(p,Quaternion.Euler(n*7,n*53,n*3),scale)});}
   // Two broken arms leave an open eastern inlet rather than a uniform circular island.
   for(int n=0;n<10;n++){float side=n<5?-1:1;float t=n%5;pieces.Add(new CombineInstance{mesh=rock,transform=Matrix4x4.TRS(new Vector3(8+t*1.45f,-1.0f,side*(3.3f+t*.4f)),Quaternion.Euler(0,n*41,12),new Vector3(2.4f,1.8f,2.6f))});}
   combined=new Mesh{name="Combined rocky inlet, no player collision"};combined.indexFormat=IndexFormat.UInt32;combined.CombineMeshes(pieces.ToArray(),true,true);var go=new GameObject("Wet rocky coastline and eastern inlet");go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=combined;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=wet;r.shadowCastingMode=ShadowCastingMode.Off;
   // Dress the already-supported House threshold only. Do not invent an unsafe walkable route.
   var path=GameObject.CreatePrimitive(PrimitiveType.Cube);path.name="Wet worn stone at House threshold";path.transform.SetParent(transform,false);path.transform.position=new Vector3(0,.006f,-4.8f);path.transform.localScale=new Vector3(1.28f,.008f,1.4f);var c=path.GetComponent<Collider>();c.enabled=false;Destroy(c);path.GetComponent<Renderer>().sharedMaterial=wet;
  }
  void OnDestroy(){if(combined)Destroy(combined);foreach(var m in owned)if(m)Destroy(m);}
 }
}
