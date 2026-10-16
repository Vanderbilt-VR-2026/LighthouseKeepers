using LighthouseKeepers.Networking;
using UnityEngine;

/// <summary>Preserves William's button entry points while sharing the lobby session lifecycle.</summary>
public class FusionLauncher : MonoBehaviour
{
    [SerializeField] FusionLobbyTransport transport;
    [SerializeField] string roomName = "LighthouseKeepersRoom";
    [SerializeField, Range(2, 8)] int maxPlayers = 4;

    FusionLobbyTransport Transport
    {
        get
        {
            if (!transport) transport = FindAnyObjectByType<FusionLobbyTransport>();
            if (!transport) transport = new GameObject("Fusion crew session").AddComponent<FusionLobbyTransport>();
            transport.SessionName = roomName;
            return transport;
        }
    }
    public void StartHost() => Transport.HostGame(maxPlayers);
    public void JoinSession() => Transport.JoinGame(roomName);
    public void LeaveSession() => Transport.LeaveGame();
}
