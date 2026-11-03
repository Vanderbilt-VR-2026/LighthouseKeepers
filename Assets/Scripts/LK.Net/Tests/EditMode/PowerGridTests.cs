using LighthouseKeepers.Core;
using NUnit.Framework;

namespace LighthouseKeepers.Net.Tests
{
    public sealed class PowerGridTests
    {
        static PowerGrid Grid()=>new PowerGrid(1,1,1,.25f);
        [Test] public void TwoBigLoadsAndLightsFitButThirdTripsEveryCircuitUntilSafeReset()
        {
            var grid=Grid();grid.Reset(2.5f,11,true);
            Assert.AreEqual(2.25f,grid.Demand);Assert.IsTrue(grid.Powered(PowerCircuit.Beacon));
            Assert.IsTrue(grid.SetCircuit(PowerCircuit.Radio,true));Assert.IsTrue(grid.Tripped);
            foreach(PowerCircuit c in System.Enum.GetValues(typeof(PowerCircuit)))Assert.IsFalse(grid.Powered(c));
            Assert.IsTrue(grid.EngineRunning,"A breaker trip must not stop the engine");
            Assert.IsFalse(grid.ResetBreaker(),"Overloaded reset must not energize the circuits");
            grid.SetCircuit(PowerCircuit.Pump,false);Assert.IsTrue(grid.Tripped);
            Assert.IsTrue(grid.ResetBreaker());Assert.IsTrue(grid.Powered(PowerCircuit.Radio));
            Assert.IsFalse(grid.Powered(PowerCircuit.Pump));Assert.IsTrue(grid.Powered(PowerCircuit.Beacon));
        }
        [Test] public void DayOneCapacityPowersAllLoadsAndEngineRestartCannotClearTrippedBreaker()
        {
            var grid=Grid();grid.Reset(3.25f,15,true);
            Assert.IsFalse(grid.Tripped);foreach(PowerCircuit c in System.Enum.GetValues(typeof(PowerCircuit)))Assert.IsTrue(grid.Powered(c));
            grid.Reset(2.5f,15,false);Assert.IsFalse(grid.Tripped);
            grid.SetEngine(true);Assert.IsTrue(grid.Tripped);grid.SetEngine(false);grid.SetCircuit(PowerCircuit.Radio,false);
            grid.SetEngine(true);Assert.IsTrue(grid.Tripped);Assert.IsTrue(grid.ResetBreaker());
        }
        [Test] public void ReplicaRejectsLocalCommandsAndImpossibleOrNonfiniteHostStates()
        {
            var grid=Grid();grid.Reset(2.5f,11,true);grid.ReadOnly=true;
            Assert.IsFalse(grid.SetCircuit(PowerCircuit.Pump,false));grid.SetEngine(false);Assert.IsTrue(grid.EngineRunning);
            Assert.IsFalse(grid.ApplyReplica(float.NaN,11,true,false));Assert.IsFalse(grid.ApplyReplica(2.5f,16,true,false));
            Assert.IsFalse(grid.ApplyReplica(2.5f,15,true,false));Assert.AreEqual(11,grid.SelectedMask);
            Assert.IsTrue(grid.ApplyReplica(2.5f,15,true,true));Assert.IsFalse(grid.ResetBreaker());
            Assert.IsTrue(grid.ApplyReplica(2.5f,14,true,false));Assert.IsTrue(grid.Powered(PowerCircuit.Radio));
        }
    }
}
