namespace LighthouseKeepers.Lobby
{
    /// <summary>Optional transport restrictions shown by the board before a launch is attempted.</summary>
    public interface ILobbyLaunchPolicy
    {
        string StartBlockReason(string sceneName);
    }
}
