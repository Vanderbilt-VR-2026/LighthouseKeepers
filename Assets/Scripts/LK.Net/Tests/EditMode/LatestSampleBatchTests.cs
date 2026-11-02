using System;
using NUnit.Framework;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class LatestSampleBatchTests
    {
        [Test] public void CongestionKeepsLatestPosePerActorAndPropWithoutOverwritingPartner()
        {
            var q=new LatestSampleBatch();
            for(int i=0;i<1000;i++){q.Put(11,2,BitConverter.GetBytes(i));q.Put(11,3,BitConverter.GetBytes(-i));q.Put(21,1,BitConverter.GetBytes(i+10));}
            Assert.AreEqual(3,q.Count);var wire=q.Pack(7);Assert.Less(wire.Length,2048);Assert.AreEqual(0,q.Count);
            Assert.IsTrue(LatestSampleBatch.TryUnpack(wire,out var seq,out var samples));Assert.AreEqual(7,seq);
            CollectionAssert.AreEquivalent(new[]{999,-999,1009},samples.ConvertAll(s=>BitConverter.ToInt32(s.Data,0)));
        }
        [Test] public void VisualMayHoldLastPoseWhileGameplayRejectsTheStaleHand()
        {
            var buffer=new KeeperPoseBuffer();var q=UnityEngine.Quaternion.identity;
            Assert.IsTrue(buffer.Add(new KeeperPoseFrame{Actor=2,Sequence=1,HeadRotation=q,LeftRotation=q,RightRotation=q},1));
            Assert.IsFalse(buffer.Sample(2,out _));
            Assert.IsTrue(buffer.Sample(2,out var visual,true));Assert.AreEqual(2,visual.Actor);
        }
        [Test] public void TruncatedOversizedAndNonSampleTrafficIsRejected()
        {
            var q=new LatestSampleBatch();q.Put(42,0,new byte[]{1});Assert.AreEqual(0,q.Count);
            q.Put(10,0,new byte[2049]);Assert.AreEqual(0,q.Count);
            Assert.IsFalse(LatestSampleBatch.TryUnpack(new byte[2049],out _,out _));
            Assert.IsFalse(LatestSampleBatch.TryUnpack(new byte[]{1,2,3},out _,out _));
            q.Put(10,0,new byte[]{1,2,3});var wire=q.Pack(1);
            Array.Resize(ref wire,wire.Length/2);Assert.IsFalse(LatestSampleBatch.TryUnpack(wire,out _,out _));
        }
    }
}
