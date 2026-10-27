using UnityEngine;

namespace LighthouseKeepers.Environment.Sprint3
{
    public sealed class ShipStormPresentation : MonoBehaviour
    {
        public static float LensWetness { get; private set; }
        [SerializeField] ParticleSystem rain;
        [SerializeField] Light lightning;
        [SerializeField] Collider[] shelterVolumes;
        [SerializeField] AudioSource wind, waves, thunder;
        Transform head;
        float nextStrike, flashStart = -100, thunderAt = -1;
        ParticleSystem.Particle[] particles;
        static readonly int Wetness = Shader.PropertyToID("_ShipLensWetness");
        void Awake()
        {
            particles = new ParticleSystem.Particle[rain.main.maxParticles];
            nextStrike = Time.time + Random.Range(20f, 40f);
        }
        public bool Outdoors(Vector3 point)
        {
            foreach (var volume in shelterVolumes)
                if (volume && volume.bounds.Contains(point) && (volume.ClosestPoint(point) - point).sqrMagnitude < .00001f) return false;
            return true;
        }
        void LateUpdate()
        {
            if (!head && Camera.main) head = Camera.main.transform;
            if (!head) return;
            rain.transform.position = head.position + Vector3.up * 7;
            // Roof volumes keep droplets off interior floors while exterior rain remains visible at windows.
            int count = rain.GetParticles(particles);
            for (int i = 0; i < count; i++) if (!Outdoors(particles[i].position)) particles[i].remainingLifetime = 0;
            rain.SetParticles(particles, count);
            bool outside = Outdoors(head.position);
            LensWetness = Mathf.MoveTowards(LensWetness, outside ? 1 : 0, Time.deltaTime / 1.5f);
            Shader.SetGlobalFloat(Wetness, LensWetness);
            if (wind) wind.volume = Mathf.MoveTowards(wind.volume, outside ? .32f : .08f, Time.deltaTime * .2f);
            if (waves) waves.volume = Mathf.MoveTowards(waves.volume, outside ? .3f : .09f, Time.deltaTime * .2f);
            if (Time.time >= nextStrike)
            { flashStart = Time.time; nextStrike = Time.time + Random.Range(20f, 40f); thunderAt = Time.time + Random.Range(1.5f, 3.5f); }
            // One smooth pulse, no secondary flashes or strobing.
            float age = Time.time - flashStart;
            if (lightning) lightning.intensity = age < .8f ? Mathf.Sin(age / .8f * Mathf.PI) * .65f : 0;
            if (thunderAt > 0 && Time.time >= thunderAt) { if (thunder) thunder.Play(); thunderAt = -1; }
        }
        void OnDisable() { LensWetness = 0; Shader.SetGlobalFloat(Wetness, 0); }
    }
}
