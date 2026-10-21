using System.Collections.Generic;
using NUnit.Framework;
using LighthouseKeepers.Interaction;
namespace LighthouseKeepers.Net.Tests
{
    /// <summary>Deterministic application-rule model, not a Photon/Quest latency measurement.</summary>
    public sealed class LatencyGateModelTests
    {
        [Test] public void EightyMillisecondRttOnePercentLossKeepsFreshDistinctHolds()
        {
            var gate=new CoopStationGate();int packets=0,dropped=0,openSamples=0,soloPasses=0;
            var arrivals=new Queue<(double time,int actor)>();
            // Two actors, 20 heartbeats/s each for 60 seconds. Drop every 100th datagram, 40 ms uplink.
            for (int step=0;step<1200;step++)
            {
                double now=step*.05;
                for(int actor=1;actor<=2;actor++)
                { packets++;if(packets%100==0){dropped++;continue;}arrivals.Enqueue((now+.04,actor)); }
                while(arrivals.Count>0 && arrivals.Peek().time<=now)
                { var p=arrivals.Dequeue();gate.Observe(p.actor,p.actor==1?10:20,true,p.time); }
                if(gate.Allows(10,20,now))openSamples++;
                var solo=new CoopStationGate();solo.Observe(1,10,true,now);solo.Observe(1,20,true,now);
                if(solo.Allows(10,20,now))soloPasses++;
            }
            Assert.AreEqual(2400,packets);Assert.AreEqual(24,dropped);Assert.AreEqual(1199,openSamples);Assert.Zero(soloPasses);
            Assert.IsFalse(gate.Allows(10,20,60.7));
            System.Console.WriteLine("MODEL ONLY: RTT=80ms; sent=2400; dropped=24 (1%); open samples=1199/1200; solo passes=0/1200; stale at 60.7s=false");
        }
        [Test] public void SimultaneousRequestsAtEightyMillisecondsGrantExactlyOneOwner()
        {
            var leases=new GrabAuthority();leases.Register(1);
            Assert.IsTrue(leases.Acquire(1,1,0,.04));Assert.IsFalse(leases.Acquire(1,2,0,.04));
            Assert.IsTrue(leases.Release(1,1,1));Assert.IsTrue(leases.Acquire(1,2,2,.20));
            Assert.IsFalse(leases.Touch(1,1,1,.21));Assert.AreEqual(2,leases.Find(1).Owner);
            System.Console.WriteLine("MODEL ONLY: grants=1/2 simultaneous requests; rejected stale owner updates=1/1");
        }
    }
}
