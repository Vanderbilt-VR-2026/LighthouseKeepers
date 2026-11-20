using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Personal kit reminders use the same objective presentation. Shared station progress
    // remains host-owned; one player taking a radio never dismisses the other's reminder.
    public sealed class ArrivalEquipment:MonoBehaviour
    {
        WatchDemoController watch;HandheldCrewRadio[] radios;
        bool radioTaken;
        public KeeperStep Step=>watch.State.Day!=1||watch.State.Failed||watch.State.Recovery.AnyDown?KeeperStep.None:
            !radioTaken?KeeperStep.GrabWalkie:KeeperStep.None;
        public Transform Target {
            get {
                if(Step==KeeperStep.GrabWalkie)foreach(var r in radios)if(r.Available)return r.transform;
                return null;
            }
        }
        public static ArrivalEquipment Create(WatchDemoController w){var c=w.gameObject.AddComponent<ArrivalEquipment>();c.watch=w;c.radios=w.GetComponentsInChildren<HandheldCrewRadio>();return c;}
        void Update(){
            foreach(var r in radios)if(r.Held||r.Carried)radioTaken=true;
        }
    }
}
