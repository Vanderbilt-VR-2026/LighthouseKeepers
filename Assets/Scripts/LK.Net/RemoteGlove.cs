using LighthouseKeepers.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
namespace LighthouseKeepers.Net
{
    /// <summary>Renders A's exact skin payload without instantiating its tracked input/interactor component.</summary>
    public sealed class RemoteGlove : MonoBehaviour
    {
        HandRig.Model model;
        Transform[] bones;
        Mesh mesh;
        public bool Prepare(bool left)
        {
            var payload=Resources.Load<TextAsset>(left?"Hands/Glove_L":"Hands/Glove_R");
            var material=Resources.Load<Material>("Hands/M_Glove");
            if(!payload || !material){Debug.LogError("Remote keeper needs A's glove payload and M_Glove Resources.",this);return false;}
            model=JsonUtility.FromJson<HandRig.Model>(payload.text);bones=new Transform[model.bones.Length];
            var bind=new Matrix4x4[bones.Length];
            for(int i=0;i<bones.Length;i++)
            {bones[i]=new GameObject(model.bones[i].name).transform;bones[i].SetParent(model.bones[i].parent<0?transform:bones[model.bones[i].parent],false);bones[i].localPosition=model.bones[i].position;bind[i]=bones[i].worldToLocalMatrix*transform.localToWorldMatrix;}
            var weights=new BoneWeight[model.vertices.Length];
            for(int i=0;i<weights.Length;i++)weights[i]=new BoneWeight{boneIndex0=model.boneIndices[i*2],boneIndex1=model.boneIndices[i*2+1],weight0=model.boneWeights[i],weight1=1-model.boneWeights[i]};
            mesh=new Mesh{name="Authored remote glove",vertices=model.vertices,normals=model.normals,triangles=model.triangles,boneWeights=weights,bindposes=bind};mesh.RecalculateBounds();
            var skin=gameObject.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=mesh;skin.bones=bones;skin.rootBone=bones[0];skin.sharedMaterial=material;
            skin.localBounds=new Bounds(Vector3.zero,Vector3.one*.4f);skin.shadowCastingMode=ShadowCastingMode.Off;return true;
        }
        public void Apply(byte pose,float grip,float trigger)
        {
            if(model==null)return;
            int index=pose==4?2:pose;
            for(int i=0;i<bones.Length;i++)
            {
                var r=index==0?Quaternion.Slerp(model.poses[0].rotations[i],model.poses[1].rotations[i],grip):model.poses[index].rotations[i];
                if(index==0)r=Quaternion.Slerp(r,model.poses[3].rotations[i],trigger*(1-grip));
                bones[i].localRotation=Quaternion.Slerp(bones[i].localRotation,r,1-Mathf.Exp(-30*Time.unscaledDeltaTime));
            }
        }
        void OnDestroy(){if(mesh)Destroy(mesh);}
    }
}
