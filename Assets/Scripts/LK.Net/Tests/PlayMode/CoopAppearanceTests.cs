using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class CoopAppearanceTests
    {
        [UnityTest] public IEnumerator BothHouseSpawnsHaveStandingClearanceAndRemoteHandsAreVisualOnlyTanMeshes()
        {
            yield return SceneManager.LoadSceneAsync("LK_Lab_Environment");yield return null;
            foreach(float z in new[]{-9.15f,-7.65f})
            {
                var point=new Vector3(-.9f,.04f,z);
                Assert.IsTrue(Physics.Raycast(point+Vector3.up,Vector3.down,out var hit,1.5f,~0,QueryTriggerInteraction.Ignore),"House spawn has no floor");
                Assert.That(hit.point.y,Is.InRange(-.05f,.1f));
                Assert.IsFalse(Physics.CheckCapsule(point+Vector3.up*.4f,point+Vector3.up*1.45f,.2f,~0,QueryTriggerInteraction.Ignore),"Standing volume hits House furniture");
            }
            var copies=new GameObject[2];
            for(int i=0;i<2;i++){copies[i]=new GameObject("Remote mesh verification");copies[i].AddComponent<RemoteHandVisual>().left=i==0;}
            yield return new WaitForSeconds(1.1f);
            foreach(var copy in copies)
            {
                var skin=copy.GetComponentInChildren<SkinnedMeshRenderer>();Assert.IsNotNull(skin);Assert.IsNotNull(skin.sharedMesh);
                var colour=skin.sharedMaterial.GetColor("_BaseColor");Assert.That(colour.r,Is.EqualTo(198f/255).Within(.01f));
                Assert.That(colour.g,Is.EqualTo(142f/255).Within(.01f));Assert.That(colour.b,Is.EqualTo(106f/255).Within(.01f));
                Assert.IsEmpty(copy.GetComponentsInChildren<Camera>(true));Assert.IsEmpty(copy.GetComponentsInChildren<AudioListener>(true));
                Assert.IsEmpty(copy.GetComponentsInChildren<XRBaseInteractor>(true));Object.Destroy(copy);
            }
        }
    }
}
