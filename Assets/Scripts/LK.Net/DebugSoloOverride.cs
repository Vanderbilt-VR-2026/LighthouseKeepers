namespace LighthouseKeepers.Net
{
    /// <summary>There is no writable switch or backing field in a release player.</summary>
    public static class DebugSoloOverride
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public const bool CompiledIn = true;
        public static bool Enabled { get; set; } = false;
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Enabled = false;
#else
        public const bool CompiledIn = false;
        public static bool Enabled => false;
#endif
    }
}
