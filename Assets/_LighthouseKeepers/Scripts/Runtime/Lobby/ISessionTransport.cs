using System;

namespace LighthouseKeepers.Lobby
{
    /// <summary>Async connection status and a launch that reaches every member of the crew.</summary>
    public interface ISessionTransport : INetworkTransport
    {
        bool IsConnecting { get; }
        string LastError { get; }
        string RoomName { get; set; }
        event Action GameStarting;
        bool BeginGame(string sceneName, bool requireAllReady = true);
    }

    /// <summary>Allows the installed networking backend to serve the lobby without a circular assembly reference.</summary>
    public static class LobbyTransportProvider
    {
        public static Func<INetworkTransport> Create;
    }
}
