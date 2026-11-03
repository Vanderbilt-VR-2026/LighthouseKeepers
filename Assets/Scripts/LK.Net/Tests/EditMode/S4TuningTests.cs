using LighthouseKeepers.Core;
using NUnit.Framework;
using UnityEngine;
namespace LighthouseKeepers.Net.Tests
{
    public sealed class S4TuningTests
    {
        [Test] public void OutagePulseIsSmoothBoundedAndHasOnePeakPerTwoSeconds()
        {
            var t=ScriptableObject.CreateInstance<S4Tuning>();
            try
            {
                float last=t.EmergencyPulse(0);int maxima=0;float previousSlope=0;
                for(int i=1;i<=400;i++)
                {
                    float value=t.EmergencyPulse(i*.01f),slope=value-last;
                    Assert.That(value,Is.InRange(.0999f,.1501f));Assert.Less(Mathf.Abs(slope),.001f);
                    if(previousSlope>0&&slope<0)maxima++;if(slope!=0)previousSlope=slope;last=value;
                }
                Assert.AreEqual(2,maxima);
            }
            finally{Object.DestroyImmediate(t);}
        }
    }
}
