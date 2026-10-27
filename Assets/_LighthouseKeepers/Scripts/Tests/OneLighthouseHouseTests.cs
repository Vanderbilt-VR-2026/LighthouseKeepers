using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;
namespace LighthouseKeepers.Tests
{
    public sealed class OneLighthouseHouseTests
    {
        SceneSetup[] previous;
        [SetUp] public void Open(){previous=EditorSceneManager.GetSceneManagerSetup();EditorSceneManager.OpenScene("Assets/Scenes/Lab/LK_Lab_Environment.unity");Physics.SyncTransforms();}
        [TearDown] public void Close(){if(previous.Any(s=>s.isLoaded)&&previous.Count(s=>s.isActive)==1)EditorSceneManager.RestoreSceneManagerSetup(previous);else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);}
        [Test] public void BoardMountSurvivesReauthoringWithNormalAndWallGap()
        {
            CheckBoard();
            var builder=Type.GetType("LighthouseKeepers.WatchDemo.Editor.WatchHouseAuthoring, Assembly-CSharp-Editor",true);
            builder.GetMethod("MountBoard").Invoke(null,null);Physics.SyncTransforms();CheckBoard();
        }
        static void CheckBoard()
        {
            var wall=GameObject.Find("UCX_SM_Wall_House_Side_N1_01").GetComponent<Collider>();
            var board=GameObject.Find("House wall briefing board").transform;
            Assert.Less(Vector3.Angle(board.forward,Vector3.right),2);
            Assert.That(board.position.y,Is.EqualTo(1.5f).Within(.001f));
            var back=board.TransformPoint(new Vector3(0,0,-.05f));
            Assert.That(Vector3.Distance(wall.ClosestPoint(back),back),Is.InRange(.009f,.03f));
            var rig=Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var horizontal=Vector3.ProjectOnPlane(rig.transform.position-board.position,Vector3.up);
            Assert.That(horizontal.magnitude,Is.InRange(2.4f,2.7f));
            var ink=GameObject.Find("Watch checklist").transform;
            Assert.Greater(Vector3.Dot(-ink.forward,board.forward),.999f,"Text readable side faces room");
        }
        [Test] public void HouseHasWoodBenchAndSingleMeshWrenchRestingAtColliderContact()
        {
            Assert.IsNull(GameObject.Find("PF_Shell_ScaleReference"));
            var top=GameObject.Find("Wrench table top");Assert.IsNotNull(top.GetComponent<Renderer>().sharedMaterial.GetTexture("_BaseMap"));
            var tool=GameObject.Find("Grab wrench — grip to hold");
            var meshes=tool.GetComponentsInChildren<MeshFilter>();Assert.AreEqual(1,meshes.Length);
            Assert.That(meshes[0].sharedMesh.bounds.size.z,Is.EqualTo(.25f).Within(.001f));
            float bottom=tool.GetComponents<Collider>().Min(c=>c.bounds.min.y);
            Assert.That(bottom-top.GetComponent<Collider>().bounds.max.y,Is.InRange(0f,.003f));
            Assert.IsTrue(tool.GetComponent<Rigidbody>().isKinematic);
            Assert.IsNotNull(tool.GetComponent("WatchWrenchReturn"));
            var left=tool.transform.Find("Handle centre — left");var right=tool.transform.Find("Handle centre — right");
            Assert.Less(Vector3.Distance(left.localPosition,right.localPosition),.001f);
            Assert.Less(Vector3.Dot(left.forward,right.forward),-.9f);
        }
        [Test] public void CameraHasOnlyCleanReadoutAndInactiveSleepFadeWithoutNoiseVolumes()
        {
            var hud=Camera.main.transform.Find("Watch camera HUD");
            CollectionAssert.AreEquivalent(new[]{"Watch readout","Sleep fade only"},hud.Cast<Transform>().Select(t=>t.name));
            Assert.IsFalse(hud.Find("Sleep fade only").GetComponent<Renderer>().enabled);
            Assert.IsNull(Camera.main.transform.Find("Camera recording overlay"));
            foreach(var id in AssetDatabase.FindAssets("t:VolumeProfile",new[]{"Assets"}))
                Assert.IsFalse(AssetDatabase.LoadAssetAtPath<VolumeProfile>(AssetDatabase.GUIDToAssetPath(id)).components.Any(c=>c is FilmGrain));
        }
    }
}
