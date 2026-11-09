#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace LighthouseKeepers.Presentation.Environment.Tests
{
    // Solo-playtest regressions: open stairwell drops, floor bumps near the wrench bench, throat flicker, glass lens.
    public sealed class PlaytestWorldFixesTests
    {
        const string GuardPrefix="Playtest edge guard ",Wrench="Grab wrench — grip to hold";
        static Vector3 Polar(float azimuth,float radius,float y){float a=azimuth*Mathf.Deg2Rad;return new Vector3(radius*Mathf.Sin(a),y,radius*Mathf.Cos(a));}
        static Renderer R(string name){var go=GameObject.Find(name);Assert.That(go,Is.Not.Null,name+" missing from Lab");return go.GetComponent<Renderer>();}
        [UnitySetUp] public IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Lab/LK_Lab_Environment.unity",new LoadSceneParameters(LoadSceneMode.Single));
            for(int i=0;i<10;i++)yield return null;
            Assert.That(GameObject.Find("Playtest world fixes"),Is.Not.Null,"Runtime playtest fixes must bootstrap on Lab load.");
        }
        [UnityTest] public IEnumerator OpenStairwellEdgesAreGuardedWhileStairWalkLinesStayClear()
        {
            // Cross each open radial edge 0.5 m above the floor: a guard must stop the walk before the 3.3 m drop.
            foreach(var (floorY,from,to,radius) in new[]{(6.6f,325f,315f,2.6f),(6.6f,325f,315f,3.4f),(9.9f,325f,315f,1.4f),(9.9f,325f,315f,2.5f),(9.9f,135f,145f,2.5f)})
            {
                Vector3 a=Polar(from,radius,floorY+.5f),b=Polar(to,radius,floorY+.5f);
                Assert.That(Physics.Raycast(a,(b-a).normalized,out var hit,(b-a).magnitude,~0,QueryTriggerInteraction.Ignore),Is.True,$"Open edge at floor {floorY} az {from}->{to} r {radius}");
                Assert.That(hit.collider.name,Does.StartWith(GuardPrefix));
                Assert.That(Physics.Raycast(b,Vector3.down,out var below,6,~0,QueryTriggerInteraction.Ignore)&&below.point.y<floorY-2.5f,Is.True,"Guard must sit on the drop side of the edge");
            }
            // Stair departure (F3) and arrival (F4) walk-line points from lighthouse-layout.json stay unobstructed by guards.
            var buffer=new Collider[16];
            foreach(var p in new[]{new Vector3(-.900863f,6.6f,1.441682f),new Vector3(-1.092739f,6.6f,1.302276f),new Vector3(-1.202082f,6.691667f,1.202082f),new Vector3(1.092739f,9.9f,-1.302276f),new Vector3(1.263346f,9.9f,-1.137522f),new Vector3(.97508f,9.808333f,-1.392558f)})
            {
                int n=Physics.OverlapCapsuleNonAlloc(p+Vector3.up*.25f,p+Vector3.up*1.43f,.22f,buffer,~0,QueryTriggerInteraction.Ignore);
                for(int i=0;i<n;i++)Assert.That(buffer[i].name,Does.Not.StartWith(GuardPrefix),"Guard blocks stair walk line at "+p);
            }
            yield return null;
        }
        [UnityTest] public IEnumerator InteriorFloorsHaveNoRockStepsAndPlayerCapsuleIgnoresWrench()
        {
            // Exterior outcrop hulls used to stand 5 cm proud of the House/F1 floors (bench, centre aisle, F1 north wall).
            var hits=new RaycastHit[8];
            for(float x=-3.4f;x<=3.4f;x+=.2f)for(float z=-9.7f;z<=3.7f;z+=.2f)
            {
                bool house=z<=-4.9f,f1=x*x+z*z<3.8f*3.8f;if(!house&&!f1)continue;
                int n=Physics.RaycastNonAlloc(new Vector3(x,.5f,z),Vector3.down,hits,.7f,~0,QueryTriggerInteraction.Ignore);
                for(int i=0;i<n;i++)if(hits[i].collider.name.StartsWith("UCX_SM_Rock_"))Assert.That(hits[i].point.y,Is.LessThanOrEqualTo(0f),$"{hits[i].collider.name} steps the floor at ({x:F1},{z:F1})");
            }
            var wrench=GameObject.Find(Wrench);Assert.That(wrench,Is.Not.Null);
            var bodies=Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None);Assert.That(bodies,Is.Not.Empty);
            foreach(var body in bodies)foreach(var c in wrench.GetComponentsInChildren<Collider>())
                Assert.That(Physics.GetIgnoreCollision(body,c),Is.True,"Player capsule can climb the wrench collider "+c.name);
            yield return null;
        }
        [UnityTest] public IEnumerator ThroatShellFacesAreNotCoplanarAndLensReadsAsGlass()
        {
            float house=R("SM_Floor_House").bounds.max.y;
            foreach(var floor in new[]{"SM_Floor_Throat","SM_Floor_Porch"})Assert.That(R(floor).bounds.max.y,Is.LessThanOrEqualTo(house-.002f),floor+" is coplanar with the overlapping House floor");
            Assert.That(R("SM_Reveal_F1_0_Frame_N1").bounds.min.x,Is.LessThanOrEqualTo(R("SM_Wall_Throat_1").bounds.min.x-.002f),"East door frame strip is coplanar with the throat wall face");
            Assert.That(R("SM_Reveal_F1_0_Frame_1").bounds.max.x,Is.GreaterThanOrEqualTo(R("SM_Wall_Throat_N1").bounds.max.x+.002f),"West door frame strip is coplanar with the throat wall face");
            foreach(var (landing,floorY) in new[]{("SM_Landing_F1_F2_departure",0f),("SM_Landing_F1_F2_arrival",3.3f),("SM_Landing_F2_F3_departure",3.3f),("SM_Landing_F2_F3_arrival",6.6f),("SM_Landing_F3_F4_departure",6.6f),("SM_Landing_F3_F4_arrival",9.9f)})
                Assert.That(R(landing).bounds.max.y-floorY,Is.GreaterThanOrEqualTo(.003f),landing+" nearly coplanar with its floor");
            var lens=R("Fresnel prism ring").sharedMaterial;
            Assert.That(lens.GetFloat("_Surface"),Is.EqualTo(1f),"Fresnel lens must be transparent");
            Assert.That(lens.GetFloat("_Smoothness"),Is.GreaterThanOrEqualTo(.8f));Assert.That(lens.IsKeywordEnabled("_EMISSION"),Is.True,"Lens keeps its powered emission tint");
            var glass=R("SM_Glass_Lantern_0").sharedMaterial;Assert.That(glass.GetFloat("_Surface"),Is.EqualTo(1f));Assert.That(glass.GetFloat("_Smoothness"),Is.GreaterThanOrEqualTo(.8f),"Lantern glazing needs visible reflections");
            yield return null;
        }
    }
}
#endif
