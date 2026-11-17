namespace LighthouseKeepers.Core.Progression
{
    // Authoritative observation contract for the future hunter/ANT adapter. Unknown evidence is safe.
    // Renderer brightness, same-floor membership and distance alone are not proof of being witnessed.
    public readonly struct KeeperExposure
    {
        public readonly bool ObservationsValid, PartnerCanSee, RoomPowered, LanternHighNear, HiddenUnseen;
        public KeeperExposure(bool observationsValid,bool partnerCanSee,bool roomPowered,bool lanternHighNear,bool hiddenUnseen=false)
        {ObservationsValid=observationsValid;PartnerCanSee=partnerCanSee;RoomPowered=roomPowered;LanternHighNear=lanternHighNear;HiddenUnseen=hiddenUnseen;}
        public bool Dark => ObservationsValid&&!RoomPowered&&!LanternHighNear;
        public bool Alone => ObservationsValid&&!PartnerCanSee;
        public bool CanBeTaken => Alone&&Dark&&!HiddenUnseen;
    }
    public static class KeeperWitnessRules
    {
        public static bool CanStartHunt(int night,double elapsed,bool tutorialStep,bool fullScares,bool escapeVerified,KeeperExposure keeper) =>
            night>=3&&elapsed>=60&&!tutorialStep&&fullScares&&escapeVerified&&keeper.CanBeTaken;
        public static bool CanSabotage(bool validObservation,bool anyKeeperWatching,double sinceNewProblem,double sinceFix) =>
            validObservation&&!anyKeeperWatching&&sinceNewProblem>=15&&sinceFix>=15;
    }
}
