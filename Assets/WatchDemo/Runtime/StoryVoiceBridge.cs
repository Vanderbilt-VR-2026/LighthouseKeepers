using UnityEngine;
using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    // Additive content bridge. Existing recordings and cue identities stay available.
    // A new performance is used only for an implemented, mechanically accurate trigger.
    public static class StoryVoiceBridge
    {
        public static string Replacement(int legacy,WatchState s)
        {
            bool crew=s.HumanKeeperCount>=2;
            switch(legacy)
            {
                case 1:return crew?"HM_N1_ARRIVAL_COOP":null; // Solo names Owen: not shipped yet.
                case 13:return "HM_N2_OPEN";
                case 76:return "HM_N3_OPEN";
                case 94:return "HM_N4_OPEN";
                case 95:return "HM_N5_OPEN";
                case 2:return !s.Repaired?"HM_N1_PIPE_LEAK":null;
                case 4:return s.NightTwoFuelFailure&&s.NightTwoOutage&&!s.Engine.Running?"HM_N2_FUEL_FAILURE":null;
                case 5:return crew&&s.Complete?"HM_TASKS_DONE_COOP":null;
                case 16:return "HM_EARLY_SLEEP";
                case 21:return s.Engine?.Settled==true?"HM_GEN_RUNNING":null;
                case 27:return "HM_WARN_WATER_HALF";
                case 28:return s.WaterDanger>=.85f?"HM_WARN_WATER_CRITICAL":null;
                case 29:return "HM_WARN_WATER_CRITICAL";
                case 30:return s.AimingShip>=0?"HM_WARN_BEACON_DARK":null;
                case 92:return "HM_RADIO_TRANSMIT_OK";
                case 97:return "HM_SHIP_DANGER";
                case 98:return "HM_SHIP_PASSED";
                case 99:return "HM_FAIL_WRECK";
                // No invented two-wheel valve, missing lantern/hide, compass direction, or invisible Owen.
                default:return null;
            }
        }
        public static int Resolve(int legacy,WatchState state,RadioLineCatalog catalog)
        {
            var entry=StoryVoiceCatalog.Get(Replacement(legacy,state));
            return entry!=null&&catalog.Line(entry.cue)?entry.cue:legacy;
        }
        public static bool RadioCue(int cue)=>StoryVoiceCatalog.Get(cue) is {Antagonist:false};
    }
}
