using System.Globalization;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    /// <summary>Opt-in device log samples. RTT is measured, not assumed from the impairment setting.</summary>
    public sealed class CrewNetworkMetrics : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] bool recordSamples;
        float nextSample;
        public void SetRecording(bool enabled) => recordSamples=enabled;
        void Update()
        {
            var channel=FusionMessageChannel.Active;
            if(!recordSamples || !channel || !channel.Connected || Time.unscaledTime<nextSample)return;
            nextSample=Time.unscaledTime+1;
            foreach(var peer in channel.Runner.ActivePlayers)
                if(peer!=channel.Runner.LocalPlayer)
                    Debug.Log(string.Format(CultureInfo.InvariantCulture,"LK_NET_METRIC,time={0:F3},actor={1},rtt_ms={2:F3}",
                        Time.unscaledTime,peer.RawEncoded,channel.Runner.GetPlayerRtt(peer)*1000));
        }
#endif
    }
}
