using System.Collections.Generic;
using UnityEngine;using UnityEngine.Rendering;
namespace LighthouseKeepers.WatchDemo
{
    // One inexpensive skinned body. A real joint hierarchy gives the wet gait weight; no camera motion.
    public sealed class WalterRig:MonoBehaviour
    {
        SkinnedMeshRenderer skin;Mesh mesh;Material material;MaterialPropertyBlock block;
        Transform hips,spine,head;readonly Transform[] thighs=new Transform[2],knees=new Transform[2],arms=new Transform[2],elbows=new Transform[2];
        readonly List<Transform> bones=new();float stride;
        public int Triangles=>mesh?mesh.triangles.Length/3:0;
        public static WalterRig Create(Transform parent){var g=new GameObject("Walter — single patched oilskin rig");g.transform.SetParent(parent,false);var r=g.AddComponent<WalterRig>();r.Build();return r;}
        Transform Bone(string name,Transform parent,Vector3 bind){var b=new GameObject(name).transform;b.SetParent(parent,false);b.position=transform.TransformPoint(bind);bones.Add(b);return b;}
        void Build()
        {
            hips=Bone("Hips",transform,new Vector3(0,1.02f,0));spine=Bone("Spine",hips,new Vector3(0,1.35f,-.03f));head=Bone("Neck",spine,new Vector3(0,1.9f,.08f));
            for(int i=0;i<2;i++){int side=i==0?-1:1;thighs[i]=Bone("Thigh"+i,hips,new Vector3(side*.12f,1.05f,0));knees[i]=Bone("Knee"+i,thighs[i],new Vector3(side*.16f,.58f,.09f));arms[i]=Bone("Upper arm"+i,spine,new Vector3(side*.255f,1.69f,0));elbows[i]=Bone("Elbow"+i,arms[i],new Vector3(side*.4f,1.13f,.045f));}
            mesh=DrownedForms.BuildWalter();var vertices=mesh.vertices;var weights=new BoneWeight[vertices.Length];var binds=new Matrix4x4[bones.Count];
            for(int i=0;i<binds.Length;i++)binds[i]=bones[i].worldToLocalMatrix*transform.localToWorldMatrix;
            for(int i=0;i<vertices.Length;i++){
                var v=vertices[i];int side=v.x<0?0:1;Transform a,b;float w;
                if(v.y>1.85f){a=spine;b=head;w=Mathf.InverseLerp(1.83f,1.97f,v.y);}
                else if(Mathf.Abs(v.x)>.29f&&v.y>.15f){a=arms[side];b=elbows[side];w=1-Mathf.InverseLerp(1.02f,1.25f,v.y);}
                else if(v.y<1.02f&&Mathf.Abs(v.x)>.035f){a=thighs[side];b=knees[side];w=1-Mathf.InverseLerp(.49f,.70f,v.y);}
                else {a=hips;b=spine;w=Mathf.InverseLerp(1.06f,1.5f,v.y);}
                weights[i]=new BoneWeight{boneIndex0=bones.IndexOf(a),weight0=1-w,boneIndex1=bones.IndexOf(b),weight1=w};
            }
            mesh.boneWeights=weights;mesh.bindposes=binds;skin=gameObject.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=mesh;skin.bones=bones.ToArray();skin.rootBone=hips;skin.localBounds=new Bounds(new Vector3(0,1.1f,.1f),new Vector3(1.6f,2.8f,1.5f));
            material=new Material(Resources.Load<Shader>("S5Drowned")){name="Walter wet black oilskin"};skin.sharedMaterial=material;skin.shadowCastingMode=ShadowCastingMode.Off;skin.receiveShadows=false;block=new MaterialPropertyBlock();Show(false,0,0,0);
        }
        public void Show(bool visible,float speed,float dt,float hunch)
        {
            skin.enabled=visible;if(!visible)return;stride+=speed*dt*7;
            float amount=Mathf.Clamp01(speed/1.2f),swing=Mathf.Sin(stride)*amount;
            hips.localRotation=Quaternion.Euler(0,Mathf.Sin(stride)*3*amount,Mathf.Sin(stride)*2*amount);
            spine.localRotation=Quaternion.Euler(Mathf.Lerp(5,65,hunch),0,-2);head.localRotation=Quaternion.Euler(Mathf.Lerp(4,20,hunch),Mathf.Sin(stride*.23f)*7,3);
            for(int i=0;i<2;i++){float step=swing*(i==0?1:-1);thighs[i].localRotation=Quaternion.Euler(step*22,0,0);knees[i].localRotation=Quaternion.Euler(Mathf.Max(0,-step)*32,0,0);arms[i].localRotation=Quaternion.Euler(-step*13,0,i==0?2:-2);elbows[i].localRotation=Quaternion.Euler(-8-Mathf.Max(0,step)*7,0,0);}
            // Suppress the apparition shader's arrival lunge; this is sustained physical movement.
            block.SetFloat("_Visibility",1);block.SetFloat("_Height",2.3f);block.SetFloat("_Style",0);block.SetFloat("_Arrival",10);block.SetFloat("_Breath",Time.time);skin.SetPropertyBlock(block);
        }
        void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
    }
}
