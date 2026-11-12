using System;
using System.Collections.Generic;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    /// <summary>Versioned, baked skin data exported by Tools/Avatars/export_fisherman_skin.py.</summary>
    [Serializable]
    internal sealed class CrewHumanoidRig
    {
        [Serializable] public sealed class Joint
        {
            public string name;
            public int parent;
            public Vector3 position, curlAxis;
            public Quaternion rotation;
        }
        public int version;
        public Vector3 eyeReference;
        public float floorHeight;
        public Joint[] joints;
        public float[] vertices, normals, tangents, uv, boneWeights;
        public int[] boneIndices, triangles;
        public Dictionary<string,int> Indices {get;private set;}
        static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
        static bool Finite(float[] values)
        {foreach(float value in values)if(!float.IsFinite(value))return false;return true;}

        public static bool TryRead(TextAsset source,out CrewHumanoidRig rig,out string error)
        {
            rig=null;error="Assign a supported baked humanoid rig TextAsset.";
            if(!source)return false;
            try
            {
                var data=JsonUtility.FromJson<CrewHumanoidRig>(source.text);
                if(data==null||data.version!=1||data.joints==null||data.joints.Length<21||data.joints.Length>100||
                   data.vertices==null||data.vertices.Length%3!=0||data.vertices.Length<9||data.vertices.Length>196605||
                   data.normals==null||data.tangents==null||data.uv==null||data.boneWeights==null||data.boneIndices==null||data.triangles==null)
                    return false;
                int count=data.vertices.Length/3;
                if(data.normals.Length!=count*3||data.tangents.Length!=count*4||data.uv.Length!=count*2||
                   data.boneWeights.Length!=count*4||data.boneIndices.Length!=count*4||data.triangles.Length%3!=0||data.triangles.Length==0||
                   !Finite(data.eyeReference)||!float.IsFinite(data.floorHeight)||data.eyeReference.y-data.floorHeight<.5f||
                   !Finite(data.vertices)||!Finite(data.normals)||!Finite(data.tangents)||!Finite(data.uv)||!Finite(data.boneWeights))return false;
                data.Indices=new Dictionary<string,int>(data.joints.Length);
                for(int i=0;i<data.joints.Length;i++)
                {
                    var joint=data.joints[i];
                    if(joint==null||(i==0?(joint.parent!=-1||joint.name!="Hips"):joint.parent<0)||string.IsNullOrEmpty(joint.name)||data.Indices.ContainsKey(joint.name)||joint.parent < -1||joint.parent>=i||
                       !Finite(joint.position)||!Finite(joint.curlAxis)||!float.IsFinite(joint.rotation.x)||!float.IsFinite(joint.rotation.y)||
                       !float.IsFinite(joint.rotation.z)||!float.IsFinite(joint.rotation.w))return false;
                    float n=Quaternion.Dot(joint.rotation,joint.rotation);
                    if(n<.99f||n>1.01f)return false;
                    data.Indices.Add(joint.name,i);
                }
                foreach(string name in new[]{"Hips","Spine","Chest","Neck","Head","LeftShoulder","RightShoulder",
                         "LeftUpperArm","LeftLowerArm","LeftHand","RightUpperArm","RightLowerArm","RightHand",
                         "LeftUpperLeg","LeftLowerLeg","LeftFoot","RightUpperLeg","RightLowerLeg","RightFoot","LeftToes","RightToes"})
                    if(!data.Indices.ContainsKey(name)){error=$"Baked humanoid rig is missing {name}.";return false;}
                foreach(var chain in new[]{new[]{"Neck","Head"},new[]{"LeftUpperArm","LeftLowerArm","LeftHand"},
                    new[]{"RightUpperArm","RightLowerArm","RightHand"},new[]{"LeftUpperLeg","LeftLowerLeg","LeftFoot"},
                    new[]{"RightUpperLeg","RightLowerLeg","RightFoot"}})
                    for(int i=1;i<chain.Length;i++)
                        if(Vector3.Distance(data.joints[data.Indices[chain[i-1]]].position,data.joints[data.Indices[chain[i]]].position)<.001f)
                        {error="Baked humanoid limb lengths must be positive.";return false;}
                for(int v=0;v<count;v++)
                {
                    float sum=0;
                    for(int k=v*4;k<v*4+4;k++)
                    {
                        if(data.boneIndices[k]<0||data.boneIndices[k]>=data.joints.Length||data.boneWeights[k]<0)return false;
                        sum+=data.boneWeights[k];
                    }
                    if(Mathf.Abs(sum-1)>.001f)return false;
                }
                foreach(int index in data.triangles)if(index<0||index>=count)return false;
                rig=data;error=null;return true;
            }
            catch(ArgumentException exception){error="Invalid humanoid rig JSON: "+exception.Message;return false;}
        }

        public Mesh BuildMesh()
        {
            int count=vertices.Length/3;
            var positions=new Vector3[count];var ns=new Vector3[count];var ts=new Vector4[count];
            var uvs=new Vector2[count];var weights=new BoneWeight[count];
            for(int i=0;i<count;i++)
            {
                int a=i*3,b=i*4;
                positions[i]=new Vector3(vertices[a],vertices[a+1],vertices[a+2]);
                ns[i]=new Vector3(normals[a],normals[a+1],normals[a+2]);
                ts[i]=new Vector4(tangents[b],tangents[b+1],tangents[b+2],tangents[b+3]);uvs[i]=new Vector2(uv[i*2],uv[i*2+1]);
                weights[i]=new BoneWeight{boneIndex0=boneIndices[b],boneIndex1=boneIndices[b+1],boneIndex2=boneIndices[b+2],boneIndex3=boneIndices[b+3],
                    weight0=boneWeights[b],weight1=boneWeights[b+1],weight2=boneWeights[b+2],weight3=boneWeights[b+3]};
            }
            var binds=new Matrix4x4[joints.Length];
            for(int i=0;i<binds.Length;i++)binds[i]=Matrix4x4.TRS(joints[i].position,joints[i].rotation,Vector3.one).inverse;
            var mesh=new Mesh{name="Fisherman runtime skin",vertices=positions,normals=ns,tangents=ts,uv=uvs,
                boneWeights=weights,bindposes=binds,triangles=triangles};
            mesh.RecalculateBounds();mesh.UploadMeshData(true);return mesh;
        }
    }
}
