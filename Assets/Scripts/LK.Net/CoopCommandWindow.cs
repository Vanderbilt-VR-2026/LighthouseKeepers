using System.Collections.Generic;
using LighthouseKeepers.Lobby;
namespace LighthouseKeepers.Net
{
    // The channel authenticates identity; this window rejects stale/replayed day input.
    public sealed class CoopCommandWindow
    {
        readonly Dictionary<int,int> sequences=new();
        public bool Accept(int actor,int sequence,int revision,int currentRevision,CoopAction action,float value)
        {
            if(actor<=0||sequence<=0||revision!=currentRevision||!float.IsFinite(value)||
                action<CoopAction.Starter||action>CoopAction.BeaconAlign||
                sequences.TryGetValue(actor,out int old)&&sequence<=old)return false;
            sequences[actor]=sequence;return true;
        }
        public void Remove(int actor)=>sequences.Remove(actor);
    }
}
