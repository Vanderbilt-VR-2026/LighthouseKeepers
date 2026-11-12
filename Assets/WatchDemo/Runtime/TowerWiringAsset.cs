using System;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Generated from the final Lab preview. No simulation state is stored here.
    public sealed class TowerWiringAsset : ScriptableObject
    {
        [Serializable] public struct Run { public string name; public int circuit,floor; public float radius; public Vector3[] points; }
        [Serializable] public struct Endpoint { public string name; public int circuit; public Vector3 position; }
        public Material material;
        public Mesh[] floors;
        public Run[] runs;
        public Endpoint[] endpoints;
        public Vector3[] junctions;
        public Vector3 breaker;
        public int triangles;
        public float pulseSeconds=1.5f;
    }
}
