using System;
using UnityEngine;

namespace LighthouseKeepers.Core.Progression
{
    [CreateAssetMenu(menuName = "Lighthouse Keepers/Sprint 3/Day")]
    public sealed class DayDefinition : ScriptableObject
    {
        [Min(1)] public int day = 1;
        [Range(1, 2)] public int requiredKeepers = 1;
        public string briefingHeading = "Day 1 — The leak";
        [TextArea] public string briefingText = "Find the wrench and repair the leaking pipe.";
        public AudioClip briefingVoice;
        public string faultObjectivePrefix = "";
        public ObjectiveDefinition[] objectives = Array.Empty<ObjectiveDefinition>();
        public IncidentDefinition[] incidents = Array.Empty<IncidentDefinition>();
        public DaySuccessCondition successCondition;
        public DayFailureCondition failCondition;
        public bool requiresPartnerIsolation;
        [Min(1)] public int primerStrokes = 3;
        [Min(0)] public float crankCooldownSeconds = 2f;
        [Min(0)] public float chokeReleaseSeconds = 10f;
        [Min(.01f)] public float isolationFreshnessSeconds = .3f;
        [Range(0, 1)] public float initialFuelFraction = .25f;

        public DayPlan CreatePlan() => new DayPlan
        {
            Day = day, RequiredKeepers = requiredKeepers, Heading = briefingHeading, Briefing = briefingText,
            FaultObjectivePrefix = faultObjectivePrefix, Objectives = objectives, Incidents = incidents,
            Success = successCondition, Failure = failCondition, RequiresPartnerIsolation = requiresPartnerIsolation,
            PrimerStrokes = primerStrokes, CrankCooldownSeconds = crankCooldownSeconds,
            ChokeReleaseSeconds = chokeReleaseSeconds, IsolationFreshnessSeconds = isolationFreshnessSeconds
        };
    }
}
