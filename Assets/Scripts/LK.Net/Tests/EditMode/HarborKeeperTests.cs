using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class HarborKeeperTests
    {
        GameObject model,floor;CrewHumanoidAvatar avatar;
        static readonly Vector3 Location=new(100,0,100);
        [SetUp] public void Setup()
        {
            EditorUtility.audioMasterMute=true;
            var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Avatars/PF_HarborKeeper.prefab");Assert.NotNull(p);
            model=Object.Instantiate(p);avatar=model.GetComponent<CrewHumanoidAvatar>();
            Assert.IsTrue(model.GetComponent<CrewAvatarModel>().PrepareVisualOnly(out var error),error);
            floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=Location+Vector3.down*.05f;
            floor.transform.localScale=new Vector3(10,.1f,10);Physics.SyncTransforms();
        }
        [TearDown] public void Cleanup(){Object.DestroyImmediate(model);Object.DestroyImmediate(floor);}
        static KeeperPoseFrame Pose(Vector3 move=default,float yaw=0,float crouch=0)
        {
            var offset=Location+move;var q=Quaternion.Euler(0,yaw,0);
            return new KeeperPoseFrame{Actor=1,Sequence=1,Head=offset+Vector3.up*(1.665f-crouch),HeadRotation=q,
                Left=offset+q*new Vector3(-.28f,1.15f-crouch,.25f),Right=offset+q*new Vector3(.28f,1.15f-crouch,.25f),LeftRotation=q,RightRotation=q};
        }
        [Test] public void OriginalRigContractAndNoGameplayComponents()
        {
            Assert.AreEqual(56,avatar.Skin.bones.Length);Assert.AreEqual(1,avatar.Skin.sharedMaterials.Length);
            Assert.Less(avatar.Skin.sharedMesh.GetIndexCount(0)/3,25000);
            Assert.IsEmpty(model.GetComponentsInChildren<Collider>());Assert.IsEmpty(model.GetComponentsInChildren<Camera>());
            Assert.IsEmpty(model.GetComponentsInChildren<AudioListener>());
            var pose=Pose();avatar.ApplyPose(pose,1f/60);
            Assert.Less(Vector3.Distance(avatar.EyePosition,pose.Head),.001f);
            Assert.Less(Vector3.Distance(avatar.Joint("LeftHand").position,pose.Left),.001f);
        }
        [Test] public void PossessionMovesFaceAndBodyButLeavesOfferedHandsReachable()
        {
            var p=Pose();avatar.ApplyPose(p,1f/60);var eye=avatar.Joint("EyeL");var jaw=avatar.Joint("Jaw");var normal=eye.rotation;var jawRest=jaw.rotation;
            avatar.Possession=1;avatar.VisualLift=.3f;avatar.ApplyPose(p,1f/60);
            Assert.Greater(Quaternion.Angle(normal,eye.rotation),100);Assert.Greater(Quaternion.Angle(jawRest,jaw.rotation),20);
            Assert.That(avatar.EyePosition.y,Is.EqualTo(p.Head.y+.3f).Within(.001));Assert.Less(Vector3.Distance(avatar.Joint("LeftHand").position,p.Left),.001f);
            avatar.Possession=avatar.VisualLift=0;avatar.ApplyPose(p,1f/60);Assert.Less(Quaternion.Angle(normal,eye.rotation),.01f);Assert.Less(avatar.Joint("TearL").localScale.x,.002f);
        }
        [Test] public void SurfaceUsesBakedClothMapsAndAndroidAstcMipmaps()
        {
            var material=avatar.Skin.sharedMaterial;
            foreach(var entry in new[]{("_BaseMap","Albedo"),("_BumpMap","Normal"),("_MetallicGlossMap","Smoothness")})
            {
                var texture=material.GetTexture(entry.Item1);Assert.NotNull(texture);
                string path=AssetDatabase.GetAssetPath(texture);Assert.That(path,Does.Contain("HarborKeeper/Fisherman_"+entry.Item2));
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);Assert.IsTrue(importer.mipmapEnabled);
                var android=importer.GetPlatformTextureSettings("Android");Assert.IsTrue(android.overridden);
                Assert.AreEqual(entry.Item2=="Normal"?TextureImporterFormat.ASTC_4x4:TextureImporterFormat.ASTC_6x6,android.format);
            }
        }
        [Test] public void SmallLookLeavesBootsPlantedAndLargeTurnStepsRound()
        {
            avatar.ApplyPose(Pose(),1f/60);var ankle=avatar.Joint("LeftFoot");var pos=ankle.position;var rot=ankle.rotation;
            for(int i=0;i<60;i++)avatar.ApplyPose(Pose(yaw:15),1f/60);
            Assert.Less(Quaternion.Angle(rot,ankle.rotation),.1f);Assert.Less(Vector3.Distance(pos,ankle.position),.02f);
            float maxY=ankle.position.y;
            for(int i=0;i<150;i++){avatar.ApplyPose(Pose(yaw:100),1f/60);maxY=Mathf.Max(maxY,ankle.position.y);}
            Assert.Greater(maxY,pos.y+.03f);Assert.Greater(Quaternion.Angle(rot,ankle.rotation),65);
        }
        [Test] public void CrouchKeepsFeetAndBendsKneesForward()
        {
            avatar.ApplyPose(Pose(),1f/60);var foot=avatar.Joint("LeftFoot").position;float hips=avatar.Joint("Hips").position.y;
            avatar.ApplyPose(Pose(crouch:.48f),1f/60);
            Assert.Less(Vector3.Distance(foot,avatar.Joint("LeftFoot").position),.01f);
            Assert.That(avatar.Joint("Hips").position.y,Is.EqualTo(hips-.48f).Within(.01f));
            Assert.Greater(avatar.Joint("LeftLowerLeg").position.z,Location.z+.1f);
        }
        [Test] public void BootsReachRaisedLandingAndResetAfterTeleport()
        {
            avatar.ApplyPose(Pose(),1f/60);float baseY=avatar.Joint("LeftFoot").position.y;
            var step=GameObject.CreatePrimitive(PrimitiveType.Cube);
            try{
                step.transform.position=Location+new Vector3(0,.1f,.5f);step.transform.localScale=new Vector3(2,.2f,.5f);Physics.SyncTransforms();
                for(int i=0;i<90;i++)avatar.ApplyPose(Pose(new Vector3(0,.2f,.5f)),1f/60);
                Assert.That(avatar.Joint("LeftFoot").position.y,Is.EqualTo(baseY+.2f).Within(.015f));
                avatar.ApplyPose(Pose(Vector3.right*3),1f/60);
                Assert.That(avatar.Joint("LeftFoot").position.x,Is.EqualTo(Location.x+2.9f).Within(.02f));
            }finally{Object.DestroyImmediate(step);}
        }
        [Test] public void LegacyChestProxyCannotShowThroughTheKeeper()
        {
            var player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_LighthouseKeepers/Prefabs/Player/LK_QuestPlayer.prefab");
            var proxy=player.transform.Find("Stabilized chest proxy");Assert.NotNull(proxy);
            Assert.IsFalse(proxy.GetComponent<Renderer>().enabled);
        }
        [Test] public void FirstPersonSkinHasNoFaceOrGlovesAndBothNetworkRolesUseNewModel()
        {
            var local=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Avatars/Resources/PF_HarborKeeperBody.prefab");
            Assert.NotNull(local);var go=Object.Instantiate(local);
            try{
                var body=go.GetComponent<CrewHumanoidAvatar>();Assert.IsTrue(body.TryInitialize(out var error),error);
                foreach(var v in body.Skin.sharedMesh.vertices)Assert.LessOrEqual(v.y,1.451f);
                Assert.Less(body.Skin.sharedMesh.vertexCount,avatar.Skin.sharedMesh.vertexCount);
                Assert.IsEmpty(go.GetComponentsInChildren<Collider>());
            }finally{Object.DestroyImmediate(go);}
            var player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Network/Resources/PF_PlayerRig.prefab");
            var so=new SerializedObject(player.GetComponent<CrewAvatarView>());
            foreach(var name in new[]{"captainModel","fishermanModel"})
                Assert.AreEqual("PF_HarborKeeper",so.FindProperty(name).objectReferenceValue.name);
        }
    }
}
