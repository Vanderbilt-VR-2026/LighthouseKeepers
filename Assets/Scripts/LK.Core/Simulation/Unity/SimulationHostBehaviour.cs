using System;
using LighthouseKeepers.Core.Simulation;
using UnityEngine;

namespace LighthouseKeepers.Simulation.Unity
{
    /// <summary>Scene-owned driver; never auto-created and never a singleton. Network authority
    /// must be injected before Start through ConfigureAuthority. This component alone advances time.</summary>
    public sealed class SimulationHostBehaviour : MonoBehaviour, ISimulationHost, ISimulationGeometry
    {
        [SerializeField] FloodTuning tuning;
        [SerializeField, Tooltip("C's F1 finished-floor origin, not the water surface.")]
        Transform floorOrigin;
        [SerializeField, Tooltip("C's exact generator plinth TOP transform. No root-Y fallback.")]
        Transform generatorPlinthTop;
        [SerializeField, Tooltip("Enable only for an explicit offline rehearsal. E injects authority in production.")]
        bool useLocalAuthority;
        SimulationHost host;
        IAuthority authority;
        Func<int, bool> repairValidation;
        InteractionEventBridge interactions;
        public bool IsInitialized => host != null;
        public SimSnapshot Current => host != null ? host.Current : throw new InvalidOperationException("Simulation not initialized.");
        public double FloorWorldY => floorOrigin.position.y;
        public event Action<SimSnapshot> OnTick;

        /// <summary>Composition entry for the ship loader. Configure while inactive,
        /// before Start; E supplies authority and D supplies actor-specific repair validation.</summary>
        public void ConfigureScene(FloodTuning profile, Transform floor, Transform plinthTop,
            IAuthority sessionAuthority, Func<int, bool> canRepairPipe)
        {
            if (host != null) throw new InvalidOperationException("Configure the scene before simulation starts.");
            if (!profile || !floor || !plinthTop) throw new ArgumentException("Supply tuning and C's measured floor/plinth transforms.");
            tuning = profile; floorOrigin = floor; generatorPlinthTop = plinthTop;
            ConfigureAuthority(sessionAuthority); ConfigureRepairValidation(canRepairPipe);
        }

        public void ConfigureRepairValidation(Func<int, bool> canRepairPipe)
        { repairValidation = canRepairPipe ?? throw new ArgumentNullException(nameof(canRepairPipe)); }

        public void ConfigureAuthority(IAuthority value)
        {
            if (host != null) throw new InvalidOperationException("Inject authority before simulation starts.");
            authority = value ?? throw new ArgumentNullException(nameof(value));
        }

        void Start()
        {
            if (!tuning || !floorOrigin || (authority == null && !useLocalAuthority))
            {
                Debug.LogError("Simulation requires FloodTuning, C's floor origin and explicit local/network authority.", this);
                enabled = false; return;
            }
            host = new SimulationHost(tuning.ToParameters(), this, authority ?? new LocalAuthority(),
                canRepairPipe: actor => repairValidation != null && repairValidation(actor));
            host.OnTick += ForwardTick;
            interactions = new InteractionEventBridge(this);
            if (!interactions.BindAvailableAssembly())
                Debug.LogWarning("Session A InteractionEvents unavailable. Call RebindInteractionEvents after integration.", this);
        }

        public bool TryGetGeneratorPlinthTopWorldY(out double height)
        { height = generatorPlinthTop ? generatorPlinthTop.position.y : 0; return generatorPlinthTop; }
        public bool RebindInteractionEvents() => interactions != null && interactions.BindAvailableAssembly();
        public void Enqueue(SimCommand cmd)
        {
            if (host == null) throw new InvalidOperationException("Simulation not initialized.");
            host.Enqueue(cmd);
        }
        public void ForceGeneratorFault() => Enqueue(new SimCommand(SimCommandKind.ForceGeneratorFault));
        public void ActivateDayLeak(int leakId) => Enqueue(new SimCommand(SimCommandKind.ActivateDayLeak, targetId: leakId));
        void Update() { if (host != null) host.Advance(Time.deltaTime); }
        void ForwardTick(SimSnapshot snapshot) => OnTick?.Invoke(snapshot);
        void OnEnable() { if (interactions != null) interactions.BindAvailableAssembly(); }
        void OnDisable() { interactions?.Dispose(); }
        void OnDestroy()
        {
            interactions?.Dispose();
            if (host != null) host.OnTick -= ForwardTick;
        }
    }
}
