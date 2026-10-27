using Fusion;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    public static class NetworkPrefabRegistry
    {
        public static NetworkObjectGuid PlayerGuid => NetworkObjectGuid.Parse("0fb3413dd54f47098c7a38bce022d6b1");
        public static NetworkObjectGuid PropGuid => NetworkObjectGuid.Parse("0fb3413dd54f47098c7a38bce022d6b2");
        public static NetworkObject Player { get; private set; }
        public static NetworkObject PropAuthority { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            Player = Add("PF_PlayerRig", "0fb3413dd54f47098c7a38bce022d6b1");
            PropAuthority = Add("PF_GrabAuthority", "0fb3413dd54f47098c7a38bce022d6b2");
        }
        static NetworkObject Add(string name, string guid)
        {
            var prefab = Resources.Load<NetworkObject>(name);
            if (!prefab) { Debug.LogWarning("Sprint 3 network prefab missing: " + name); return null; }
            NetworkProjectConfig.Global.PrefabTable.TryAddSource(new NetworkPrefabSourceStatic
                { Object = prefab, AssetGuid = NetworkObjectGuid.Parse(guid), AssetPath = "Assets/Prefabs/Network/Resources/" + name + ".prefab" }, out _);
            return prefab;
        }
    }
}
