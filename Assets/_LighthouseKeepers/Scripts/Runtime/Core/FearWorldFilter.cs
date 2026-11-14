using UnityEngine;
namespace LighthouseKeepers.Core
{
    // Cascaded one-pole filter, only assigned to World/Ambience. Radio and crew voice stay untouched.
    public sealed class FearWorldFilter:MonoBehaviour
    {
        readonly float[] z1=new float[8],z2=new float[8];
        volatile float alpha=1,wet;
        int rate;bool wasFiltering;
        public bool FilterBus {get;set;}=true;
        public static bool CanAttach(AudioSource source)=>source&&!source.GetComponent<AudioListener>()&&source.GetComponents<AudioSource>().Length==1;
        void Awake()=>rate=AudioSettings.outputSampleRate;
        void Update()
        {
            float fear=FilterBus&&KeeperAudioMix.Instance?KeeperAudioMix.Instance.LocalFear:0;
            float fraction=Mathf.Clamp01((fear-.6f)/.4f),cutoff=Mathf.Lerp(22000,6000,fraction);
            alpha=1-Mathf.Exp(-2*Mathf.PI*Mathf.Min(cutoff,rate*.45f)/rate);wet=fraction;
        }
        void OnAudioFilterRead(float[] data,int channels)
        {
            if(channels<1||channels>8)return;float a=alpha,w=wet;
            if(w<=0){if(wasFiltering){System.Array.Clear(z1,0,z1.Length);System.Array.Clear(z2,0,z2.Length);wasFiltering=false;}return;}wasFiltering=true;
            for(int i=0;i<data.Length;i++){int c=i%channels;float original=data[i];z1[c]+=a*(original-z1[c]);z2[c]+=a*(z1[c]-z2[c]);if(w>0)data[i]=original+(z2[c]-original)*w;}
        }
    }
}
