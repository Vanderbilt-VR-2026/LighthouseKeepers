using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Bridges authored XRI props to host-controlled leases and physics. Nothing is added to saved scenes.</summary>
[DefaultExecutionOrder(9000)]
public sealed class NetworkGrabbable : MonoBehaviour, IXRSelectFilter
{
    FusionLauncher session;
    XRGrabInteractable grab;
    Rigidbody body;
    bool originalKinematic, suppressEvents, pendingGrab, releasePending, hasState;
    float requestedAt;
    int heldSequence, releaseSentEpoch = -1;
    Vector3 previousPosition, measuredVelocity, measuredAngularVelocity;
    Quaternion previousRotation;
    public int Id { get; private set; }
    public PropPose State { get; private set; }
    public Vector3 AuthoritativePosition => hasState ? State.Position : body.position;
    public int LastOwnerSequence { get; set; }
    public bool canProcess => isActiveAndEnabled;
    bool HandSelected => grab && grab.interactorsSelecting.Any(i => !(i is XRSocketInteractor));
    public bool IsLocallyHeld => session && session.CanInteract && State.Owner == session.LocalOwner && HandSelected;

    public void Configure(FusionLauncher owner, int id, XRGrabInteractable interactable)
    {
        session = owner; Id = id; grab = interactable; body = GetComponent<Rigidbody>();
        originalKinematic = body.isKinematic;
        previousPosition = body.position; previousRotation = body.rotation;
        State = new PropPose { Position = body.position, Rotation = body.rotation, Sequence = -1 };
        grab.selectFilters.Add(this);
        grab.selectEntered.AddListener(Selected);
        grab.selectExited.AddListener(Deselected);
    }
    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (!session || !session.IsOnline) return true;
        if (!session.CanInteract) return false;
        if (interactor is XRSocketInteractor) return session.IsHost && State.Owner == 0;
        return State.Owner == 0 || State.Owner == session.LocalOwner;
    }
    void Selected(SelectEnterEventArgs args)
    {
        if (suppressEvents || args.interactorObject is XRSocketInteractor || !session.IsOnline || IsLocallyHeld) return;
        pendingGrab = true; requestedAt = Time.unscaledTime;
        session.RequestGrab(this);
    }
    void Deselected(SelectExitEventArgs args)
    {
        if (suppressEvents || HandSelected || !session.IsOnline) return;
        pendingGrab = false;
        if (State.Owner == session.LocalOwner) releasePending = true;
    }
    public void DenyGrab()
    {
        pendingGrab = releasePending = false;
        CancelSelections(false);
        RefreshPhysics();
    }
    void CancelSelections(bool socketsOnly)
    {
        if (!grab || !grab.interactionManager) return;
        suppressEvents = true;
        try
        {
            foreach (var interactor in grab.interactorsSelecting.ToArray())
                if (!socketsOnly || interactor is XRSocketInteractor) grab.interactionManager.SelectExit(interactor, grab);
        }
        finally { suppressEvents = false; }
    }
    public void ResetSession()
    {
        CancelSelections(false);
        pendingGrab = releasePending = hasState = false;
        heldSequence = LastOwnerSequence = 0; releaseSentEpoch = -1;
        measuredVelocity = measuredAngularVelocity = Vector3.zero;
        State = new PropPose { Position = body.position, Rotation = body.rotation, Sequence = -1 };
        previousPosition = body.position; previousRotation = body.rotation;
        RefreshPhysics();
    }
    public void ResetOwnerSequence() { LastOwnerSequence = heldSequence = 0; }
    public void RefreshPhysics()
    {
        if (!body) return;
        if (!session || !session.IsOnline) { body.isKinematic = originalKinematic; return; }
        // XRI owns the local held body's movement mode. Only the host simulates released objects.
        if (IsLocallyHeld || (pendingGrab && HandSelected)) return;
        bool remoteHeld = State.Owner != 0 && State.Owner != session.LocalOwner;
        body.isKinematic = !session.IsHost || remoteHeld || originalKinematic;
    }
    public PropPose CaptureState() => new()
    {
        Position = transform.position, Rotation = transform.rotation,
        Velocity = body.isKinematic ? measuredVelocity : body.linearVelocity,
        AngularVelocity = body.isKinematic ? measuredAngularVelocity : body.angularVelocity,
        Owner = State.Owner, Epoch = State.Epoch
    };
    public PropPose CaptureHeld()
    {
        var pose = CaptureState(); pose.Sequence = ++heldSequence;
        pose.Velocity = measuredVelocity; pose.AngularVelocity = measuredAngularVelocity;
        return pose;
    }
    public void ApplyHeldOnHost(PropPose pose)
    {
        body.isKinematic = true;
        body.position = pose.Position; body.rotation = pose.Rotation.normalized;
        measuredVelocity = Vector3.ClampMagnitude(pose.Velocity, 10);
        measuredAngularVelocity = Vector3.ClampMagnitude(pose.AngularVelocity, 20);
    }
    public void ApplyReleaseOnHost(PropPose pose)
    {
        body.position = pose.Position; body.rotation = pose.Rotation.normalized;
        body.isKinematic = originalKinematic;
        if (!body.isKinematic)
        {
            body.linearVelocity = Vector3.ClampMagnitude(pose.Velocity, 10);
            body.angularVelocity = Vector3.ClampMagnitude(pose.AngularVelocity, 20);
        }
    }
    public void ApplyState(PropPose state)
    {
        if (state.Epoch < State.Epoch || (state.Epoch == State.Epoch && state.Sequence <= State.Sequence)) return;
        bool first = !hasState;
        bool newLease = state.Epoch != State.Epoch;
        State = state; hasState = true;
        if (newLease) { heldSequence = 0; releaseSentEpoch = -1; }
        if (state.Owner != 0) CancelSelections(true);
        if (state.Owner == session.LocalOwner && state.Owner != 0)
        {
            pendingGrab = false;
            if (!HandSelected) releasePending = true; // The user released while the grant was in flight.
        }
        else if (state.Owner != 0 || !pendingGrab)
        {
            if (HandSelected) CancelSelections(false);
            pendingGrab = false;
        }
        RefreshPhysics();
        if (first && !session.IsHost && !HandSelected)
        {
            body.position = state.Position; body.rotation = state.Rotation.normalized;
        }
    }
    void LateUpdate()
    {
        if (!session || !body) return;
        if (pendingGrab && Time.unscaledTime - requestedAt > 1.5f) DenyGrab();
        float dt = Mathf.Max(Time.unscaledDeltaTime, .0001f);
        if (HandSelected)
        {
            measuredVelocity = Vector3.Lerp(measuredVelocity, Vector3.ClampMagnitude((transform.position - previousPosition) / dt, 10), .5f);
            var delta = transform.rotation * Quaternion.Inverse(previousRotation);
            delta.ToAngleAxis(out float angle, out var axis);
            if (angle > 180) angle -= 360;
            measuredAngularVelocity = Vector3.Lerp(measuredAngularVelocity,
                Vector3.ClampMagnitude(axis * (angle * Mathf.Deg2Rad / dt), 20), .5f);
        }
        if (releasePending && session.IsOnline && State.Owner == session.LocalOwner && releaseSentEpoch != State.Epoch)
        {
            releasePending = false; releaseSentEpoch = State.Epoch;
            session.ReleaseGrab(this, CaptureHeld());
        }
        if (session.IsOnline && !session.IsHost && hasState && !IsLocallyHeld && !pendingGrab)
        {
            float blend = 1 - Mathf.Exp(-25 * Time.unscaledDeltaTime);
            body.position = Vector3.Lerp(body.position, State.Position, blend);
            body.rotation = Quaternion.Slerp(body.rotation, State.Rotation, blend);
        }
        previousPosition = transform.position; previousRotation = transform.rotation;
    }
    public void Unbind()
    {
        if (grab)
        {
            grab.selectFilters.Remove(this);
            grab.selectEntered.RemoveListener(Selected);
            grab.selectExited.RemoveListener(Deselected);
        }
        if (body) body.isKinematic = originalKinematic;
        session = null;
    }
    void OnDestroy() => Unbind();
}
