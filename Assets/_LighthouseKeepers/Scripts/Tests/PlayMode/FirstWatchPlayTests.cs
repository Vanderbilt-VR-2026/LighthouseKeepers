#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Progression;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Tests
{
    public sealed class FirstWatchPlayTests
    {
        const string Entry = "Assets/_LighthouseKeepers/Scenes/Development/LK_FirstWatch.unity";
        FirstWatchDirector watch;
        FloodController flood;
        XROrigin rig;

        [UnitySetUp] public IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(Entry,new LoadSceneParameters(LoadSceneMode.Single));
            float deadline=Time.realtimeSinceStartup+60;
            do { yield return null; watch=Object.FindAnyObjectByType<FirstWatchDirector>(); }
            while ((!watch || !watch.Ready) && Time.realtimeSinceStartup<deadline);
            Assert.IsNotNull(watch); Assert.IsTrue(watch.Ready);
            flood=Object.FindAnyObjectByType<FloodController>(); rig=Object.FindAnyObjectByType<XROrigin>();
        }
        void Place(Vector3 position)
        {
            var character=rig.GetComponent<CharacterController>(); character.enabled=false;
            rig.transform.position=position; Physics.SyncTransforms(); character.enabled=true;
        }
        [UnityTest] public IEnumerator AuthoredSliceRepairsBuysTimeAndRecoversAtLanding()
        {
            Assert.AreEqual(7,SceneManager.sceneCount);
            Assert.AreEqual(1,Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(4,Object.FindObjectsByType<FirstWatchValve>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(3,Object.FindObjectsByType<FirstWatchBoard>(FindObjectsSortMode.None).Length);
            Assert.IsTrue(flood.Paused);
            watch.BeginWatch(); Assert.IsFalse(flood.Paused);
            // A distant ray/keyboard action cannot repair from the safe upper floor.
            Assert.AreEqual(ValveResult.Ignored,watch.TurnValve(watch.Rules.NextValve,270));
            Place(new Vector3(0,.05f,-1.85f)); yield return null;
            Assert.IsTrue(watch.AtStation);
            Assert.AreEqual(ValveResult.Ignored,watch.TurnValve(watch.Rules.NextValve,270));
            watch.LatchClamp(); Assert.IsTrue(watch.Braced);
            flood.SetHeight(.8f);
            var leak = GameObject.Find("Active pipe leak");
            Assert.IsNotNull(leak);
            float before=flood.Height, speed=flood.RiseSpeed;
            for(int i=0;i<3;i++) watch.TurnValve(watch.Rules.NextValve,270);
            Assert.AreEqual(WatchPhase.Escape,watch.Rules.Phase);
            Assert.IsFalse(leak.activeSelf, "Repair should visibly stop the pipe spray.");
            Assert.Less(flood.Height,before-.5f); Assert.Less(flood.RiseSpeed,speed);
            Place(new Vector3(2.7f,3.25f,0)); yield return null; yield return null;
            Assert.AreEqual(WatchPhase.Recovery,watch.Rules.Phase); Assert.IsTrue(flood.Paused);
            yield return new WaitForSeconds(FirstWatchRules.RecoverySeconds + .2f);
            Assert.AreEqual(WatchPhase.Complete,watch.Rules.Phase);
            watch.RestartWatch();
            Assert.AreEqual(WatchPhase.Briefing,watch.Rules.Phase); Assert.IsTrue(flood.Paused);
            Assert.That(flood.Height,Is.EqualTo(flood.MinimumHeight).Within(.001f));
            Assert.That(rig.transform.position.y,Is.LessThan(.2f));
            Assert.IsTrue(leak.activeSelf, "Retry must restore the damaged pipe.");
        }

        [UnityTest] public IEnumerator WrongValveSurgeIsBoundedAndRetryWorksAfterDrowning()
        {
            watch.BeginWatch(); Place(new Vector3(0,.05f,-1.85f)); yield return null;
            watch.LatchClamp(); var before=flood.Height;
            int wrong=(watch.Rules.NextValve+1)%3;
            Assert.AreEqual(ValveResult.WrongValve,watch.TurnValve(wrong,30));
            for(int i=0;i<100;i++) watch.TurnValve(wrong,30);
            Assert.That(flood.Height-before,Is.EqualTo(.08f).Within(.001f));
            Assert.AreEqual(1,watch.Rules.Mistakes);
            flood.SetHeight(rig.Camera.transform.position.y+.1f);
            yield return new WaitForSeconds(FirstWatchRules.AirSeconds + .2f);
            Assert.AreEqual(WatchPhase.Failed,watch.Rules.Phase);
            Assert.IsTrue(flood.Paused);
            watch.RestartWatch(); Assert.AreEqual(0,watch.Rules.Mistakes); Assert.AreEqual(0,watch.ClampRemaining);
            yield return null;
        }

        [UnityTest] public IEnumerator SelectedBraceAndWristRotationActuallySealAValve()
        {
            watch.BeginWatch(); Place(new Vector3(0,.05f,-1.85f)); yield return null;
            var brace=GameObject.Find("Hold brass brace").GetComponent<XRSimpleInteractable>();
            var valve=GameObject.Find("Seal valve "+FirstWatchDirector.Letter(watch.Rules.NextValve)).GetComponent<XRSimpleInteractable>();
            var manager=Object.FindAnyObjectByType<XRInteractionManager>();
            XRSocketInteractor Hand(string name, Vector3 position)
            {
                var go=new GameObject(name);go.transform.position=position;
                var hit=go.AddComponent<SphereCollider>();hit.radius=.1f;hit.isTrigger=true;
                var body=go.AddComponent<Rigidbody>();body.isKinematic=true;
                var hand=go.AddComponent<XRSocketInteractor>();hand.showInteractableHoverMeshes=false;return hand;
            }
            var left=Hand("Test brace hand",brace.transform.position);
            var right=Hand("Test turning hand",valve.transform.position);
            try
            {
                yield return null;
                manager.SelectEnter((IXRSelectInteractor)left,(IXRSelectInteractable)brace);
                manager.SelectEnter((IXRSelectInteractor)right,(IXRSelectInteractable)valve);
                yield return null;
                Assert.IsTrue(watch.Braced,"Selected nearby brace must support the repair without the clamp.");
                for(int i=0;i<24;i++){right.transform.Rotate(0,0,15,Space.World);yield return null;}
                Assert.AreEqual(1,watch.Rules.SealedCount,"Tracked wrist rotation must reach the shared repair rules.");
                left.transform.position+=Vector3.up*1.5f;
                Assert.IsFalse(watch.Braced,"A selected hand carried away from the brace must not keep bracing.");
            }
            finally {Object.Destroy(left.gameObject);Object.Destroy(right.gameObject);}
        }
    }
}
#endif
