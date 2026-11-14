using LighthouseKeepers.Core.Progression;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchState
    {
        public ShipPassageState Ships { get; private set; }
        ShipPassageState NewShips(DayDefinition day) => Sprint5 && Sprint5.shipPassages && day.scenario?.configured == true
            ? new ShipPassageState(day.scenario.ships,Sprint5.shipApproachDangerSeconds,Sprint5.shipWheelTracking,Sprint5.shipAimSteadySeconds) { ReadOnly = ReadOnly } : null;
        public int AimingShip => Ships!=null&&Sprint5&&Sprint5.shipWheelTracking?Ships.TrackingShip(Elapsed):-1;
        public bool BeamTrackingMode => Ships!=null&&Sprint5&&Sprint5.shipWheelTracking&&(AimingShip>=0||Ships.PassedMask!=0&&!Tutorial);
        public bool ShipGuidanceDue => AimingShip>=0&&Elapsed>=Ships.WindowStart(AimingShip)-Sprint5.shipGuidanceLeadSeconds;
        public bool BeamEnergized => Beacon!=null&&HasPower(LighthouseKeepers.Core.PowerCircuit.Beacon)&&Beacon.Ignited&&Beacon.Soot<=Sprint5.shipMaximumSoot;
        public UnityEngine.Vector3 ShipPoint(int i,double now) => ShipNavigation.Route((float)Ships.VisualAge(i,now),(float)Ships.Approach(i),Scenario.ships[i].windowSeconds,55).position+UnityEngine.Vector3.up*4;
        public float ShipWheelTarget => AimingShip<0?Beacon.Target:ShipNavigation.WheelFor(ShipPoint(AimingShip,Elapsed));
        public bool ShipOnBeam => AimingShip>=0&&IlluminatesShip(AimingShip,Elapsed);
        bool IlluminatesShip(int i,double now) => BeamEnergized&&ShipNavigation.Aimed(Beacon.Alignment,ShipPoint(i,now),Sprint5.shipAimToleranceDegrees);
        bool ValidShipSnapshot(DayDefinition day, WatchSnapshot snapshot)
        {
            var model = NewShips(day);
            return model == null ? !snapshot.ships.enabled : model.ValidReplica(snapshot.ships, snapshot.elapsed);
        }
        void TickShips(double from)
        {
            if (Ships == null || ReadOnly) return;
            int previous = Ships.PassedMask;
            bool changed = Sprint5.shipWheelTracking?Ships.TickGuided(from,Elapsed,IlluminatesShip,Radio.Completed):Ships.Tick(from, Elapsed, BeaconShining, Radio.Completed);
            if (Ships.PassedMask != previous) Tension?.Fixed(Elapsed);
            SetShipFear(Ships.AnyWindow(Elapsed) ? .35f + Ships.Danger * .65f : 0);
            if (changed) Changed?.Invoke();
        }
    }
}
