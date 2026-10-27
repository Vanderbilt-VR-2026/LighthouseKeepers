using System.Reflection;
using LighthouseKeepers.Core.Simulation;
using UnityEngine;

namespace LighthouseKeepers.Environment.Sprint3
{
    public sealed class ShipBeaconPresentation : MonoBehaviour
    {
        [SerializeField] Renderer lens;
        [SerializeField] GameObject rotatingBeam;
        [SerializeField] Light lantern;
        [SerializeField] float degreesPerSecond = 18;
        ISimulationHost host;
        MaterialPropertyBlock block;
        // B's round-2 field is accepted without cloning or owning simulation state.
        static readonly PropertyInfo LitProperty = typeof(SimSnapshot).GetProperty("BeaconLit");
        static readonly FieldInfo LitField = typeof(SimSnapshot).GetField("BeaconLit");
        float nextBind;
        void Awake() { block = new MaterialPropertyBlock(); Apply(false); }
        void Update()
        {
            if (host == null && Time.time >= nextBind)
            {
                nextBind = Time.time + 1;
                foreach (var behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                    if (behaviour is ISimulationHost candidate) { host = candidate; break; }
            }
            bool lit = false;
            if (host != null)
            {
                try
                {
                    var snapshot = host.Current;
                    lit = LitProperty != null ? (bool)LitProperty.GetValue(snapshot) : LitField != null ? (bool)LitField.GetValue(snapshot) : snapshot.GeneratorRunning && snapshot.PowerAvailable > .5f;
                }
                catch (System.InvalidOperationException) { } // Host initializes after additive geometry loads.
            }
            Apply(lit);
            if (lit) rotatingBeam.transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
        }
        void Apply(bool lit)
        {
            if (rotatingBeam) rotatingBeam.SetActive(lit);
            if (lantern) lantern.enabled = lit;
            if (lens) { block.SetColor("_EmissionColor", lit ? new Color(1, .451f, .141f) * 3 : Color.black); lens.SetPropertyBlock(block); }
        }
    }
}
