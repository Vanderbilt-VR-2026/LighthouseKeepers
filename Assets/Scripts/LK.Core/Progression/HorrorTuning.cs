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
        [Header("Night 3 radio authentication")]
        public bool radioAuthentication=true;
        [Min(1)] public float authResultSeconds=8;
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
        public float sprintMultiplier=1.6f,sprintSeconds=4.5f,sprintCooldown=3,breathingHearingRange=2;
        [Range(0,1)] public float sprintWarning=.15f;
        [Min(.1f)] public float breathInputLease=.4f;
        [Header("Flare — metres and seconds")]
        public float flareSpeed=50,flareGravity=3.5f,flareFlightSeconds=4,flareBurnSeconds=8;
        public float flareSweepRadius=.06f,flareTargetRadius=.65f,flareTrailSeconds=.12f,flareSubstep=.02f;
        public float flareLightRange=3,flareLightIntensity=2.2f,flareHitSeconds=1.6f;
        [Header("Lantern beam")]
        [Range(30,140)] public float lanternBeamAngle=85,lanternBeamInner=55;
        [Range(5,60)] public float lanternHighBeamAngle=28,lanternHighBeamInner=16;
        [Header("Lantern / defenses")]
        public float highOilSeconds=90,lowOilSeconds=180,pourSeconds=4;
        public float lowRange=2.5f,highRange=4,highExposure=2,highOilCost=.15f,highRetreat=20;
        public float flareRetreat=60,bellRetreat=45;
        [Header("Catch / recovery")]
        public float soloTimeLost=45,reviveSeconds=40;
        public int soloCatchLimit=2;
        public float reviveHoldSeconds=3,reviveGripLease=.35f,reviveGraceSeconds=10;
        public float reviveHandReach=.28f,reviveFadeSeconds=.45f;
        public Vector3 recoveryFloorPosition=new(2.6f,.04f,0);
        [Header("Night 2 possession lesson")]
        public bool possessionLesson=true;
        [Header("Scripted end-of-Day-2 nightmare")]
        public bool nightmareEnabled=false;
        [Min(1)] public float nightmareWakeSeconds=3;
        [Min(1)] public float nightmareTurnSeconds=4;
        [Min(1)] public float nightmareEscapeSeconds=55;
        [Min(1)] public float nightmareRescueSeconds=38;
        [Min(1)] public float nightmareReliefSeconds=8;
        [Min(.1f)] public float nightmareFollowSpeed=.3f;
        [Min(1)] public float nightmareFollowDistance=3.5f;
        [Min(.1f)] public float nightmareChaseSpeed=2.15f;
        [Min(.1f)] public float nightmareLateChaseSpeed=2.3f;
        [Min(0)] public float nightmareChaseLeadSeconds=5f;
        [Min(.5f)] public float nightmareCatchDistance=.75f;
        [Min(.1f)] public float nightmareLungeSeconds=.35f,nightmareLungeNear=1.4f,nightmareLungeFar=1.8f;
        [Min(1)] public float nightmareDropDistance=3.2f,nightmareExitClearance=2f;
        [Range(0,1)] public float nightmareFollowFear=.35f,nightmareChaseFear=.9f;
        public Vector3 nightmareStart=new(-1.59f,6.6f,-2.54f);
        public Vector3 nightmareWakePosition=new(2,6.6f,-.8f);
        public Vector3 nightmareCorner=new(1.7f,3.3f,1.3f);
        public Vector3 nightmareOutside=new(0,0,-10.5f);
        public float lessonStartSeconds=60,lessonCallSeconds=8,lessonSealSeconds=4,lessonThreatSeconds=6,lessonReliefSeconds=6;
        public float possessionLift=.32f,possessionRiseSpeed=.10f,ceilingClearance=.18f;
        public Vector3 lessonPartnerSpot=new(1,3.3f,0);
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
