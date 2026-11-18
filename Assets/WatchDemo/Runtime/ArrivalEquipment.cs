using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Personal kit reminders use the same objective presentation. Shared station progress
    // remains host-owned; one player taking a radio never dismisses the other's reminder.
    public sealed class ArrivalEquipment:MonoBehaviour
    {
        WatchDemoController watch;HandheldCrewRadio[] radios;WatchLantern[] lanterns;
        bool radioTaken,lanternTaken,lit;int slot=-1;
        public KeeperStep Step=>watch.State.Day!=1||watch.State.Failed||watch.State.Recovery.AnyDown?KeeperStep.None:
            !radioTaken?KeeperStep.GrabWalkie:!lanternTaken?KeeperStep.GrabLantern:!lit?KeeperStep.LanternWick:KeeperStep.None;
        public Transform Target {
            get {
                if(Step==KeeperStep.GrabWalkie)foreach(var r in radios)if(r.Available)return r.transform;
                if(Step==KeeperStep.GrabLantern)foreach(var l in lanterns)if(l&&!l.Held)return l.transform;
                return slot>=0&&lanterns[slot]?lanterns[slot].transform:null;
            }
        }
        public static ArrivalEquipment Create(WatchDemoController w){var c=w.gameObject.AddComponent<ArrivalEquipment>();c.watch=w;c.radios=w.GetComponentsInChildren<HandheldCrewRadio>();c.lanterns=new[]{w.Lantern(0),w.Lantern(1)};return c;}
        void Update(){
            foreach(var r in radios)if(r.Held||r.Carried)radioTaken=true;
            for(int i=0;i<lanterns.Length;i++)if(lanterns[i]&&lanterns[i].Grab.isSelected){lanternTaken=true;slot=i;}
            if(slot>=0&&watch.State.Lanterns?.Lit(slot)==true)lit=true;
        }
    }
}
