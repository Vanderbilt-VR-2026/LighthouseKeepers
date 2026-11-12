using System.Collections.Generic;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Presentation only. Starter, grip anchors, engine state and original moving parts stay intact.
    public sealed class GeneratorIdentity : MonoBehaviour
    {
        Material yellow; Mesh conduit;
        public static void Create(){var g=FindAnyObjectByType<WatchGenerator>();if(g&&!g.GetComponent<GeneratorIdentity>())g.gameObject.AddComponent<GeneratorIdentity>().Build();}
        void Build()
        {
            var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();
            yellow=new Material(parts.Metal){name="Worn diesel yellow"};yellow.SetTexture("_BaseMap",null);yellow.SetColor("_BaseColor",new Color(.68f,.43f,.07f));yellow.SetColor("_EmissionColor",Color.black);yellow.SetFloat("_Metallic",.35f);
            foreach(var r in GetComponentsInChildren<Renderer>())if(r.name=="Diesel crankcase"||r.name=="Alternator cover"||r.name=="Generator service fascia")r.sharedMaterial=yellow;
            if(Resources.Load<TowerWiringAsset>("TowerWiring/Layout"))return; // Full baked route replaces this older single feed.
            // Short feed up the rear of the engine, then around the wall to the separate panel.
            // One combined mesh, no light/collider, no conductor crossing the operating aisle.
            var points=new List<Vector3>{transform.TransformPoint(new Vector3(0,.45f,0)),transform.TransformPoint(new Vector3(0,2.8f,0))};
            for(int i=0;i<=12;i++){float a=Mathf.Lerp(15,90,i/12f)*Mathf.Deg2Rad;points.Add(new Vector3(Mathf.Sin(a)*3.60f,6.10f,Mathf.Cos(a)*3.60f));}
            points.Add(new Vector3(3.60f,5.05f,0));
            var combine=new List<CombineInstance>();
            for(int i=1;i<points.Count;i++){
                var o=parts.Part("Generator feed segment",transform,Vector3.zero,Vector3.one,parts.Dark,PrimitiveType.Cylinder);
                var delta=points[i]-points[i-1];var matrix=Matrix4x4.TRS((points[i]+points[i-1])*.5f,Quaternion.FromToRotation(Vector3.up,delta),new Vector3(.025f,delta.magnitude*.5f,.025f));
                combine.Add(new CombineInstance{mesh=o.GetComponent<MeshFilter>().sharedMesh,transform=matrix});o.SetActive(false);Destroy(o);
            }
            conduit=new Mesh{name="Generator to power panel feed"};conduit.CombineMeshes(combine.ToArray());
            var feed=new GameObject("Generator to POWER PANEL conduit");feed.transform.SetParent(transform,false);feed.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            feed.AddComponent<MeshFilter>().sharedMesh=conduit;feed.AddComponent<MeshRenderer>().sharedMaterial=parts.Dark;
        }
        void OnDestroy(){if(yellow)Destroy(yellow);if(conduit)Destroy(conduit);}
    }
}
