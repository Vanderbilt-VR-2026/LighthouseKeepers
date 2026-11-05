using System.Collections.Generic;using UnityEngine;
namespace LighthouseKeepers.WatchDemo {
 public sealed class StormPresentation:MonoBehaviour {
  WatchDemoController watch;readonly List<(Transform t,Quaternion rest)> lamps=new();ParticleSystem rain;float originalRain;AudioSource ocean;float originalOcean;Renderer sea;MaterialPropertyBlock block;
  public static void Create(WatchDemoController w){var p=w.gameObject.AddComponent<StormPresentation>();p.watch=w;}
  void Start(){var seaObject=GameObject.Find("Original storm ocean presentation");if(seaObject){sea=seaObject.GetComponent<Renderer>();block=new MaterialPropertyBlock();var bounds=sea.localBounds;bounds.Expand(Vector3.up);sea.localBounds=bounds;}foreach(var light in FindObjectsByType<Light>(FindObjectsSortMode.None))if(light.name.Contains("Shielded stair lamp")||light.name.Contains("service lamp"))lamps.Add((light.transform,light.transform.localRotation));
   foreach(var p in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))if(p.name.ToLowerInvariant().Contains("rain")){rain=p;originalRain=p.emission.rateOverTime.constant;break;}
   foreach(var a in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))if(a.name.ToLowerInvariant().Contains("ocean")){ocean=a;originalOcean=a.volume;break;}
  }
  void LateUpdate(){int stage=watch.State.Storm.Stage;float strength=(stage-1)/3f;foreach(var lamp in lamps)if(lamp.t)lamp.t.localRotation=lamp.rest*Quaternion.Euler(0,0,Mathf.Sin((float)watch.State.Elapsed*1.3f)*strength*2);
   if(sea){sea.GetPropertyBlock(block);block.SetFloat("_StormAmplitude",Mathf.Lerp(.08f,.3f,strength));sea.SetPropertyBlock(block);}
   if(rain){var e=rain.emission;e.rateOverTime=originalRain*Mathf.Lerp(.8f,1.15f,strength);}if(ocean)ocean.volume=originalOcean*Mathf.Lerp(.65f,1.2f,strength);
  }
 }
}