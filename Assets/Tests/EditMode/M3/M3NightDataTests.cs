using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.Tests.M3
{
    public sealed class M3NightDataTests
    {
        [Test] public void FiveNightsAndDemoHaveCompleteValidDataWithoutEnablingTheCampaign()
        {
            var c=AssetDatabase.LoadAssetAtPath<FiveNightCatalog>("Assets/WatchDemo/Resources/M3Campaign.asset");Assert.IsNotNull(c);Assert.False(c.enabled);
            Assert.AreEqual(5,c.nights.Length);CollectionAssert.AreEqual(new[]{1,2,3,4,5},c.nights.Select(d=>d.day));
            foreach(var day in c.nights.Concat(new[]{c.demo})){
                Assert.IsNotNull(day.scenario);Assert.True(day.scenario.configured);Assert.IsEmpty(day.scenario.Validate().ToArray(),day.name);Assert.False(day.scenario.timingApproved,"Timing must be measured in A4");
                CollectionAssert.AreEquivalent(day.tasks.Select(t=>t.stationId),day.scenario.stations.Select(s=>s.stationId));
                Assert.IsTrue(day.scenario.stations.All(s=>s.soloLocalInformation));
            }
            Assert.True(c.nights[0].scenario.noFail);Assert.True(c.nights[0].scenario.ships.Single().safePreview);
            Assert.True(c.nights[4].scenario.ships.Last().largeFinale);Assert.AreEqual(3,c.nights[4].scenario.ships.Length);
            Assert.True(c.demo.scenario.demo);Assert.AreEqual(360,c.demo.scenario.soloSeconds);
        }
        [Test] public void ExistingSoloCatalogHasNotBeenReplacedOrGivenCampaignSchedules()
        {
            foreach(var path in new[]{"Assets/WatchDemo/Resources/WatchDays.asset","Assets/WatchDemo/Resources/WatchDaysS4M2.asset"}){
                var catalog=AssetDatabase.LoadAssetAtPath<DayCatalog>(path);Assert.IsNotNull(catalog);
                Assert.True(catalog.days.All(d=>d.scenario==null||!d.scenario.configured),"Legacy catalog changed: "+path);
            }
        }
        [Test] public void InvalidShipWindowsAndUnknownDependenciesAreRejected()
        {
            var p=new NightScenario{id="test",nameKey="test",openingCue=1,soloSeconds=100,crewSeconds=100,stations=new[]{new NightStationRule{stationId="pipe",activeSteps=new[]{"tighten"}}}};
            p.ships=new[]{new NightShipEvent{id="ship",nameKey="ship",sightedAtSeconds=80,approachSeconds=60,windowSeconds=50}};
            p.faults=new[]{new NightFault{id="fault",stationId="pipe",gate=NightEventGate.JobComplete,afterStationId="unknown"}};
            var errors=p.Validate().ToArray();Assert.True(errors.Any(e=>e.Contains("beyond dawn")));Assert.True(errors.Any(e=>e.Contains("dependency")));
            p.soloSeconds=float.NaN;Assert.True(p.Validate().Any(e=>e.Contains("duration")));
        }
    }
}
