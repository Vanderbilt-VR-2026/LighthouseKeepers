using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        public ShipPassageState Ships { get; private set; }
        ShipPassageState NewShips(DayDefinition day) => Sprint5 && Sprint5.shipPassages && day.scenario?.configured == true
            ? new ShipPassageState(day.scenario.ships,Sprint5.shipApproachDangerSeconds) { ReadOnly = ReadOnly } : null;
        bool ValidShipSnapshot(DayDefinition day, WatchSnapshot snapshot)
        {
            var model = NewShips(day);
            return model == null ? !snapshot.ships.enabled : model.ValidReplica(snapshot.ships, snapshot.elapsed);
        }
        void TickShips(double from)
        {
            if (Ships == null || ReadOnly) return;
            int previous = Ships.PassedMask;
            bool changed = Ships.Tick(from, Elapsed, BeaconShining, Radio.Completed);
            if (Ships.PassedMask != previous) Tension?.Fixed(Elapsed);
            SetShipFear(Ships.AnyWindow(Elapsed) ? .35f + Ships.Danger * .65f : 0);
            if (changed) Changed?.Invoke();
        }
    }
}
