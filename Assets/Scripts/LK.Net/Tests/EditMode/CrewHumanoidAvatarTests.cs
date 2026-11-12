using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LighthouseKeepers.Net.Tests
{
    /// <summary>Unity integration checks. Portable IK tests do not replace running these in the Editor.</summary>
    public sealed class CrewHumanoidAvatarTests
    {
        GameObject model,floor;
        CrewHumanoidAvatar avatar;
        static readonly Vector3 Location=new Vector3(100,0,100);
        [SetUp] public void SetUp()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Avatars/PF_RiggedFisherman.prefab");
            Assert.NotNull(prefab);model=Object.Instantiate(prefab);avatar=model.GetComponent<CrewHumanoidAvatar>();
            var adapter=model.GetComponent<CrewAvatarModel>();Assert.IsTrue(adapter.TryValidate(out var error),error);
            Assert.IsTrue(adapter.PrepareVisualOnly(out error),error);
            floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=Location+Vector3.down*.05f;
            floor.transform.localScale=new Vector3(10,.1f,10);Physics.SyncTransforms();
        }
        [TearDown] public void TearDown(){if(model)Object.DestroyImmediate(model);if(floor)Object.DestroyImmediate(floor);}
        static KeeperPoseFrame Pose(float crouch=0,Vector3 move=default)
        {
            var offset=Location+move;
            return new KeeperPoseFrame{Actor=1,Sequence=1,Head=offset+Vector3.up*(1.665f-crouch),HeadRotation=Quaternion.identity,
                Left=offset+new Vector3(-.28f,1.15f-crouch,.25f),Right=offset+new Vector3(.28f,1.15f-crouch,.25f),
                LeftRotation=Quaternion.identity,RightRotation=Quaternion.identity};
        }
        [Test] public void SkinBuildsOnceWithAllBonesAndNoGameplayOrTrackingComponents()
        {
            Assert.NotNull(avatar.Skin);Assert.AreEqual(51,avatar.Skin.bones.Length);
            Assert.That(avatar.Skin.sharedMesh.vertexCount,Is.InRange(19000,22000));
            Assert.AreEqual(26496,avatar.Skin.sharedMesh.GetIndexCount(0)/3);
            var mesh=avatar.Skin.sharedMesh;Assert.IsTrue(avatar.TryInitialize(out var error),error);
            Assert.AreSame(mesh,avatar.Skin.sharedMesh);Assert.AreEqual(1,model.GetComponentsInChildren<SkinnedMeshRenderer>().Length);
            Assert.IsEmpty(model.GetComponentsInChildren<Collider>());Assert.IsEmpty(model.GetComponentsInChildren<Camera>());
            Assert.IsEmpty(model.GetComponentsInChildren<AudioListener>());Assert.IsEmpty(model.GetComponentsInChildren<Animator>());
        }
        [Test] public void EyesAndReachableWristsFollowExistingTrackingIncludingHeadTilt()
        {
            var pose=Pose();pose.HeadRotation=Quaternion.Euler(20,35,10);avatar.ApplyPose(pose,1f/60);
            Assert.Less(Vector3.Distance(avatar.EyePosition,pose.Head),.001f);
            Assert.Less(Vector3.Distance(avatar.Joint("LeftHand").position,pose.Left),.001f);
            Assert.Less(Vector3.Distance(avatar.Joint("RightHand").position,pose.Right),.001f);
        }
        [Test] public void CrouchingLowersHipsBendsKneesAndKeepsBootsOnTheFloor()
        {
            avatar.ApplyPose(Pose(),1f/60);float standing=avatar.Joint("Hips").position.y;
            var foot=avatar.Joint("LeftFoot").position;avatar.ApplyPose(Pose(.5f),1f/60);
            Assert.That(avatar.Joint("Hips").position.y,Is.EqualTo(standing-.5f).Within(.005f));
            Assert.Less(Vector3.Distance(foot,avatar.Joint("LeftFoot").position),.005f);
            Assert.Greater(avatar.Joint("LeftLowerLeg").position.z,Location.z+.1f);
            Assert.Less(Vector3.Distance(avatar.EyePosition,Pose(.5f).Head),.001f);
        }
        [Test] public void GripCurlsFingersAndOpenInputRestoresThem()
        {
            var pose=Pose();avatar.ApplyPose(pose,1f/60);var open=avatar.Joint("LeftIndexIntermediate").rotation;
            pose.LeftGrip=pose.LeftTrigger=1;avatar.ApplyPose(pose,1f/60);
            Assert.Greater(Quaternion.Angle(open,avatar.Joint("LeftIndexIntermediate").rotation),45);
            pose.LeftGrip=pose.LeftTrigger=0;avatar.ApplyPose(pose,1f/60);
            Assert.Less(Quaternion.Angle(open,avatar.Joint("LeftIndexIntermediate").rotation),.01f);
        }
        [Test] public void DiscreteFistPoseAlsoCurlsTheIndexWithoutAnalogTriggerInput()
        {
            var pose=Pose();avatar.ApplyPose(pose,1f/60);var open=avatar.Joint("LeftIndexIntermediate").rotation;
            pose.LeftPose=1;avatar.ApplyPose(pose,1f/60);
            Assert.Greater(Quaternion.Angle(open,avatar.Joint("LeftIndexIntermediate").rotation),45);
        }
        [Test] public void AStaticStepRaisesTheBootsEvenWhenALowerFloorExistsUnderIt()
        {
            avatar.ApplyPose(Pose(),1f/60);
            var step=GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                step.transform.position=Location+new Vector3(0,.1f,.5f);step.transform.localScale=new Vector3(2,.2f,.5f);Physics.SyncTransforms();
                for(int i=1;i<=60;i++)avatar.ApplyPose(Pose(0,new Vector3(0,.2f,.5f)),1f/60);
                Assert.That(avatar.Joint("LeftFoot").position.y,Is.EqualTo(.3375f).Within(.01f));
                Assert.That(avatar.Joint("RightFoot").position.y,Is.EqualTo(.3375f).Within(.01f));
            }
            finally{Object.DestroyImmediate(step);}
        }
        [Test] public void WalkingLiftsFeetAndATeleportResetsThePlantedStance()
        {
            avatar.ApplyPose(Pose(),1f/60);float baseY=avatar.Joint("LeftFoot").position.y,maxY=baseY;
            for(int i=1;i<=60;i++)
            {
                avatar.ApplyPose(Pose(0,Vector3.forward*(i*.01f)),1f/60);
                maxY=Mathf.Max(maxY,avatar.Joint("LeftFoot").position.y,avatar.Joint("RightFoot").position.y);
            }
            Assert.Greater(maxY,baseY+.025f);
            var teleported=Pose(0,Vector3.right*3);avatar.ApplyPose(teleported,1f/60);
            Assert.That(avatar.Joint("LeftFoot").position.x,Is.EqualTo(Location.x+2.9f).Within(.01f));
            Assert.That(avatar.Joint("LeftFoot").position.y,Is.EqualTo(baseY).Within(.005f));
        }
        [Test] public void CorruptTrackingCannotMoveTheBodyOrProduceNanBones()
        {
            avatar.ApplyPose(Pose(),1f/60);var before=avatar.Joint("Head").position;
            var pose=Pose();pose.Head.x=float.NaN;avatar.ApplyPose(pose,1f/60);
            Assert.AreEqual(before,avatar.Joint("Head").position);
        }
    }
}
