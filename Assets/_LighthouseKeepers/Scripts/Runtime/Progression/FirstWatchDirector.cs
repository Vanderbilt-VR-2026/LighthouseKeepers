using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using LighthouseKeepers.Core;
using LighthouseKeepers.Flood;

namespace LighthouseKeepers.Progression
{
    public sealed class FirstWatchDirector : MonoBehaviour
    {
        [SerializeField] Transform station;
        [SerializeField] FirstWatchValve brace;
        [SerializeField] AudioClip feedbackClip;
        [Header("Repair and survival")]
        [SerializeField, Min(1), Tooltip("Accumulated wrist rotation required for each seal, in degrees.")]
        float degreesPerValve = FirstWatchRules.DefaultDegreesPerValve;
        [SerializeField, Min(.1f), Tooltip("Seconds of submerged head time before failure.")]
        float airSeconds = FirstWatchRules.DefaultAirSeconds;
        [SerializeField, Min(.1f), Tooltip("Safe time on the landing required to finish, in seconds.")]
        float recoverySeconds = FirstWatchRules.DefaultRecoverySeconds;
        [SerializeField, Min(.1f)] float clampSeconds = 20;
        [SerializeField, Min(.1f)] float wrongValveCooldownSeconds = 1.5f;
        [SerializeField, Min(0), Tooltip("Water rise per wrong turn, in metres.")]
        float wrongValveWaterRise = .08f;
        [Header("Flood rates in metres per second")]
        [SerializeField, Min(.001f)] float repairRiseSpeed = .018f;
        [SerializeField, Min(.001f)] float escapeRiseSpeed = .003f;
        [SerializeField, Min(0)] float repairWaterDrop = .65f;
        [SerializeField, Tooltip("Starting water elevation in world-space metres.")] float startingWaterHeight = .1f;
        [Header("Station and landing bounds in metres")]
        [SerializeField, Min(.1f)] float stationReach = 2.3f;
        [SerializeField] Vector2 landingFootHeight = new(3.05f, 3.65f);
        [SerializeField, Min(.1f)] float landingRadius = 4.8f;
        public FirstWatchRules Rules { get; private set; }
        public float ClampSeconds => clampSeconds;
        public bool Ready { get; private set; }
        public float ClampRemaining { get; private set; }
        public string Feedback { get; private set; } = "";
        public float WaterHeight => flood ? flood.Height : -.25f;
        public bool AtStation => head && station && Vector3.Distance(head.position, station.position + Vector3.up * .4f) < stationReach;
        public bool Braced => AtStation && (ClampRemaining > 0 || (brace && brace.IsHeld) || desktopBrace);
        public bool OnLanding => rig && rig.Camera && rig.transform.position.y >= landingFootHeight.x && rig.transform.position.y < landingFootHeight.y &&
            new Vector2(rig.Camera.transform.position.x, rig.Camera.transform.position.z).magnitude < landingRadius;
        public string Sequence { get; private set; } = "";
        public static string Letter(int valve) => ((char)('A' + valve)).ToString();
        FloodController flood;
        DayDirector days;
        XROrigin rig;
        Transform head;
        AudioSource sound;
        GameObject pipeLeak;
        bool leakInitiallyActive;
        Vector3 spawn;
        Quaternion spawnRotation;
        float wrongCooldown, feedbackUntil;
        bool desktopBrace;
        int watchNumber;

        void Awake() => Rules = new FirstWatchRules(degreesPerValve, airSeconds, recoverySeconds);

        public bool ValidateConfiguration(out string reason)
        {
            reason = "";
            if (!station || !brace || !feedbackClip) reason = "Assign the repair station, brace and feedback clip.";
            else if (!Positive(degreesPerValve) || !Positive(airSeconds) || !Positive(recoverySeconds) ||
                !Positive(clampSeconds) || !Positive(wrongValveCooldownSeconds) || !Positive(stationReach) || !Positive(landingRadius) ||
                !Positive(repairRiseSpeed) || !Positive(escapeRiseSpeed) || !Finite(wrongValveWaterRise) || wrongValveWaterRise < 0 ||
                !Finite(repairWaterDrop) || repairWaterDrop < 0 || !Finite(startingWaterHeight) ||
                !Finite(landingFootHeight.x) || !Finite(landingFootHeight.y) || landingFootHeight.x >= landingFootHeight.y)
                reason = "Use finite, valid watch thresholds and ordered landing heights.";
            return reason.Length == 0;
        }

        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        static bool Positive(float value) => Finite(value) && value > 0;

        IEnumerator Start()
        {
            if (!ValidateConfiguration(out var reason)) { Debug.LogError("First Watch: " + reason, this); yield break; }
            var bootstrap = FindAnyObjectByType<EnvironmentBootstrap>();
            while (bootstrap && !bootstrap.Ready) yield return null;
            flood = FindAnyObjectByType<FloodController>();
            days = FindAnyObjectByType<DayDirector>();
            rig = FindAnyObjectByType<XROrigin>();
            if (!flood || !days || !rig || !rig.Camera) { Debug.LogError("First Watch requires flood, days and a player with a tracked camera."); yield break; }
            head = rig.Camera.transform;
            spawn = rig.transform.position;
            spawnRotation = rig.transform.rotation;
            pipeLeak = GameObject.Find("Active pipe leak");
            leakInitiallyActive = pipeLeak && pipeLeak.activeSelf;
            sound = gameObject.AddComponent<AudioSource>();
            sound.playOnAwake = false; sound.spatialBlend = 0; sound.volume = .3f;
            Ready = true;
            ResetWatch(false);
        }

