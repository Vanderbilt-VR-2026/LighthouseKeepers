using UnityEngine.Rendering.Universal;

namespace LighthouseKeepers.Environment.Sprint3
{
    // URP 17's RenderGraph/XR-aware full screen pass. Hardware alpha blending needs no color copy.
    public sealed class ShipLensDropletsFeature : FullScreenPassRendererFeature
    {
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
        {
            if (data.cameraData.cameraType != UnityEngine.CameraType.Game || ShipStormPresentation.LensWetness <= .001f) return;
            base.AddRenderPasses(renderer, ref data);
        }
    }
}
