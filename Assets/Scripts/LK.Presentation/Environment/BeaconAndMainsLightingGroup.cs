using UnityEngine;
using UnityEngine.Rendering;
namespace LighthouseKeepers.Presentation.Environment
{
    // Presentation only. Eli supplies the flag; this component never decides power state.
    [ExecuteAlways]
    public sealed class BeaconAndMainsLightingGroup : MonoBehaviour
    {
        [SerializeField] bool powered=true;
        [SerializeField] EnvironmentLightingPreset poweredPreset, emergencyPreset;
        [SerializeField] Renderer[] bakedRenderers;
        [SerializeField] Light[] poweredRealtimeLights;
        [SerializeField] Renderer[] mainsEmitters;
        MaterialPropertyBlock darkEmission;
        LightmapData[] poweredMaps,emergencyMaps;
        SphericalHarmonicsL2[] poweredProbes,emergencyProbes;
        bool applied, previous;
        public bool Powered { get=>powered; set=>powered=value; }
        void Awake(){darkEmission=new MaterialPropertyBlock();darkEmission.SetColor("_EmissionColor",Color.black);poweredProbes=Probes(poweredPreset);emergencyProbes=Probes(emergencyPreset);poweredMaps=Maps(poweredPreset);emergencyMaps=Maps(emergencyPreset);}
        static LightmapData[] Maps(EnvironmentLightingPreset p)
        {
            if(!p)return null;var maps=new LightmapData[p.colorMaps.Length];
            for(int i=0;i<maps.Length;i++)maps[i]=new LightmapData{lightmapColor=p.colorMaps[i],lightmapDir=p.directionMaps.Length>i?p.directionMaps[i]:null,shadowMask=p.shadowMasks.Length>i?p.shadowMasks[i]:null};
            return maps;
        }
        static SphericalHarmonicsL2[] Probes(EnvironmentLightingPreset p)
        {
            if(!p||p.probeCoefficients==null)return null;var result=new SphericalHarmonicsL2[p.probeCoefficients.Length/27];for(int i=0;i<result.Length;i++)for(int c=0;c<3;c++)for(int k=0;k<9;k++)result[i][c,k]=p.probeCoefficients[i*27+c*9+k];return result;
        }
        void LateUpdate()
        {
            if(applied&&previous==powered)return;
            var p=powered?poweredPreset:emergencyPreset;var maps=powered?poweredMaps:emergencyMaps;
            if(!p||maps==null)return;
            LightmapSettings.lightmapsMode=p.mode;LightmapSettings.lightmaps=maps;
            for(int i=0;i<bakedRenderers.Length;i++)if(bakedRenderers[i]&&!bakedRenderers[i].isPartOfStaticBatch){bakedRenderers[i].lightmapIndex=p.rendererIndices[i];bakedRenderers[i].lightmapScaleOffset=p.rendererScaleOffsets[i];}
            foreach(var r in mainsEmitters)if(r)r.SetPropertyBlock(powered?null:darkEmission);
            foreach(var l in poweredRealtimeLights)if(l)l.enabled=powered;
            var probes=powered?poweredProbes:emergencyProbes;if(LightmapSettings.lightProbes&&probes!=null&&probes.Length==LightmapSettings.lightProbes.count)LightmapSettings.lightProbes.bakedProbes=probes;
            RenderSettings.ambientLight=p.ambient;applied=true;previous=powered;
        }
    }
}