        void Update()
        {
            if (!Ready) return;
            DesktopInput();
            ClampRemaining = Mathf.Max(0, ClampRemaining - Time.deltaTime);
            wrongCooldown = Mathf.Max(0, wrongCooldown - Time.deltaTime);
            if (Time.time > feedbackUntil) Feedback = "";
            var previous = Rules.Phase;
            Rules.Tick(Time.deltaTime, head.position.y - flood.Height, OnLanding);
            if (Rules.Phase != previous) ApplyPhase();
        }

        public void BeginWatch()
        {
            if (!Ready || Rules.Phase != WatchPhase.Briefing) return;
            Rules.Begin(); days.ApplyDay(3); flood.SetHeight(startingWaterHeight); flood.RiseSpeed = repairRiseSpeed; flood.ResumeFlood();
            Say("Water is rising. Find the marked plumbing controls.", .85f);
        }

        public void LatchClamp()
        {
            if (!Ready || Rules.Phase != WatchPhase.Repair || !AtStation) return;
            ClampRemaining = clampSeconds;
            Say($"Clamp latched for {clampSeconds:0.#} seconds. Follow the valve order.", 1);
        }

        public ValveResult TurnValve(int valve, float degrees)
        {
            if (!Ready || !AtStation) return ValveResult.Ignored;
            if (!Braced) { Feedback = "Hold the brass brace, or latch the clamp."; feedbackUntil = Time.time + 2; return ValveResult.Ignored; }
            if (wrongCooldown > 0 && valve != Rules.NextValve) return ValveResult.Ignored;
            var result = Rules.Turn(valve, degrees, Braced);
            if (result == ValveResult.WrongValve)
            {
                wrongCooldown = wrongValveCooldownSeconds;
                flood.SetHeight(flood.Height + wrongValveWaterRise);
                Say("Wrong valve — pressure surged. Check the order.", .55f);
            }
            else if (result == ValveResult.ValveSealed) Say("Seal locked. Next valve.", 1.3f);
            else if (result == ValveResult.RepairComplete)
            {
                flood.SetHeight(flood.Height - repairWaterDrop);
                flood.RiseSpeed = escapeRiseSpeed;
                if (pipeLeak) pipeLeak.SetActive(false);
                ClampRemaining = 0;
                Say("Leak contained. Water is falling. Take the stairs to the first landing.", 1.6f);
            }
            return result;
        }

        void ApplyPhase()
        {
            if (Rules.Phase == WatchPhase.Recovery)
            { flood.Paused = true; days.ApplyDay(1); Say("First landing reached. Catch your breath.", 1.5f); }
            else if (Rules.Phase == WatchPhase.Escape)
            { flood.RiseSpeed = escapeRiseSpeed; flood.ResumeFlood(); }
            else if (Rules.Phase == WatchPhase.Failed)
            { flood.Paused = true; Say("The water caught you. Restart the watch.", .5f); }
            else if (Rules.Phase == WatchPhase.Complete)
            { flood.Paused = true; Say("First watch survived.", 1.8f); }
        }

        public void RestartWatch()
        {
            if (Ready && (Rules.Phase == WatchPhase.Failed || Rules.Phase == WatchPhase.Complete)) ResetWatch(true);
        }

        void ResetWatch(bool movePlayer)
        {
            flood.ResetFlood(); days.ApplyDay(1);
            if (pipeLeak) pipeLeak.SetActive(leakInitiallyActive);
            Rules.Reset(++watchNumber * 7919);
            Sequence = $"{Letter(Rules.ValveAt(0))}  →  {Letter(Rules.ValveAt(1))}  →  {Letter(Rules.ValveAt(2))}";
            ClampRemaining = wrongCooldown = 0; desktopBrace = false; Feedback = "";
            if (!movePlayer) return;
            // Explicit retry only; never move the camera during active gameplay.
            var character = rig.GetComponent<CharacterController>();
            if (character) character.enabled = false;
            rig.transform.rotation = spawnRotation;
            var offset = Vector3.ProjectOnPlane(head.position - rig.transform.position, Vector3.up);
            rig.transform.position = spawn - offset;
            Physics.SyncTransforms();
            if (character) character.enabled = true;
        }

        void DesktopInput()
        {
#if UNITY_EDITOR
            if (XRSettings.isDeviceActive || Keyboard.current == null) { desktopBrace = false; return; }
            var k = Keyboard.current;
            if (k.enterKey.wasPressedThisFrame) BeginWatch();
            if (k.rKey.wasPressedThisFrame) RestartWatch();
            if (k.cKey.wasPressedThisFrame) LatchClamp();
            desktopBrace = k.bKey.isPressed;
            if (k.digit1Key.isPressed) TurnValve(0, 100 * Time.deltaTime);
            else if (k.digit2Key.isPressed) TurnValve(1, 100 * Time.deltaTime);
            else if (k.digit3Key.isPressed) TurnValve(2, 100 * Time.deltaTime);
#endif
        }

        void Say(string message, float pitch)
        {
            Feedback = message; feedbackUntil = Time.time + 5;
            if (sound && feedbackClip) { sound.pitch = pitch; sound.PlayOneShot(feedbackClip); }
        }
        void OnDisable() { if (flood) flood.Paused = true; }
    }
}
