using System;
using System.Numerics;
using NUnit.Framework;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class CrewLimbIKTests
    {
        [Test] public void ReachableHandKeepsBothSegmentLengthsAndBendsTowardThePole()
        {
            var root=new Vector3(.2f,1.4f,0);var target=new Vector3(.4f,1.1f,.2f);
            var result=CrewLimbIK.Solve(root,target,new Vector3(.5f,1.2f,-.3f),.26f,.27f);
            Assert.That(Vector3.Distance(root,result.Bend),Is.EqualTo(.26f).Within(.0001));
            Assert.That(Vector3.Distance(result.Bend,result.End),Is.EqualTo(.27f).Within(.0001));
            Assert.That(Vector3.Distance(target,result.End),Is.LessThan(.0001));
            Assert.Less(result.Bend.Z,.1f);Assert.AreEqual(1,result.Stretch);
        }
        [Test] public void OutOfReachTargetsRespectTheMaximumStretch()
        {
            var result=CrewLimbIK.Solve(Vector3.Zero,Vector3.UnitZ*5,Vector3.UnitX,.25f,.25f,1.4f);
            Assert.AreEqual(1.4f,result.Stretch,.0001);
            Assert.LessOrEqual(result.End.Length(),.7f);Assert.Greater(result.End.Length(),.69f);
            Assert.That(result.Bend.Length(),Is.EqualTo(.35f).Within(.0001));
        }
        [Test] public void FoldedAndCollinearTargetsStillProduceFiniteValidBones()
        {
            foreach(var target in new[]{Vector3.Zero,Vector3.UnitY*.1f,Vector3.UnitY})
            {
                var result=CrewLimbIK.Solve(Vector3.Zero,target,Vector3.UnitY,.4f,.3f);
                Assert.IsTrue(float.IsFinite(result.Bend.X)&&float.IsFinite(result.Bend.Y)&&float.IsFinite(result.Bend.Z));
                Assert.That(result.Bend.Length(),Is.EqualTo(.4f).Within(.001));
                Assert.That(Vector3.Distance(result.Bend,result.End),Is.EqualTo(.3f).Within(.001));
            }
        }
        [Test] public void InvalidArtistLengthsAndNonfiniteTargetsAreRejected()
        {
            Assert.Throws<ArgumentException>(()=>CrewLimbIK.Solve(Vector3.Zero,Vector3.One,Vector3.UnitX,0,1));
            Assert.Throws<ArgumentException>(()=>CrewLimbIK.Solve(Vector3.Zero,new Vector3(float.NaN,0,0),Vector3.One,1,1));
            Assert.Throws<ArgumentException>(()=>CrewLimbIK.Solve(Vector3.Zero,Vector3.One,Vector3.UnitX,1,1,.5f));
        }
        [Test] public void APlantedFootStaysStillAndAStepLiftsThenLandsExactly()
        {
            var step=new CrewFootstep();step.Reset(Vector3.Zero);step.Tick(.2f);Assert.AreEqual(Vector3.Zero,step.Position);
            step.Begin(new Vector3(.2f,.1f,0),.2f,.08f);step.Tick(.1f);
            Assert.IsTrue(step.Moving);Assert.Greater(step.Position.Y,.1f);
            step.Tick(.1f);Assert.IsFalse(step.Moving);Assert.AreEqual(new Vector3(.2f,.1f,0),step.Position);
        }
        [Test] public void AnActiveFootstepCannotBeRetargetedAndTeleportResetCancelsIt()
        {
            var step=new CrewFootstep();step.Reset(Vector3.Zero);step.Begin(Vector3.UnitZ,.2f,.05f);
            step.Begin(Vector3.UnitX,.1f,.1f);step.Tick(.2f);Assert.AreEqual(Vector3.UnitZ,step.Position);
            step.Begin(Vector3.One,.3f,.1f);step.Reset(Vector3.UnitY*3);step.Tick(1);
            Assert.AreEqual(Vector3.UnitY*3,step.Position);Assert.IsFalse(step.Moving);
        }
    }
}
