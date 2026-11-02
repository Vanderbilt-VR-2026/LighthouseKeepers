using NUnit.Framework;
using UnityEngine;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class CoopReleaseObserver : MonoBehaviour,LighthouseKeepers.Interaction.INetworkPropRelease
    {public int releases;public void OnNetworkReleased()=>releases++;}
    public sealed class GrabReleasePhysicsTests
    {
        [Test] public void ReleasePacketPreservesMomentumAndWorldGeneration()
        {
            var sent=new FusionGrabAuthority.Packet {id=2,action=2,epoch=3,generation=7,path="Lab/Wrench",
                position=Vector3.up,rotation=Quaternion.identity,velocity=new Vector3(3,4,0),angularVelocity=Vector3.up*2};
            string json=JsonUtility.ToJson(sent);
            Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(json),FusionMessageChannel.MaxBytes);
            var received=JsonUtility.FromJson<FusionGrabAuthority.Packet>(json);
            Assert.AreEqual(sent.velocity,received.velocity);Assert.AreEqual(sent.angularVelocity,received.angularVelocity);
            Assert.AreEqual(7,received.generation);Assert.AreEqual(3,received.epoch);
        }
        [Test] public void DisconnectClearsRemoteOwnerAndNotifiesTheEightSecondReturnExactlyOnce()
        {
            var root=new GameObject("Disconnect prop");var session=new GameObject("Disconnect authority");
            try
            {
                var observer=root.AddComponent<CoopReleaseObserver>();
                var body=root.AddComponent<Rigidbody>();
                var grab=root.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                var authority=session.AddComponent<FusionGrabAuthority>();
                var binding=root.AddComponent<FusionGrabBinding>();binding.Bind(authority,1,"Test/Wrench",grab);
                binding.ApplyState(new FusionGrabAuthority.Packet{owner=2,epoch=1,sequence=1,position=Vector3.zero,rotation=Quaternion.identity});
                Assert.AreEqual(2,binding.Owner);Assert.IsFalse(binding.CanSimulateReleased);
                binding.Unbind();Assert.AreEqual(0,binding.Owner);Assert.IsTrue(binding.CanSimulateReleased);
                Assert.IsFalse(body.isKinematic);Assert.AreEqual(1,observer.releases);
                binding.Unbind();Assert.AreEqual(1,observer.releases);
            }
            finally{Object.DestroyImmediate(root);Object.DestroyImmediate(session);}
        }
        [Test] public void HostReceivesReleaseMomentumAndCapsExtremeThrows()
        {
            var root=new GameObject("Network release physics");
            try
            {
                root.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
                var body=root.AddComponent<Rigidbody>();
                var interaction=root.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                var session=new GameObject("Release test authority");
                try
                {
                    var authority=session.AddComponent<FusionGrabAuthority>();
                    var binding=root.AddComponent<FusionGrabBinding>();
                    binding.Bind(authority,1,"Test/Prop",interaction);
                    body.isKinematic=true;
                    binding.ReleaseOnHost(Vector3.up,Quaternion.identity,Vector3.right*100,Vector3.up*100);
                    Assert.IsFalse(body.isKinematic);
                    Assert.AreEqual(Vector3.up,body.position);
                    Assert.That(body.linearVelocity.magnitude,Is.GreaterThan(0).And.LessThanOrEqualTo(10.001f));
                    Assert.That(body.angularVelocity.magnitude,Is.GreaterThan(0).And.LessThanOrEqualTo(20.001f));
                    binding.DropOnHost();
                    Assert.AreEqual(Vector3.zero,body.linearVelocity);
                    Assert.AreEqual(Vector3.zero,body.angularVelocity);
                }
                finally {Object.DestroyImmediate(session);}
            }
            finally {Object.DestroyImmediate(root);}
        }
    }
}
