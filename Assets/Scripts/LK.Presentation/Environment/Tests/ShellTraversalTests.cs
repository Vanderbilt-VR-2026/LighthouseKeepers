#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
namespace LighthouseKeepers.Presentation.Environment.Tests
{
    public sealed class ShellTraversalTests
    {
        float previousDelta;
        XRInputValueReader<Vector2> input;
        XROrigin rig;
        [UnityTest] public IEnumerator ExistingRigWalksAuthoredHouseToGalleryRoute()
        {
            previousDelta=Time.captureDeltaTime;Time.captureDeltaTime=1f/72f;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(int i=0;i<10;i++)yield return null;
            rig=UnityEngine.Object.FindFirstObjectByType<XROrigin>();Assert.That(rig,Is.Not.Null);
            foreach(var c in rig.GetComponents<MonoBehaviour>())if(c.GetType().Name=="DesktopPreview")c.enabled=false;
            input=new XRInputValueReader<Vector2>("Environment traversal regression",XRInputValueReader.InputSourceMode.ManualValue);
            rig.GetComponentInChildren<ContinuousMoveProvider>().leftHandMoveInput=input;
            for(int i=0;i<72;i++)yield return null;
            Assert.That(rig.GetComponentInChildren<GravityProvider>().isGrounded,Is.True,"Rig must recognize new shell floor collision.");
            // Test-only reflection reads the editor's parsed source data without adding
            // an Editor assembly dependency to player presentation code.
            var builder=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.Editor.Environment.LighthouseBuilder")).First(t=>t!=null);
            var layout=builder.GetProperty("Layout").GetValue(null);
            var routes=(Array)layout.GetType().GetField("routes").GetValue(layout);
            var paths=new Dictionary<string,Vector3[]>();
            foreach(var route in routes){var t=route.GetType();paths.Add((string)t.GetField("id").GetValue(route),(Vector3[])t.GetField("points").GetValue(route));}
            foreach(var id in new[]{"RT-HOUSE-MAIN","RT-F1-ENTRY","RT-F1_F2","RT-F2-TRANSFER","RT-F2_F3","RT-F3-TRANSFER","RT-F3_F4","RT-F4-GALLERY"})
                foreach(var point in paths[id])
                {
                    yield return WalkTo(point,id);
                    Assert.That(rig.transform.position.y,Is.EqualTo(point.y).Within(.25f),id+" vertical alignment at "+point);
                }
            input.manualValue=Vector2.zero;var stopped=rig.transform.position;
            for(int i=0;i<36;i++)yield return null;
            Assert.That(Vector3.Distance(stopped,rig.transform.position),Is.LessThan(.04f));
        }
        IEnumerator WalkTo(Vector3 target,string route)
        {
            for(int i=0;i<720;i++)
            {
                var delta=Vector3.ProjectOnPlane(target-rig.transform.position,Vector3.up);
                if(delta.magnitude<.07f){input.manualValue=Vector2.zero;yield return null;yield break;}
                var forward=Vector3.ProjectOnPlane(rig.Camera.transform.forward,Vector3.up).normalized;
                var right=Vector3.Cross(Vector3.up,forward);delta.Normalize();
                input.manualValue=new Vector2(Vector3.Dot(delta,right),Vector3.Dot(delta,forward));yield return null;
            }
            Assert.Fail(route+" stuck at "+rig.transform.position+" targeting "+target);
        }
        [TearDown] public void Restore(){if(input!=null)input.manualValue=Vector2.zero;Time.captureDeltaTime=previousDelta;}
    }
}
#endif
