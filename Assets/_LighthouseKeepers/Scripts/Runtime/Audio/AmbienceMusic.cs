using UnityEngine;
using UnityEngine.Audio;
namespace LighthouseKeepers.Audio
{
 [RequireComponent(typeof(AudioSource))]
 public sealed class AmbienceMusic : MonoBehaviour
 {
  [SerializeField] AudioMixer mixer;
  [SerializeField, Range(-80, 0)] float volumeDb = -20;
  [SerializeField, Min(.1f)] float fadeSeconds = 5;
  [SerializeField] AudioSource thunder;
  static AmbienceMusic instance;
  AudioSource source;
  void Awake()
  {
   if (instance && instance != this) { Destroy(gameObject); return; }
   instance = this; source = GetComponent<AudioSource>(); source.volume = 0;
  }
  void Start() { if (mixer) mixer.SetFloat("AmbienceMusicVolume", volumeDb); source.Play(); }
  void Update()
  {
   // Duck only this bed; environmental mixer snapshots retain control of the storm.
   float target = thunder && thunder.isPlaying ? .35f : 1;
   source.volume = Mathf.MoveTowards(source.volume, target, Time.deltaTime / fadeSeconds);
  }
  public void SetVolume(float decibels) { volumeDb = Mathf.Clamp(decibels, -80, 0); if (mixer) mixer.SetFloat("AmbienceMusicVolume", volumeDb); }
  void OnDestroy() { if (instance == this) instance = null; }
 }
}
