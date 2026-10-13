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
        [SerializeField, Min(.001f)] float repairRiseSpeed = .018f;
        [SerializeField, Min(.001f)] float escapeRiseSpeed = .003f;
        [SerializeField, Min(0)] float repairWaterDrop = .65f;
        public FirstWatchRules Rules { get; } = new();
        public bool Ready { get; private set; }
        public float ClampRemaining { get; private set; }
        public string Feedback { get; private set; } = "";
        public float WaterHeight => flood ? flood.Height : -.25f;
        public bool AtStation => head && station && Vector3.Distance(head.position, station.position + Vector3.up * .4f) < 2.3f;
        public bool Braced => AtStation && (ClampRemaining > 0 || (brace && brace.IsHeld) || desktopBrace);
        public bool OnLanding => rig && rig.transform.position.y >= 3.05f && rig.transform.position.y < 3.65f &&
            new Vector2(rig.Camera.transform.position.x, rig.Camera.transform.position.z).magnitude < 4.8f;
        public string Sequence => $"{Letter(Rules.ValveAt(0))}  →  {Letter(Rules.ValveAt(1))}  →  {Letter(Rules.ValveAt(2))}";
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

        IEnumerator Start()
        {
            var bootstrap = FindAnyObjectByType<EnvironmentBootstrap>();
            while (bootstrap && !bootstrap.Ready) yield return null;
            flood = FindAnyObjectByType<FloodController>();
            days = FindAnyObjectByType<DayDirector>();
            rig = FindAnyObjectByType<XROrigin>();
            if (!flood || !days || !rig || !station) { Debug.LogError("First Watch requires flood, days, player and repair station."); yield break; }
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
            Rules.Begin(); days.ApplyDay(3); flood.SetHeight(.1f); flood.RiseSpeed = repairRiseSpeed; flood.ResumeFlood();
            Say("Water is rising. Find the marked plumbing controls.", .85f);
        }

        public void LatchClamp()
        {
            if (!Ready || Rules.Phase != WatchPhase.Repair || !AtStation) return;
            ClampRemaining = 20;
            Say("Clamp latched for 20 seconds. Follow the valve order.", 1);
        }

        public ValveResult TurnValve(int valve, float degrees)
        {
            if (!Ready || !AtStation) return ValveResult.Ignored;
            if (!Braced) { Feedback = "Hold the brass brace, or latch the clamp."; feedbackUntil = Time.time + 2; return ValveResult.Ignored; }
            if (wrongCooldown > 0 && valve != Rules.NextValve) return ValveResult.Ignored;
            var result = Rules.Turn(valve, degrees, Braced);
            if (result == ValveResult.WrongValve)
            {
                wrongCooldown = 1.5f;
                flood.SetHeight(flood.Height + .08f);
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
