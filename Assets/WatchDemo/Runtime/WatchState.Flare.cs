using LighthouseKeepers.Core.Progression;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        public FlareState Flares { get; private set; }
        FlareState NewFlares(DayDefinition day) => !Survival && Master && Sprint5 && Sprint5.flareDefense && day.scenario?.configured == true && !day.scenario.noFail ? new FlareState { ReadOnly = ReadOnly } : null;
        public bool FireFlare(int slot, Vector3 muzzle, Vector3 forward)
        {
            if (ReadOnly || Failed || Complete || Flares == null || !Flares.Fire(slot, muzzle, forward, Elapsed)) return false;
            Changed?.Invoke(); return true;
        }
        public bool RepelWithFlare()
        {
            if (ReadOnly || Failed || Drowned == null || !Drowned.Repel(Elapsed)) return false;
            TickTension(); Storm?.Tick(); Changed?.Invoke(); return true;
        }
    }
}
