using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
namespace LighthouseKeepers.Net
{
    /// <summary>XRI selection remains optimistic until the host grants the lease; denial explicitly detaches the losing hand.</summary>
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
        public int Id { get; private set; }
        public string Path { get; private set; }
        public int Epoch { get; private set; }
        public int Owner { get; private set; }
        public Vector3 AuthoritativePosition => hasState?targetPosition:transform.position;
        public bool canProcess => isActiveAndEnabled;
        bool Selected => interaction && interaction.interactorsSelecting.Any(i=>!(i is XRSocketInteractor));
        public void Bind(FusionGrabAuthority host,int id,string path,XRBaseInteractable interactable)
        {
            authority=host;Id=id;Path=path;interaction=interactable;body=GetComponent<Rigidbody>();initialKinematic=body&&body.isKinematic;
            legacyWrench=GetComponent<LighthouseKeepers.StormWatch.WatchWrench>();
            legacyControl=GetComponent<LighthouseKeepers.StormWatch.WatchControl>();
            wrenchWasEnabled=legacyWrench && legacyWrench.enabled;controlWasEnabled=legacyControl && legacyControl.enabled;
            targetPosition=transform.position;targetRotation=transform.rotation;
            interaction.selectFilters.Add(this);interaction.selectEntered.AddListener(Grab);interaction.selectExited.AddListener(Release);
            PhysicsOwner();
        }
        public bool Process(IXRSelectInteractor interactor,IXRSelectInteractable value)
        {
            if (!authority || authority.LocalActor<=0) return false;
            if (interactor is XRSocketInteractor) return authority.IsHost && Owner==0;
            return Owner==0 || Owner==authority.LocalActor;
        }
        void Grab(SelectEnterEventArgs args)
        {
            if (suppress || args.interactorObject is XRSocketInteractor || Owner==authority.LocalActor) return;
            pending=true;requested=Time.unscaledTime;PhysicsOwner();authority.Request(this,0,++serial);
        }
        void Release(SelectExitEventArgs args)
        {
            if (suppress || Selected) return;
            pending=false;
            if (Owner==authority.LocalActor) authority.Request(this,2,++serial);
        }
        public void Reject(int expectedEpoch=-1)
        {
            if (expectedEpoch>=0 && expectedEpoch<Epoch) return;
            pending=false;CancelSelection();PhysicsOwner();
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
                if (!Selected) authority.Request(this,2,++serial); // released during the round trip
            }
            else if (Owner!=0 || !pending) { pending=false;CancelSelection(); }
            PhysicsOwner();
        }
        public void ApplyHeld(Vector3 position,Quaternion rotation)
        {
            if (body) body.isKinematic=true;
            transform.SetPositionAndRotation(position,rotation.normalized);
        }
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
