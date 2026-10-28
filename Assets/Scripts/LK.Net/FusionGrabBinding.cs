using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
namespace LighthouseKeepers.Net
{
    public interface INetworkPropRelease
    {
        void OnNetworkReleased();
    }
    /// <summary>XRI selection remains optimistic until the host grants the lease; denial explicitly detaches the losing hand.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class FusionGrabBinding : MonoBehaviour, IXRSelectFilter
    {
        FusionGrabAuthority authority;
        XRBaseInteractable interaction;
        Rigidbody body;
        LighthouseKeepers.StormWatch.WatchWrench legacyWrench;
        LighthouseKeepers.StormWatch.WatchControl legacyControl;
        bool wrenchWasEnabled, controlWasEnabled;
        bool initialKinematic, pending, suppress, hasState;
        float requested, nextSend;
        int serial, receivedSequence;
        Vector3 targetPosition;
        Quaternion targetRotation;
        Vector3 previousPosition;
        Quaternion previousRotation;
        bool releaseQueued;
        readonly List<INetworkPropRelease> releaseObservers=new();
        public Vector3 ReleaseVelocity { get; private set; }
        public Vector3 ReleaseAngularVelocity { get; private set; }
        public bool IsRemotelyOwned => authority && Owner!=0 && Owner!=authority.LocalActor;
        public bool CanSimulateReleased => !authority || authority.IsHost && Owner==0;
        public int Id { get; private set; }
        public string Path { get; private set; }
        public int Epoch { get; private set; }
        public int Owner { get; private set; }
        public Vector3 AuthoritativePosition => hasState?targetPosition:transform.position;
        public bool canProcess => isActiveAndEnabled;
        bool Selected => interaction && interaction.interactorsSelecting.Any(i=>!(i is XRSocketInteractor));
        public void Bind(FusionGrabAuthority host,int id,string path,XRBaseInteractable interactable)
        {
            authority=host;Id=id;Path=path;interaction=interactable;body=GetComponent<Rigidbody>();
            // A grip held during scene loading must not survive without a host lease.
            // Let XRI restore the pre-grab body mode before recording that mode.
            CancelSelection();initialKinematic=body&&body.isKinematic;
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is INetworkPropRelease observer)releaseObservers.Add(observer);
            legacyWrench=GetComponent<LighthouseKeepers.StormWatch.WatchWrench>();
            legacyControl=GetComponent<LighthouseKeepers.StormWatch.WatchControl>();
            wrenchWasEnabled=legacyWrench && legacyWrench.enabled;controlWasEnabled=legacyControl && legacyControl.enabled;
            targetPosition=transform.position;targetRotation=transform.rotation;
            previousPosition=transform.position;previousRotation=transform.rotation;
            interaction.selectFilters.Add(this);interaction.selectEntered.AddListener(Grab);interaction.selectExited.AddListener(Release);
            PhysicsOwner();
        }
        public bool Process(IXRSelectInteractor interactor,IXRSelectInteractable value)
        {
            if (!authority || !authority.CanInteract || authority.LocalActor<=0) return false;
            if (interactor is XRSocketInteractor) return authority.IsHost && Owner==0;
            return Owner==0 || Owner==authority.LocalActor;
        }
        void Grab(SelectEnterEventArgs args)
        {
            if (suppress || args.interactorObject is XRSocketInteractor || Owner==authority.LocalActor) return;
            pending=true;requested=Time.unscaledTime;PhysicsOwner();authority.Request(this,0,++serial);
            previousPosition=transform.position;previousRotation=transform.rotation;
            ReleaseVelocity=ReleaseAngularVelocity=Vector3.zero;
        }
        void Release(SelectExitEventArgs args)
        {
            if (suppress || Selected) return;
            pending=false;
            if (Owner==authority.LocalActor) releaseQueued=true;
        }
        public void Reject(int expectedEpoch=-1)
        {
            if (expectedEpoch>=0 && expectedEpoch<Epoch) return;
            pending=releaseQueued=false;CancelSelection();PhysicsOwner();
        }
        void CancelSelection()
        {
            if (!interaction || !interaction.interactionManager) return;
            suppress=true;
            try { foreach (var hand in interaction.interactorsSelecting.ToArray()) interaction.interactionManager.SelectExit(hand,interaction); }
            finally { suppress=false; }
        }
        public void ApplyState(FusionGrabAuthority.Packet p)
        {
            if (p.epoch<Epoch || p.epoch==Epoch && p.sequence<=receivedSequence) return;
            bool changed=p.epoch!=Epoch;
            Owner=p.owner;Epoch=p.epoch;receivedSequence=p.sequence;targetPosition=p.position;targetRotation=p.rotation;hasState=true;
            if (changed) serial=0;
            if (Owner==authority.LocalActor)
            {
                pending=false;
                if (!Selected) releaseQueued=true; // released during the round trip
            }
            else if (Owner!=0 || !pending) { pending=false;CancelSelection(); }
            PhysicsOwner();
        }
        public void ApplyHeld(Vector3 position,Quaternion rotation)
        {
            if (body) body.isKinematic=true;
            transform.SetPositionAndRotation(position,rotation.normalized);
        }
        public void ReleaseOnHost(Vector3 position,Quaternion rotation,Vector3 velocity,Vector3 angularVelocity)
        {
            transform.SetPositionAndRotation(position,rotation.normalized);
            if (!body) return;
            body.position=position;body.rotation=rotation.normalized;body.isKinematic=initialKinematic;
            if (!body.isKinematic)
            {
                body.linearVelocity=Vector3.ClampMagnitude(velocity,10);
                body.angularVelocity=Vector3.ClampMagnitude(angularVelocity,20);
            }
            foreach(var observer in releaseObservers)observer.OnNetworkReleased();
        }
        public void DropOnHost() => ReleaseOnHost(transform.position,transform.rotation,Vector3.zero,Vector3.zero);
        void PhysicsOwner()
        {
            if (!authority) return;
            // The old wrench returns home after eight unselected seconds. Only the host may recover a released wrench.
            if (legacyWrench) legacyWrench.enabled=wrenchWasEnabled && (Owner==authority.LocalActor || Owner==0 && authority.IsHost);
            if (legacyControl) legacyControl.enabled=controlWasEnabled && (!Selected || Owner==authority.LocalActor);
            if (!body || Selected) return;
            body.isKinematic=initialKinematic || !authority.IsHost || Owner!=0 && Owner!=authority.LocalActor;
        }
        void LateUpdate()
        {
            if (!authority) return;
            if (Selected && (pending || Owner==authority.LocalActor))
            {
                float dt=Mathf.Max(.001f,Time.unscaledDeltaTime);
                ReleaseVelocity=Vector3.Lerp(ReleaseVelocity,Vector3.ClampMagnitude((transform.position-previousPosition)/dt,10),.5f);
                var delta=transform.rotation*Quaternion.Inverse(previousRotation);
                delta.ToAngleAxis(out float angle,out var axis);if(angle>180)angle-=360;
                if (KeeperPoseFrame.Finite(axis.x)&&KeeperPoseFrame.Finite(axis.y)&&KeeperPoseFrame.Finite(axis.z)&&KeeperPoseFrame.Finite(angle))
                    ReleaseAngularVelocity=Vector3.Lerp(ReleaseAngularVelocity,Vector3.ClampMagnitude(axis*(angle*Mathf.Deg2Rad/dt),20),.5f);
            }
            previousPosition=transform.position;previousRotation=transform.rotation;
            if (releaseQueued && Owner==authority.LocalActor)
            { releaseQueued=false;authority.Request(this,2,++serial); }
            if (pending && Time.unscaledTime-requested>1.5f) Reject();
            if (Selected && Owner==authority.LocalActor && Time.unscaledTime>=nextSend)
            { nextSend=Time.unscaledTime+.05f;authority.Request(this,1,++serial); }
            if (hasState && !authority.IsHost && !pending && Owner!=authority.LocalActor)
            {
                float blend=1-Mathf.Exp(-25*Time.unscaledDeltaTime);
                transform.SetPositionAndRotation(Vector3.Lerp(transform.position,targetPosition,blend),Quaternion.Slerp(transform.rotation,targetRotation,blend));
            }
        }
        public void Unbind()
        {
            CancelSelection();
            if (interaction) { interaction.selectFilters.Remove(this);interaction.selectEntered.RemoveListener(Grab);interaction.selectExited.RemoveListener(Release); }
            if (body) body.isKinematic=initialKinematic;
            if (legacyWrench) legacyWrench.enabled=wrenchWasEnabled;
            if (legacyControl) legacyControl.enabled=controlWasEnabled;
            authority=null;
        }
        void OnDestroy() => Unbind();
    }
}
