using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LighthouseKeepers.Gameplay.Interaction
{
    public static class StationInteractionBinding
    {
        static Material surgeMaterial;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetStatics() => surgeMaterial = null;
        public static void BindLoadedScenes()
        {
            var leak = GameObject.Find("STN_PIPE_LEAK");
            if (leak) foreach (var wrench in Object.FindObjectsByType<WrenchInteraction>(FindObjectsSortMode.None)) wrench.BindLeak(leak.transform);
            var settings = Resources.Load<InteractionSettings>("Hands/InteractionSettings");
            var pump = GameObject.Find("STN_MANUAL_PUMP");
            if (pump && !pump.GetComponentInChildren<ConstrainedArc>(true))
            {
                var grip = Find(pump.transform, "GRIP_PumpHandle"); var pivot = Find(pump.transform, "PIVOT_PumpHandle");
                if (grip && pivot) AddArc(grip, pivot, ConstrainedArc.Purpose.ManualPump, settings, Discharge(pump.transform));
            }
            var generator = GameObject.Find("STN_GENERATOR");
            if (generator && !generator.GetComponentInChildren<GeneratorRestart>(true))
            {
                var primerGrip = Find(generator.transform, "GRIP_PrimerLever");
                var chokeGrip = Find(generator.transform, "GRIP_ChokeLever");
                var starterGrip = Find(generator.transform, "GRIP_StarterHandle");
                if (!primerGrip || !chokeGrip || !starterGrip) return;
                var primer = AddArc(primerGrip, Pivot(generator.transform, primerGrip, "Primer interaction axis"), ConstrainedArc.Purpose.Primer, settings);
                var choke = AddArc(chokeGrip, Pivot(generator.transform, chokeGrip, "Choke interaction axis"), ConstrainedArc.Purpose.Choke, settings);
                Prepare(starterGrip);
                var restart = starterGrip.gameObject.AddComponent<GeneratorRestart>();
                restart.Configure(primer, choke, starterGrip);
            }
        }
        public static Transform Find(Transform root, string exactName)
        { foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == exactName) return t; return null; }
        static Transform Pivot(Transform root, Transform grip, string name)
        {
            var pivot = new GameObject(name).transform; pivot.SetParent(root, false);
            pivot.position = grip.position - root.forward * .2f; return pivot;
        }
        static ConstrainedArc AddArc(Transform grip, Transform pivot, ConstrainedArc.Purpose verb, InteractionSettings settings, ParticleSystem surge = null)
        {
            Prepare(grip);
            var arc = grip.gameObject.AddComponent<ConstrainedArc>(); arc.Configure(pivot, grip, verb, settings, surge); return arc;
        }
        static void Prepare(Transform grip)
        {
            // Collider-only interaction volumes; no rendered helper primitives.
            var collider = grip.GetComponent<Collider>();
            if (!collider) { var sphere = grip.gameObject.AddComponent<SphereCollider>(); sphere.radius = .09f; }
            if (!grip.GetComponent<XRSimpleInteractable>()) grip.gameObject.AddComponent<XRSimpleInteractable>();
            if (!grip.GetComponent<PhysicalGrip>()) grip.gameObject.AddComponent<PhysicalGrip>();
            grip.GetComponent<PhysicalGrip>().Configure(grip, HandPose.PumpHandle);
        }
        static ParticleSystem Discharge(Transform pump)
        {
            var outlet = Find(pump, "DISCHARGE_ManualPump");
            if (!outlet)
            {
                Debug.LogWarning("Manual pump has no DISCHARGE_ManualPump; C must place its outlet before water-surge acceptance.", pump);
                return null;
            }
            var effect = new GameObject("Stroke water surge").AddComponent<ParticleSystem>(); effect.transform.SetParent(outlet, false);
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = effect.main; main.playOnAwake = false; main.loop = true; main.startLifetime = .3f;
            main.startSize = .025f; main.startSpeed = .7f; main.startColor = new Color(.30f, .65f, .8f, .75f); main.gravityModifier = .5f; main.maxParticles = 96;
            var emission = effect.emission; emission.enabled = false;
            var shape = effect.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 8; shape.radius = .018f;
            if (!surgeMaterial) surgeMaterial = Resources.Load<Material>("Hands/M_WaterSurge");
            effect.GetComponent<ParticleSystemRenderer>().sharedMaterial = surgeMaterial;
            effect.Play();
            return effect;
        }
    }
}
