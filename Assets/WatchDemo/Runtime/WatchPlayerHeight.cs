using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace LighthouseKeepers.WatchDemo
{
    /// <summary>Use the Quest's floor-relative eye height; never add a second standing height.</summary>
    [DefaultExecutionOrder(500)]
    public sealed class WatchPlayerHeight : MonoBehaviour
    {
        IEnumerator Start()
        {
            var origin = GetComponent<XROrigin>();
            if (!origin) yield break;
            // XROrigin owns the floor offset and updates it again when tracking initializes.
            // The old preview forced Device plus 1.78 m even when the HMD already reported height.
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            yield return null;
#if UNITY_EDITOR
            // DesktopPreview supplies one local eye height; there is no tracked floor in batch play.
            if (!XRSettings.isDeviceActive && origin.CameraFloorOffsetObject)
            {
                var offset = origin.CameraFloorOffsetObject.transform.localPosition;
                offset.y = 0;
                origin.CameraFloorOffsetObject.transform.localPosition = offset;
            }
#endif
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            yield return new WaitForSeconds(1.5f);
            Debug.Log($"WATCH_HEIGHT requested={origin.RequestedTrackingOriginMode} actual={origin.CurrentTrackingOriginMode} origin={origin.transform.position} offset={origin.CameraFloorOffsetObject.transform.localPosition} headLocal={origin.Camera.transform.localPosition} headWorld={origin.Camera.transform.position}");
#endif
        }
    }
}
