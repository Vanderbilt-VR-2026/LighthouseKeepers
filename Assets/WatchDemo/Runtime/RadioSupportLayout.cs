using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace LighthouseKeepers.WatchDemo
{
    // Repeatable correction for the old combined console mesh. Frame supports stay entirely
    // inside the lantern's convex glazing, joining actual posts instead of piercing a pane.
    public sealed class RadioSupportLayout : MonoBehaviour
    {
        readonly List<Mesh> owned=new();
        public int RemovedBracketTriangles {get;private set;}
        public static void Apply(WatchRadioConsole radio)
        {
            if(!radio||radio.GetComponent<RadioSupportLayout>())return;
            radio.gameObject.AddComponent<RadioSupportLayout>().Build(radio);
        }
        static bool LegacyBracket(Vector3 v)=>Mathf.Abs(v.x)>=.829f&&Mathf.Abs(v.x)<=.931f&&
            (v.y>=.769f&&v.y<=.891f||v.y>=1.949f&&v.y<=2.071f)&&v.z>=-.601f&&v.z<=.101f;
        void Build(WatchRadioConsole radio)
        {
            foreach(var f in radio.GetComponentsInChildren<MeshFilter>()){
                if(!f.name.Contains("ConsoleBody")||!f.sharedMesh||!f.sharedMesh.isReadable)continue;
                var vertices=f.sharedMesh.vertices;var indices=f.sharedMesh.triangles;var keep=new List<int>(indices.Length);
                for(int i=0;i<indices.Length;i+=3){
                    if(LegacyBracket(vertices[indices[i]])&&LegacyBracket(vertices[indices[i+1]])&&LegacyBracket(vertices[indices[i+2]])){RemovedBracketTriangles++;continue;}
                    keep.Add(indices[i]);keep.Add(indices[i+1]);keep.Add(indices[i+2]);
                }
                if(keep.Count==indices.Length)continue;
                var mesh=Instantiate(f.sharedMesh);mesh.name="Console without penetrating brackets";mesh.triangles=keep.ToArray();mesh.RecalculateBounds();owned.Add(mesh);f.sharedMesh=mesh;
            }
            // The left rear cabinet corner also crossed the angled pane. Move the whole set,
            // including controls, microphone, and their reach volumes, together toward the room.
            transform.position+=transform.forward*.30f;
            CodeSource.Active?.RelocateLocal(transform.TransformPoint(new Vector3(0,.43f,.35f)));
            var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();var combined=new WorkshopMesh();
            var frames=new List<Renderer>();foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                if(r.enabled&&r.name.StartsWith("SM_Frame_Lantern_")&&r.bounds.size.y>2)frames.Add(r);
            foreach(float x in new[]{-.88f,.88f})foreach(float y in new[]{.83f,2.01f}){
                var start=transform.TransformPoint(new Vector3(x,y,-.025f));Renderer nearest=null;float best=float.PositiveInfinity;
                foreach(var frame in frames){var v=frame.bounds.center;v.y=start.y;float d=(v-start).sqrMagnitude;if(d<best){best=d;nearest=frame;}}
                if(!nearest){Debug.LogError("Radio has no lantern-frame support");continue;}
                var end=nearest.bounds.center;end.y=0;end-=end.normalized*.075f;end.y=start.y;
                combined.Tube(transform.InverseTransformPoint(start),transform.InverseTransformPoint(end),.028f,6);
                parts.Part("Radio frame clamp",transform,transform.InverseTransformPoint(end),new Vector3(.11f,.15f,.07f),parts.Metal);
            }
            var meshRoot=new GameObject("Radio frame support arms");meshRoot.transform.SetParent(transform,false);var support=combined.Finish("Four frame-mounted radio supports");owned.Add(support);meshRoot.AddComponent<MeshFilter>().sharedMesh=support;var renderer=meshRoot.AddComponent<MeshRenderer>();renderer.sharedMaterial=parts.Metal;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
        void OnDestroy(){foreach(var m in owned)if(m)Destroy(m);}
    }
}
