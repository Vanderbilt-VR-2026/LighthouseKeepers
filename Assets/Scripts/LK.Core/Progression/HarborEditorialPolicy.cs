namespace LighthouseKeepers.Core
{
    public static class HarborEditorialPolicy
    {
        public static bool Allows(int cue)
        {
            if(cue>=200){
                var line=LighthouseKeepers.Core.Progression.StoryVoiceCatalog.Get(cue);
                return line!=null&&Story(line.id);
            }
            return cue==1||cue==4||cue==5||cue==13||cue==17||cue==18||cue==29||cue==76||cue==87||cue==89||cue==92||cue>=94&&cue<=101;
        }
        public static bool Story(string id)=>id=="HM_N1_ARRIVAL_COOP"||id=="HM_N1_ARRIVAL_SOLO"||id=="HM_N2_OPEN"||id=="HM_N3_OPEN"||id=="HM_N4_OPEN"||id=="HM_N5_OPEN"||id=="HM_N2_FUEL_FAILURE"||id=="HM_N2_LANTERN_INTRO"||id=="HM_WARN_WATER_CRITICAL"||id=="HM_GEN_RUNNING"||id=="HM_RADIO_TRANSMIT_OK"||id=="HM_SHIP_DANGER"||id=="HM_SHIP_PASSED"||id=="HM_FAIL_WRECK"||id=="HM_TASKS_DONE_COOP";
    }
}
