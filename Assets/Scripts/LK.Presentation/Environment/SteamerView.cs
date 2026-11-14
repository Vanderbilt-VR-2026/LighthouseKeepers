using UnityEngine;
using UnityEngine.Rendering;
namespace LighthouseKeepers.Presentation.Environment
{
    public sealed class SteamerView : MonoBehaviour
    {
        public Mesh hull,distant,beam,particles;
        public Material hullMaterial,effectsMaterial;
        Transform lightPivot;Mesh nameMesh,flareMesh;MaterialPropertyBlock effects,flareProperties;Renderer smoke,cone,flare;bool built;
        public void Initialize(string shipName)
        {
            if(built)return;built=true;gameObject.name="Steamer — "+shipName;
            var close=Renderer("Hull, deck, portholes",hull,hullMaterial,transform);
            nameMesh=SteamerGeometry.BuildName(shipName);var lettering=Renderer("Painted hull name",nameMesh,hullMaterial,transform);
            lightPivot=new GameObject("Mechanical searchlight").transform;lightPivot.SetParent(transform,false);lightPivot.localPosition=new Vector3(0,6.8f,8.5f);
            cone=Renderer("Sweeping light cone",beam,effectsMaterial,lightPivot);
            smoke=Renderer("GPU funnel smoke and bow spray",particles,effectsMaterial,transform);
            // Keep the same silhouette, deck and attached navigation lamps at every visible
            // range. The former light-cluster proxy popped into a different vessel near shore.
            // Only lettering and small effects drop out beyond useful headset resolution.
            var lod=gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.12f,new[]{close,lettering,cone,smoke}),new LOD(.001f,new[]{close})});lod.RecalculateBounds();lod.size=52;lod.localReferencePoint=new Vector3(0,4,0);
            effects=new MaterialPropertyBlock();
        }
        public void Sample(float age,float approach,float window,float depart,float storm,float sea)
        {
            if(!built)return;var pose=SteamerGeometry.Route(age,approach,window,depart,sea);
            // Vessel rolls gently; the player's camera never moves. Root route stays deterministic.
            pose.position.y+=Mathf.Sin(age*.57f)*(.03f+.16f*storm);
            transform.SetPositionAndRotation(pose.position,pose.rotation*Quaternion.Euler(Mathf.Sin(age*.31f)*.5f*storm,0,Mathf.Sin(age*.43f)*1.2f*storm));
            // Sweep across the tower during the close passage, never a player-camera effect.
            Vector3 target=new Vector3(0,11,0)-lightPivot.position;
            lightPivot.rotation=Quaternion.LookRotation(target.normalized,Vector3.up)*Quaternion.Euler(0,Mathf.Sin(age*.11f)*24,0);
            effects.SetFloat("_ShipClock",age);effects.SetFloat("_Storm",storm);smoke.SetPropertyBlock(effects);cone.SetPropertyBlock(effects);
        }
        public void Wreck(float age)
        {
            if(!flare){
                flareMesh=new Mesh{name="Distress flare card"};flareMesh.vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(1,1,0),new Vector3(-1,1,0)};
                flareMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};flareMesh.colors=new[]{Color.clear,Color.clear,Color.clear,Color.clear};flareMesh.triangles=new[]{0,1,2,0,2,3};flareMesh.RecalculateNormals();flareMesh.RecalculateBounds();
                flare=Renderer("Ship distress flare",flareMesh,effectsMaterial,transform);flareProperties=new MaterialPropertyBlock();flareProperties.SetFloat("_Flare",1);flare.SetPropertyBlock(flareProperties);
            }
            transform.position+=Vector3.down*Mathf.Min(age*.12f,1.2f);transform.rotation*=Quaternion.Euler(0,0,Mathf.Min(age,6));
            cone.enabled=false;flare.transform.position=transform.position+Vector3.up*(7+Mathf.Min(age*3,22));flare.transform.localScale=Vector3.one*1.7f;
            var camera=Camera.main;if(camera)flare.transform.rotation=camera.transform.rotation;
        }
        static MeshRenderer Renderer(string name,Mesh mesh,Material material,Transform parent)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.BlendProbes;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;return renderer;
        }
        void OnDestroy(){if(nameMesh)Destroy(nameMesh);if(flareMesh)Destroy(flareMesh);}
    }
}
