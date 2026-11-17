using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Gameplay.Interaction;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class LocalKeeperBodyTests
    {
        GameObject root,floor,presenterObject;XROrigin origin;LocalKeeperBody presenter;
        Transform head,left,right,leftWrist,rightWrist;
        static readonly Vector3 Spot=new(200,0,200);
        [UnitySetUp] public IEnumerator Setup()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.audioMasterMute=true;
#endif
            root=new GameObject("Body regression tracking origin");root.SetActive(false);root.transform.position=Spot;
            origin=root.AddComponent<XROrigin>();origin.Origin=root;
            var offset=new GameObject("Camera Offset");offset.transform.SetParent(root.transform,false);origin.CameraFloorOffsetObject=offset;
            var camera=new GameObject("Tracked body test camera").AddComponent<Camera>();camera.transform.SetParent(offset.transform,false);origin.Camera=camera;head=camera.transform;
            left=new GameObject("Left Controller").transform;left.SetParent(offset.transform,false);
            right=new GameObject("Right Controller").transform;right.SetParent(offset.transform,false);
            root.SetActive(true);
            foreach(var pair in new[]{(left,"L"),(right,"R")})
            {
                var h=Object.Instantiate(Resources.Load<GameObject>("Hands/PF_Hand_"+pair.Item2),pair.Item1).GetComponent<HandRig>();
                h.enabled=false; // Keep accepted visual bones; fixture supplies tracking, not OpenXR input.
            }
            leftWrist=left.GetComponentInChildren<SkinnedMeshRenderer>(true).bones[0];rightWrist=right.GetComponentInChildren<SkinnedMeshRenderer>(true).bones[0];
            floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=Spot+Vector3.down*.05f;floor.transform.localScale=new Vector3(20,.1f,20);
            presenterObject=new GameObject("Body regression presentation");presenter=presenterObject.AddComponent<LocalKeeperBody>();presenter.enabled=false;presenter.Bind(origin);
            Physics.SyncTransforms();yield return null;
        }
        void Pose(float height)
        {
            head.localPosition=Vector3.up*height;head.localRotation=Quaternion.identity;
            left.localPosition=new Vector3(-.24f,height-.43f,.22f);right.localPosition=new Vector3(.24f,height-.43f,.22f);
            left.localRotation=Quaternion.Euler(20,0,35);right.localRotation=Quaternion.Euler(20,0,-35);
        }
        void Settle(float height){Pose(height);for(int i=0;i<90;i++)presenter.Present(true,1f/72);}
        [UnityTearDown] public IEnumerator Teardown(){Object.Destroy(presenterObject);Object.Destroy(root);Object.Destroy(floor);yield return null;}
        [UnityTest] public IEnumerator OldSceneChestOverrideIsSuppressedAtRuntime()
        {
            var proxy=GameObject.CreatePrimitive(PrimitiveType.Cube);
            try {
                proxy.AddComponent<LighthouseKeepers.Player.BodyPresence>();yield return null;
                Assert.IsFalse(proxy.GetComponent<Renderer>().enabled);
            } finally {Object.Destroy(proxy);}
        }
        [UnityTest] public IEnumerator StartupHeightAndRecenterCannotPlantFeetInTheAir()
        {
            Pose(2.9f);presenter.Present(true,1f/72);Assert.IsFalse(presenter.Body.gameObject.activeSelf);
            Settle(1.35f);Assert.IsTrue(presenter.Body.gameObject.activeSelf);Assert.That(presenter.StandingHeight,Is.EqualTo(1.35f).Within(.01f));
            Assert.That(presenter.Body.Joint("LeftFoot").position.y,Is.InRange(.07f,.22f));
            Assert.Less(Vector3.Distance(presenter.Body.EyePosition,head.position),.005f);
            float hips=presenter.Body.Joint("Hips").position.y;
            Assert.That(hips,Is.EqualTo(.95f*1.35f/presenter.Body.ReferenceEyeHeight).Within(.04f),"Standing player was posed as crouching");
            Pose(1.02f);for(int i=0;i<250;i++)presenter.Present(true,1f/72);
            Assert.That(presenter.StandingHeight,Is.EqualTo(1.35f).Within(.01f));Assert.Less(presenter.Body.Joint("Hips").position.y,hips-.25f);
            presenter.Present(false,1f/72);Assert.IsFalse(presenter.Body.gameObject.activeSelf);
            Settle(1.35f);Assert.Less(presenter.Body.Joint("LeftFoot").position.y,.23f);
            yield return null;
        }
        [UnityTest] public IEnumerator SleevesFollowActualGloveWristsWithoutChangingHandsAcrossTurns()
        {
            foreach(float yaw in new[]{0f,120f,245f})
            {
                root.transform.SetPositionAndRotation(Spot+new Vector3(1,0,1),Quaternion.Euler(0,yaw,0));Settle(1.65f);
                var lp=leftWrist.position;var rp=rightWrist.position;var lr=leftWrist.rotation;var rr=rightWrist.rotation;
                presenter.Present(true,1f/72);
                Assert.Less(Vector3.Distance(presenter.Body.Joint("LeftHand").position,lp),.02f);
                Assert.Less(Vector3.Distance(presenter.Body.Joint("RightHand").position,rp),.02f);
                Assert.AreEqual(lp,leftWrist.position);Assert.AreEqual(rp,rightWrist.position);Assert.Less(Quaternion.Angle(lr,leftWrist.rotation),.001f);Assert.Less(Quaternion.Angle(rr,rightWrist.rotation),.001f);
                Assert.Less(presenter.Body.Joint("LeftFoot").position.y,.3f);
                var mesh=new Mesh();presenter.Body.Skin.BakeMesh(mesh);mesh.RecalculateBounds();Assert.Less(mesh.bounds.size.y,2.2f);Assert.Less(mesh.bounds.size.x,1.6f);Object.Destroy(mesh);
            }
            yield return null;
        }
    }
}
