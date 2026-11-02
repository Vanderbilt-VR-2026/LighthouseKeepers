using System.Collections.Generic;
namespace LighthouseKeepers.Net
{
    // Host-owned quorum. Only authenticated, connected actors near the bed may vote.
    public sealed class CoopBedVotes
    {
        readonly HashSet<int> votes=new();
        public int Count=>votes.Count;
        public bool Add(int actor,bool eligible){return actor>0&&eligible&&votes.Add(actor);}
        public void Remove(int actor)=>votes.Remove(actor);
        public void Clear()=>votes.Clear();
        public bool AllReady(IEnumerable<int> connected)
        {
            bool any=false;
            foreach(int actor in connected){any=true;if(!votes.Contains(actor))return false;}
            return any;
        }
    }
}
