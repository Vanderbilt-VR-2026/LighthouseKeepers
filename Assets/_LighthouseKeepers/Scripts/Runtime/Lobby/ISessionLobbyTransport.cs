using System;

namespace LighthouseKeepers.Lobby
{
    /// <summary>Optional capabilities of a real, asynchronous lobby transport.</summary>
    public interface ISessionLobbyTransport : INetworkTransport
    {
        bool IsBusy { get; }
        bool IsStarting { get; }
        int MaxPlayers { get; }
        string LastError { get; }
        string SessionName { get; set; }
        event Action StatusChanged;
        event Action GameStarting;
        bool StartGame(string sceneName, int minimumPlayers, bool requireAllReady);
    }
}
