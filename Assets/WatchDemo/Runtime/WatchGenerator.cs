using UnityEngine;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Presentation.Environment;

namespace LighthouseKeepers.WatchDemo
{
    // Presentation/input adapter. WatchState alone owns power and completed tasks.
    public sealed class WatchGenerator : MonoBehaviour
    {
        [SerializeField] GeneratorRestart starter;
        [SerializeField] Transform engine;
        [SerializeField] Renderer lamp;
        [SerializeField] TextMesh prompt;
        [SerializeField] AudioSource runningAudio;
        [SerializeField] BeaconAndMainsLightingGroup lighting;
        [SerializeField] Light[] serviceLights;
        WatchState state;
        Vector3 engineRest;
        float shakeTime;
        bool wasRunning;
        AudioClip engineCatch;
        MaterialPropertyBlock lampBlock;
        public GeneratorRestart Starter => starter;
        // Primer/choke readiness will be added here in a later milestone, not enabled in M2.
        public bool RequiresPrimerAndChoke => false;
        public void Bind(WatchState value)
        {
            if (state != null) state.Changed -= Refresh;
            state = value; state.Changed += Refresh; ResetInteraction();
        }
        void Awake()
        {
            engineRest = engine.localPosition; lampBlock = new MaterialPropertyBlock();
            starter.PullCompleted += Pulled;
            engineCatch=Resources.Load<AudioClip>("Hands/engine_catch");
        }
        public void ResetInteraction() { starter.ResetInteraction(); shakeTime = 0; engine.localPosition = engineRest; Refresh(); }
        void Pulled()
        {
            if (state == null || !state.StarterPulled()) return;
            shakeTime=.22f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"WATCH_STARTER day={state.Day} pulls={state.StarterPulls} power={state.Power}");
#endif
        }
        void Refresh()
        {
            bool running = state.Power == PowerState.On;
            starter.PullEnabled = !running && !state.Failed;
            lighting.Powered = running;
            foreach (var light in serviceLights) light.intensity = running ? 1.2f : .22f;
            lampBlock.SetColor("_BaseColor", running ? new Color(.1f,.7f,.12f) : new Color(.2f,.055f,.025f));
            lampBlock.SetColor("_EmissionColor", running ? new Color(.08f,.9f,.1f) : Color.black);
            lamp.SetPropertyBlock(lampBlock);
            prompt.text = running ? "POWER ON" : state.Failed ? "WATCH ENDED" : $"PULL STARTER  {state.StarterPulls}/{WatchState.RequiredStarterPulls}\nGRAB + PULL TOWARD YOU";
            if (running && !wasRunning) { runningAudio.Play(); if(engineCatch)runningAudio.PlayOneShot(engineCatch,.7f); }
            else if (!running) runningAudio.Stop();
            wasRunning = running;
        }
        void Update()
        {
            if (shakeTime <= 0) return;
            shakeTime = Mathf.Max(0, shakeTime - Time.deltaTime);
            engine.localPosition = engineRest + Vector3.right * (Mathf.Sin(shakeTime * 140) * .003f * (shakeTime / .22f));
        }
        void OnDestroy()
        {
            if (state != null) state.Changed -= Refresh;
            if (starter) starter.PullCompleted -= Pulled;
        }
    }
}
