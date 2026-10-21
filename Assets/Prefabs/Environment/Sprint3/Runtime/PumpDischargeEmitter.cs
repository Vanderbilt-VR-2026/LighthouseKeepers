using UnityEngine;

namespace LighthouseKeepers.Environment.Sprint3
{
    /// <summary>A calls EmitStroke after accepting a stroke; no simulation or input here.</summary>
    public sealed class PumpDischargeEmitter : MonoBehaviour
    {
        [SerializeField] Material waterMaterial;
        [SerializeField] ParticleSystem surge;
        public ParticleSystem Surge { get { Prepare(); return surge; } }
        public void Prepare()
        {
            if (surge) return;
            var outlet = transform.Find("VFX_PumpDischarge");
            if (!outlet) return;
            surge = outlet.gameObject.AddComponent<ParticleSystem>();
            surge.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = surge.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = .32f;
            main.startSpeed = 1.1f;
            main.startSize = .018f;
            main.maxParticles = 40;
            main.gravityModifier = .7f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(.4f, .65f, .75f, .7f);
            var emission = surge.emission;
            emission.rateOverTime = 0;
            var shape = surge.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 9;
            shape.radius = .025f;
            surge.GetComponent<ParticleSystemRenderer>().sharedMaterial = waterMaterial;
        }
        public void EmitStroke() { Prepare(); if (surge) surge.Emit(24); }
        void Awake() => Prepare();
    }
}
