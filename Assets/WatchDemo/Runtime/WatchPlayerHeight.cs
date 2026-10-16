using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace LighthouseKeepers.WatchDemo
{
    /// <summary>Sets the isolated watch preview to a consistent standing camera height.</summary>
    [DefaultExecutionOrder(500)]
    public sealed class WatchPlayerHeight : MonoBehaviour
    {
        const float StandingHeightMetres = 1.78f; // 5 ft 10 in

        IEnumerator Start()
        {
            // Let the XR subsystem initialise first, then request a device origin with a fixed
            // standing-height floor offset. This affects only the authored preview instance.
            yield return null;
            var origin = GetComponent<XROrigin>();
            if (origin == null) yield break;

            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
            origin.CameraYOffset = StandingHeightMetres;
            if (origin.CameraFloorOffsetObject != null)
                origin.CameraFloorOffsetObject.transform.localPosition = Vector3.up * StandingHeightMetres;

            var controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.height = StandingHeightMetres;
                controller.center = Vector3.up * (StandingHeightMetres * .5f + .05f);
            }
        }
    }
}
