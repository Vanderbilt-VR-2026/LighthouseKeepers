using System;
using UnityEngine;
namespace LighthouseKeepers.Core.Progression
{
    [Serializable] public sealed class HorrorNight
    {
        public int night, sabotages, apparitions, hunts;
        public bool dawnGlimpse, shipAttack;
        public float beatMin=60,beatMax=120;
    }
    [CreateAssetMenu(menuName="Lighthouse/Horror loop tuning")]
    public sealed class HorrorTuning:ScriptableObject
    {
        [Tooltip("Kept OFF until live navigation, protection and escape integration pass.")]
        public bool hunterEnabled;
        [Tooltip("Never enable before hiding and recovery have passed co-op tests.")]
        public bool catchesEnabled;
        [Header("Hunter — metres and seconds")]
        public float standingHeight=2.3f, navigationHeight=1.8f, radius=.20f;
        public float walkSpeed=1.2f,chaseSpeed=3.4f, sightRange=9, sightHalfAngle=65;
        public float telegraphSeconds=2,quietSeconds=2.5f,searchSeconds=12,stalkSeconds=10,maxChaseSeconds=24;
        public float catchRange=.65f,navRepathSeconds=.35f,stuckSeconds=2,retreatSeconds=20;
        public float navVoxel=.05f,navClimb=.35f,navSlope=55,navSampleRange=.3f;
        [Header("Safety / pacing")]
        public float earliestScare=60,problemGap=15,reliefMin=10,reliefMax=20;
        public float tellMin=3,tellMax=6,maximumEscapeSeconds=10;
        public int lungesPerNight=1;
        [Header("Sprint / hiding")]
        public float sprintMultiplier=1.6f,sprintSeconds=4,sprintCooldown=6,breathingHearingRange=2;
        [Header("Lantern / defenses")]
        public float highOilSeconds=90,lowOilSeconds=180,pourSeconds=4;
        public float lowRange=2.5f,highRange=4,highExposure=2,highOilCost=.15f,highRetreat=20;
        public float flareRetreat=60,bellRetreat=45;
        [Header("Catch / recovery")]
        public float soloTimeLost=45,reviveSeconds=40;
        public int soloCatchLimit=2;
        [Header("Fear / voice")]
        public float calmBpm=62,panicBpm=140,clearFog=.02f,darkFog=.06f,highFear=.6f;
        public float antagonistSpacing=90,harborQuietBeforeWhisper=4;
        public int antagonistPerNight=6;
        public HorrorNight[] nights={
            new(){night=1,sabotages=1,apparitions=0,hunts=0,dawnGlimpse=true},
            new(){night=2,sabotages=2,apparitions=3,hunts=0},
            new(){night=3,sabotages=3,apparitions=2,hunts=1},
            new(){night=4,sabotages=5,apparitions=2,hunts=2,shipAttack=true},
            new(){night=5,sabotages=5,apparitions=3,hunts=3,shipAttack=true}};
        public HorrorNight Night(int night)=>Array.Find(nights,x=>x.night==night);
        public static HorrorTuning Current=>Resources.Load<HorrorTuning>("HorrorTuning");
    }
}
