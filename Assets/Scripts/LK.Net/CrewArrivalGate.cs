using System.Collections.Generic;
namespace LighthouseKeepers.Net
{
    // Initial roster only. Leaving cannot strand a reader; a late join never pauses an active watch.
    public sealed class CrewArrivalGate
    {
        readonly HashSet<int> acknowledged=new();
        public bool Released {get;private set;}
        public bool Acknowledge(int actor,IEnumerable<int> present)
        {
            if(actor<=0)return false;
            foreach(int id in present)if(id==actor){acknowledged.Add(actor);return true;}
            return false;
        }
        public bool Hold(IEnumerable<int> present,bool transportReady)
        {
            if(Released)return false;
            if(!transportReady)return true;
            bool any=false;
            foreach(int id in present){any=true;if(!acknowledged.Contains(id))return true;}
            if(any)Released=true;
            return !Released;
        }
    }
}
