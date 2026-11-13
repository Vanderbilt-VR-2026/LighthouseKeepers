using UnityEngine;
using UnityEngine.Rendering;
namespace LighthouseKeepers.Presentation.Environment
{
    public sealed class SteamerView : MonoBehaviour
    {
        public Mesh hull,distant,beam,particles;
        public Material hullMaterial,effectsMaterial;
        Transform lightPivot;Mesh nameMesh;MaterialPropertyBlock effects;Renderer smoke,cone;bool built;
        public void Initialize(string shipName)
        {
            if(built)return;built=true;gameObject.name="Steamer — "+shipName;
            var close=Renderer("Hull, deck, portholes",hull,hullMaterial,transform);
            nameMesh=SteamerGeometry.BuildName(shipName);var lettering=Renderer("Painted hull name",nameMesh,hullMaterial,transform);
            var far=Renderer("Distant navigation lights",distant,hullMaterial,transform);
            lightPivot=new GameObject("Mechanical searchlight").transform;lightPivot.SetParent(transform,false);lightPivot.localPosition=new Vector3(0,6.8f,8.5f);
            cone=Renderer("Sweeping light cone",beam,effectsMaterial,lightPivot);
            smoke=Renderer("GPU funnel smoke and bow spray",particles,effectsMaterial,transform);
            var lod=gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.4f,new[]{close,lettering,cone,smoke}),new LOD(.015f,new[]{far})});lod.RecalculateBounds();lod.size=52;lod.localReferencePoint=new Vector3(0,4,0);
            effects=new MaterialPropertyBlock();
        }
        public void Sample(float age,float approach,float window,float depart,float storm,float sea)
        {
            if(!built)return;var pose=SteamerGeometry.Route(age,approach,window,depart,sea);
            // Vessel rolls gently; the player's camera never moves. Root route stays deterministic.
            pose.position.y+=Mathf.Sin(age*.57f)*(.03f+.16f*storm);
            transform.SetPositionAndRotation(pose.position,pose.rotation*Quaternion.Euler(Mathf.Sin(age*.31f)*.5f*storm,0,Mathf.Sin(age*.43f)*1.2f*storm));
            lightPivot.localRotation=Quaternion.Euler(5,65+Mathf.Sin(age*.11f)*55,0);
            effects.SetFloat("_ShipClock",age);effects.SetFloat("_Storm",storm);smoke.SetPropertyBlock(effects);cone.SetPropertyBlock(effects);
        }
        static MeshRenderer Renderer(string name,Mesh mesh,Material material,Transform parent)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.BlendProbes;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;return renderer;
        }
        void OnDestroy(){if(nameMesh)Destroy(nameMesh);}
    }
}
