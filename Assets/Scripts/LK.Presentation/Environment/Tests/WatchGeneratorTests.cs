#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
namespace LighthouseKeepers.Presentation.Environment.Tests
{
    public sealed class WatchGeneratorTests
    {
        static Type Find(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        static object Get(object o,string p)=>o.GetType().GetProperty(p).GetValue(o);
        static object Call(object o,string m,params object[] args)=>o.GetType().GetMethod(m).Invoke(o,args);
        static T Field<T>(object o,string p)=>(T)o.GetType().GetField(p,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
        // Combined plumbing has an intentionally broad AABB. Visibility must test
        // triangles, not treat all the empty space inside that box as opaque metal.
        static bool SurfaceHit(Mesh mesh,Ray ray,out float distance)
        {
            distance=float.PositiveInfinity;if(!mesh.bounds.IntersectRay(ray))return false;
            var v=mesh.vertices;var tris=mesh.triangles;
            for(int n=0;n<tris.Length;n+=3){var a=v[tris[n]];var e1=v[tris[n+1]]-a;var e2=v[tris[n+2]]-a;
                var p=Vector3.Cross(ray.direction,e2);float det=Vector3.Dot(e1,p);if(Mathf.Abs(det)<.0000001f)continue;
                var t=ray.origin-a;float u=Vector3.Dot(t,p)/det;if(u<0||u>1)continue;
                var q=Vector3.Cross(t,e1);float w=Vector3.Dot(ray.direction,q)/det;if(w<0||u+w>1)continue;
                float d=Vector3.Dot(e2,q)/det;if(d>=0&&d<distance)distance=d;
            }
            return float.IsFinite(distance);
        }
        [UnityTest] public IEnumerator S4M2StarterIsVisibleFromStandingReachAndReturnsAfterPull()
        {
            string previous=System.Environment.GetEnvironmentVariable("LK_S4M2_DISABLED");
            GameObject testHand=null;
            try
            {
                System.Environment.SetEnvironmentVariable("LK_S4M2_DISABLED",null);
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity",new LoadSceneParameters(LoadSceneMode.Single));
                float deadline=Time.realtimeSinceStartup+30;
                while(!GameObject.Find("Play")&&Time.realtimeSinceStartup<deadline)yield return null;
                Find("LighthouseKeepers.Net.CrewLobbyTransport").GetField("ForceOfflineForTests").SetValue(null,true);
                GameObject.Find("Play").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                GameObject.Find("HostPlay").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                Assert.That(GameObject.Find("DevStartDay2"),Is.Not.Null);
                GameObject.Find("DevStartDay2").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                GameObject.Find("StartCrew").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                while(SceneManager.GetActiveScene().name!="LK_Lab_Environment"&&Time.realtimeSinceStartup<deadline)yield return null;
                yield return null;yield return null;
                var watch=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c=>c.GetType().Name=="WatchDemoController");
                var state=Get(watch,"State");var engine=Get(state,"Engine");Assert.That(engine,Is.Not.Null);
                // The new tutorial begins powered; test the same unprimed pull after its scheduled outage.
                if((bool)Get(state,"GeneratorTutorial")||(bool)Get(state,"Campaign")){Call(state,"Tick",100d);yield return null;}
                Assert.False((bool)Get(engine,"Running"),"Unprimed-pull fixture must start after fuel exhaustion");
                var generator=GameObject.Find("STN_GENERATOR").transform;
                var starter=GameObject.Find("GRIP_StarterHandle").GetComponents<MonoBehaviour>().Single(c=>c.GetType().Name=="GeneratorRestart");
                var bar=starter.transform.Find("Control grip");var barSize=bar.GetComponent<MeshFilter>().sharedMesh.bounds.size;
                Assert.That(bar.TransformVector(Vector3.up*barSize.y).magnitude,Is.InRange(.12f,.16f),"Visible handle must remain hand-sized, not a scaled-down baked mesh");
                // Check the actual rendered geometry: the former panel hid the grip at close standing reach.
                foreach(float y in new[]{1.5f,1.65f,1.85f})foreach(float z in new[]{1.1f,1.45f,1.8f})foreach(float x in new[]{-.2f,.24f,.6f})
                {
                    var eye=generator.TransformPoint(new Vector3(x,y,z));float nearest=float.MaxValue;MeshFilter first=null;
                    foreach(var mesh in generator.GetComponentsInChildren<MeshFilter>())
                    {
                        var renderer=mesh.GetComponent<Renderer>();if(!renderer||!renderer.enabled||!mesh.sharedMesh)continue;
                        var ray=new Ray(mesh.transform.InverseTransformPoint(eye),mesh.transform.InverseTransformVector(starter.transform.position-eye));
                        if(!SurfaceHit(mesh.sharedMesh,ray,out float distance))continue;
                        float worldDistance=Vector3.Distance(eye,mesh.transform.TransformPoint(ray.GetPoint(distance)));
                        if(worldDistance<nearest){nearest=worldDistance;first=mesh;}
                    }
                    Assert.That(first&&first.transform.IsChildOf(starter.transform),Is.True,$"Starter hidden by {first?.name} from standing view {x},{y},{z}");
                }
                var rest=starter.transform.position;var rotation=starter.transform.rotation;int coughs=(int)Get(engine,"CoughSerial");
                testHand=new GameObject("Visible starter test hand");var hand=testHand.AddComponent<XRDirectInteractor>();
                var sphere=testHand.AddComponent<SphereCollider>();sphere.isTrigger=true;sphere.radius=.04f;
                var manager=UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();var grab=starter.GetComponent<XRSimpleInteractable>();
                hand.transform.position=rest;yield return null;manager.SelectEnter((IXRSelectInteractor)hand,(IXRSelectInteractable)grab);yield return null;
                hand.transform.position=rest+generator.forward*.31f;yield return null;yield return null;
                Assert.That((float)Get(starter,"ExtensionM"),Is.GreaterThan(.29f));
                Assert.That((int)Get(engine,"CoughSerial"),Is.EqualTo(coughs+1),"An unprimed physical pull still reaches the engine rules");
                manager.SelectExit((IXRSelectInteractor)hand,(IXRSelectInteractable)grab);yield return new WaitForSeconds(.35f);
                Assert.That(Vector3.Distance(starter.transform.position,rest),Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(starter.transform.rotation,rotation),Is.LessThan(.01f));
            }
            finally {Find("LighthouseKeepers.Net.CrewLobbyTransport").GetField("ForceOfflineForTests").SetValue(null,false);System.Environment.SetEnvironmentVariable("LK_S4M2_DISABLED",previous);if(testHand)UnityEngine.Object.Destroy(testHand);}
        }
        [UnityTest] public IEnumerator RepairFirstWaitsForPowerAndThreeRealPullsRestorePump()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_LighthouseKeepers/Scenes/Development/LK_NetworkLobby.unity",new LoadSceneParameters(LoadSceneMode.Single));
            float deadline=Time.realtimeSinceStartup+30;
            while(!GameObject.Find("DevStartDay2") && Time.realtimeSinceStartup<deadline)yield return null;
            var shortcut=GameObject.Find("DevStartDay2");Assert.That(shortcut,Is.Not.Null);
            shortcut.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            while(SceneManager.GetActiveScene().name!="LK_Lab_Environment" && Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;yield return null;
            var watch=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c=>c.GetType().Name=="WatchDemoController");
            var state=Get(watch,"State");Assert.That(Get(state,"Day"),Is.EqualTo(2));Assert.That(Get(state,"Power").ToString(),Is.EqualTo("Off"));
            Assert.That(Find("LighthouseKeepers.StormWatch.WatchDayLaunch").GetMethod("Consume").Invoke(null,null),Is.EqualTo(1),"Dev request must be one-shot");
            var starter=GameObject.Find("GRIP_StarterHandle").GetComponents<MonoBehaviour>().Single(c=>c.GetType().Name=="GeneratorRestart");
            var simple=starter.GetComponent<XRSimpleInteractable>();var rest=starter.transform.position;var direction=GameObject.Find("STN_GENERATOR").transform.forward;
            var hand=new GameObject("Generator test hand").AddComponent<XRDirectInteractor>();var sphere=hand.gameObject.AddComponent<SphereCollider>();sphere.isTrigger=true;sphere.radius=.04f;
            var manager=UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();hand.transform.position=rest;yield return null;
            Call(state,"CompleteRepair");yield return null;
            var flood=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(c=>c.GetType().Name=="FloodController");
            var height=Get(flood,"Height");yield return new WaitForSeconds(.2f);Assert.That(Get(flood,"Height"),Is.EqualTo(height),"No drainage without electric power");
            Assert.That(Get(state,"Complete"),Is.False);
            for(int i=1;i<=3;i++)
            {
                hand.transform.position=rest;manager.SelectEnter((IXRSelectInteractor)hand,(IXRSelectInteractable)simple);yield return null;
                hand.transform.position=rest+direction*.15f;yield return null;Assert.That(Get(state,"StarterPulls"),Is.EqualTo(i-1));
                hand.transform.position=rest+direction*.31f;yield return null;yield return null;
                Assert.That(Get(state,"StarterPulls"),Is.EqualTo(i));
                yield return null;Assert.That(Get(state,"StarterPulls"),Is.EqualTo(i),"Held extension repeats");
                manager.SelectExit((IXRSelectInteractor)hand,(IXRSelectInteractable)simple);
                yield return new WaitForSeconds(.3f);Assert.That((float)Get(starter,"ExtensionM"),Is.LessThan(.001f));
            }
            Assert.That(Get(state,"Power").ToString(),Is.EqualTo("On"));Assert.That(Get(state,"Complete"),Is.True);
            yield return new WaitForSeconds(.2f);Assert.That((float)Get(flood,"Height"),Is.LessThan((float)height));
            Assert.That(Field<TextMesh>(watch,"board").text,Does.Contain("[X] Restart the generator").And.Contain("[X] Repair the pipe"));
            Call(state,"Retry");yield return null;Assert.That(Get(state,"Power").ToString(),Is.EqualTo("Off"));Assert.That(Get(state,"StarterPulls"),Is.Zero);
            UnityEngine.Object.Destroy(hand.gameObject);
        }
    }
}
#endif
