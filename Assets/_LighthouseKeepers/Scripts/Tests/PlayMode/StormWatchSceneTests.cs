#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.StormWatch;

namespace LighthouseKeepers.Tests
{
    public sealed class StormWatchSceneTests
    {
        StormWatchDirector director;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_LighthouseKeepers/Scenes/Development/LK_StormWatch.unity",new LoadSceneParameters(LoadSceneMode.Single));
            float limit=Time.realtimeSinceStartup+80;
            do{yield return null;director=Object.FindAnyObjectByType<StormWatchDirector>();}while((!director||!director.Ready)&&Time.realtimeSinceStartup<limit);
            Assert.IsNotNull(director);Assert.IsTrue(director.Ready);
        }
        [UnityTest] public IEnumerator SceneHasNightLightingAndRepairActuallyOpensStairGate()
        {
            Assert.AreEqual(7,SceneManager.sceneCount);Assert.AreEqual(1,Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(5,Object.FindObjectsByType<StormWatchBoard>(FindObjectsSortMode.None).Length);
            Assert.IsNull(GameObject.Find("Storm daylight"));
            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(light.gameObject.scene.name=="LK_Exterior")Assert.AreNotEqual(LightType.Directional,light.type);
            var gate=GameObject.Find("Route 1 / mechanical stair gate");Assert.IsNotNull(gate);Assert.IsTrue(gate.GetComponent<Collider>().enabled);
            var a=new KeeperInput{connected=true,head=WatchStations.Pipe+Vector3.up*.5f,action=WatchAction.Pipe,effort=1,command=WatchAction.Begin,sequence=1};
            var b=new KeeperInput{connected=true,head=WatchStations.Brace+Vector3.up*.5f,action=WatchAction.Brace};
            director.Tuning.pipeStepSeconds=.1f;
            for(int i=0;i<10;i++)director.Rules.Tick(.05f,a,b);
            yield return null;
            Assert.AreEqual(WatchAct.FirstRefuge,director.State.act);Assert.IsFalse(gate.activeSelf);
            Assert.IsTrue(GameObject.Find("Route 2 / mechanical stair gate").activeSelf);
            foreach(var text in Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None))
                if(text.GetComponentInParent<StormWatchBoard>()){text.ForceMeshUpdate();Assert.IsFalse(text.isTextOverflowing,text.name+": "+text.text);}
            yield return new WaitForSeconds(.2f);
            CaptureBoardsIfRequested();
        }
        [UnityTest] public IEnumerator LobbyApprovalBeginsOnlyWhenPartnerEnvironmentSendsInput()
        {
            var fake = new GameObject("Loading partner fixture").AddComponent<WatchTestLink>();
            fake.receiving = false;
            var data = new SerializedObject(director);
            data.FindProperty("link").objectReferenceValue = fake; data.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                yield return new WaitForSeconds(.2f);
                Assert.AreEqual(WatchAct.Briefing, director.State.act);
                Assert.AreEqual(0, director.State.elapsed, "Loading time must not advance the watch.");
                fake.receiving = true;
                yield return new WaitForSeconds(.2f);
                Assert.AreEqual(WatchAct.FirstWatch, director.State.act, "No second Begin action is required.");
                fake.receiving = false;
                float elapsed = director.State.elapsed;
                yield return new WaitForSeconds(.2f);
                Assert.AreEqual(elapsed, director.State.elapsed, "A stalled partner input pauses authority.");
                fake.receiving = true;
                yield return new WaitForSeconds(.2f);
                Assert.Greater(director.State.elapsed, elapsed);
            }
            finally { director.enabled = false; Object.Destroy(fake.gameObject); }
        }
        void CaptureBoardsIfRequested()
        {
            if(System.Environment.GetEnvironmentVariable("LK_CAPTURE_WATCH")!="1")return;
            string directory="Docs/Verification/StormWatch";System.IO.Directory.CreateDirectory(directory);
            var cameraObject=new GameObject("Watch verification camera");var camera=cameraObject.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.009f,.017f,.032f);camera.fieldOfView=75;camera.nearClipPlane=.05f;
            var render=new RenderTexture(1280,900,24);var pixels=new Texture2D(1280,900,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=render;
                void Capture(string name,Vector3 eye,Vector3 target)
                {
                    camera.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));camera.Render();
                    RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,1280,900),0,0);pixels.Apply();
                    System.IO.File.WriteAllBytes(directory+"/"+name+".png",pixels.EncodeToPNG());
                }
                Capture("Entry",new(-8,1.65f,.6f),new(-6.35f,1.75f,.8f));
                Capture("Plumbing",new(0,1.65f,-1.7f),new(0,1.9f,-2.9f));
                Capture("Generator",new(2.2f,4.9f,-2.85f),new(1.4f,5.1f,-4.1f));
                Capture("Radio",new(1.1f,8.1f,-2.9f),new(.65f,8.15f,-4.15f));
                Capture("Lantern",new(.9f,11.3f,2.8f),new(0,11.6f,4.35f));
                Capture("NightExterior",new(6,11.3f,0),new(16,11.3f,0));
            }
            finally{camera.targetTexture=null;RenderTexture.active=previous;render.Release();Object.Destroy(render);Object.Destroy(pixels);Object.Destroy(cameraObject);}
        }
        [UnityTest] public IEnumerator PhysicalPumpRequiresStrokesAndDisconnectOffersRestart()
        {
            var fake=new GameObject("Local transport fixture").AddComponent<WatchTestLink>();
            var data=new SerializedObject(director);data.FindProperty("link").objectReferenceValue=fake;data.ApplyModifiedPropertiesWithoutUndo();
            var state=director.Rules.State;state.act=WatchAct.PowerFailure;state.day=2;state.power=false;state.water=.8f;
            var rig=Object.FindAnyObjectByType<XROrigin>();var character=rig.GetComponent<CharacterController>();character.enabled=false;rig.transform.position=new(-2.6f,.05f,-.8f);character.enabled=true;
            var pump=GameObject.Find("PUMP / grip and stroke").GetComponent<XRSimpleInteractable>();
            var handGo=new GameObject("Pump test hand");handGo.transform.position=pump.transform.position;
            handGo.AddComponent<SphereCollider>().isTrigger=true;handGo.AddComponent<Rigidbody>().isKinematic=true;
            var hand=handGo.AddComponent<XRSocketInteractor>();hand.showInteractableHoverMeshes=false;
            try
            {
                yield return null;Object.FindAnyObjectByType<XRInteractionManager>().SelectEnter((IXRSelectInteractor)hand,(IXRSelectInteractable)pump);
                float before=state.water;yield return new WaitForSeconds(.4f);Assert.Greater(state.water,before,"A stationary grip must not pump.");
                before=state.water;
                for(int i=0;i<40;i++){handGo.transform.position=pump.transform.position+Vector3.up*Mathf.Sin(i*.6f)*.12f;yield return new WaitForSeconds(.02f);}
                Assert.Less(state.water,before,"Moving the selected handle must drain through the shared rules.");
                fake.partner=false;yield return new WaitForSeconds(.15f);
                Assert.IsTrue(director.SessionEnded);
                Assert.Less(Vector3.Distance(GameObject.Find("Watch instructions 0").transform.position,director.Head.position),3,"Disconnected keeper must be able to reach Leave Crew.");
                director.Leave();yield return new WaitForSeconds(.5f);
                Assert.AreEqual(WatchAct.Briefing,director.State.act);Assert.IsFalse(director.SessionEnded);
                Assert.That(director.Head.position.x,Is.EqualTo(-8).Within(.2f));
            }
            finally{director.enabled=false;Object.Destroy(handGo);Object.Destroy(fake.gameObject);}
        }
    }
    public sealed class WatchTestLink : WatchLink
    {
        readonly KeeperInput remote=new(){connected=true,head=new(0,4.8f,-2.5f),feet=new(0,3.2f,0)};
        public bool connected=true,partner=true,receiving=true;
        public override bool Connected=>connected;public override bool IsHost=>true;public override int LocalSlot=>0;
        public override bool PartnerConnected=>partner&&connected;public override string Status=>"Local fixture";
        public override KeeperInput RemoteInput { get { remote.connected=connected&&partner&&receiving;return remote; } }public override WatchState ReceivedState=>null;
public override void SendInput(KeeperInput input){}
        public override void Publish(WatchState state){}public override void Leave(){connected=false;partner=false;}
    }
}
#endif
