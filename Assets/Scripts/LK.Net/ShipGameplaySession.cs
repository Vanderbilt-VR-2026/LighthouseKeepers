using System;
using System.Linq;
using System.Reflection;
using LighthouseKeepers.Core.Progression;
using LighthouseKeepers.Core.Simulation;
using LighthouseKeepers.Gameplay.Interaction;
using LighthouseKeepers.Presentation.Board;
using Unity.XR.CoreUtils;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    /// <summary>Composition only: B owns water/power and D's ShipDaySession owns watch rules and the one board.</summary>
    public sealed class ShipGameplaySession : MonoBehaviour
    {
        public DayDirector Days {get;private set;}
        public FusionSimulationAdapter Simulation {get;private set;}
        FusionMessageChannel channel;
        ShipDayReplica replica;
        float nextBind;
        int actor;
        public void Configure(ShipRouteSettings settings,XROrigin rig,bool solo)
        {
            var floor=GameObject.Find("DEPTH_ZERO");var generator=GameObject.Find("STN_GENERATOR");
            if(!floor || !generator)throw new InvalidOperationException("Missing ship anchors: DEPTH_ZERO (F1) or STN_GENERATOR (F2). Run C's ship authoring.");
            var plinth=generator.GetComponentsInChildren<Renderer>(true).FirstOrDefault(r=>r.name=="Concrete plinth");
            if(!plinth)throw new InvalidOperationException("STN_GENERATOR is missing its measured Concrete plinth renderer.");
            if(!settings.days || !settings.tuning)throw new InvalidOperationException("Assign D's DayCatalog and B's FloodTuning in ShipRouteSettings.");
            channel=solo?null:FusionMessageChannel.Active;actor=solo?0:channel.LocalActorId;
            Simulation=gameObject.AddComponent<FusionSimulationAdapter>();
            Simulation.Configure(solo,settings.tuning.ToParameters(),new FixedSimulationGeometry(floor.transform.position.y,plinth.bounds.max.y),id=>Days && Days.CanRepairPipe(id));
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LighthouseKeepers.Presentation.Board.ShipDaySession")).FirstOrDefault(t=>t!=null);
            if(type==null)throw new InvalidOperationException("Merge D's Round2 ShipDaySession before starting a ship watch; no legacy day-flow fallback.");
            var anchor=GameObject.Find("STN_BRIEFING_BOARD");
            var board=anchor?anchor.GetComponentsInChildren<MonoBehaviour>(true).FirstOrDefault(c=>c && c.GetType().Name=="BriefingBoardView"):null;
            if(!board)throw new InvalidOperationException("House requires one C BriefingBoardView under STN_BRIEFING_BOARD.");
            var bridgeRoot=new GameObject("Ship house board callback adapter");bridgeRoot.transform.SetParent(anchor.transform,false);
            var bridge=bridgeRoot.AddComponent<ShipBoardAdapter>();bridge.Bind(board);
            var session=(MonoBehaviour)(gameObject.GetComponent(type)??gameObject.AddComponent(type));
            type.GetMethod("AssignDependencies").Invoke(session,new object[]{settings.days,Simulation,settings.tuning,bridge,rig.Camera.transform});
            bool active=(bool)(solo?type.GetMethod("ActivateSoloAtDay").Invoke(session,new object[]{1,actor}):
                type.GetMethod("ActivateCrewAtDay").Invoke(session,new object[]{Simulation.IsAuthoritative,2,actor,1}));
            if(!active)throw new InvalidOperationException("D could not activate the ship day session. Check its anchor/board diagnostics.");
            Days=(DayDirector)type.GetProperty("Director").GetValue(session);
            gameObject.AddComponent<ShipBedTransition>().Bind(Days,rig,channel);
            Simulation.ValidateInteraction=ValidateRemote;
            gameObject.AddComponent<SnapshotWaterView>().Bind(Simulation);
            if(channel)
            {
                if(!Simulation.IsAuthoritative)
                {
                    replica=gameObject.AddComponent<ShipDayReplica>();replica.Bind(channel);
                    var presenter=(DayBoardPresenter)type.GetProperty("Presenter").GetValue(session);bridge.Projection=replica;presenter.BindReadOnly(replica);replica.Presenter=presenter;
                }
            }
        }
        bool ValidateRemote(SimCommand command)
        {
            var poses=CrewPoseStream.Active;var grabs=channel?channel.GetComponent<FusionGrabAuthority>():null;
            if(!poses || !grabs)return false;
            string station=command.Kind==SimCommandKind.ManualPumpStroke?"STN_MANUAL_PUMP":command.Kind==SimCommandKind.RepairLeak?"STN_PIPE_LEAK":null;
            if(station==null)return false;
            if(command.Kind==SimCommandKind.RepairLeak){if(!grabs.HasFreshWrenchLease(command.ActorId))return false;}
            else if(!grabs.TryStationId(station,out int id) || !grabs.HasFreshLease(command.ActorId,id))return false;
            var point=GameObject.Find(station);if(!point || !poses.NearHand(command.ActorId,point.transform.position,1.5f))return false;
            return command.Kind!=SimCommandKind.RepairLeak || Days.CanRepairPipe(command.ActorId);
        }
        void Update()
        {
            if(Time.unscaledTime<nextBind)return;nextBind=Time.unscaledTime+.2f;
            foreach(var hand in FindObjectsByType<HandRig>(FindObjectsSortMode.None))hand.ActorId=actor;
            if(channel && channel.IsHost && Days)ShipDayReplica.Publish(channel,Days);
        }
    }
}
