using UnityEngine;
namespace LighthouseKeepers.Audio {
[RequireComponent(typeof(AudioSource))]
public sealed class RandomSoundEmitter:MonoBehaviour {
 [SerializeField] AudioClip[] clips;
 [SerializeField] Vector2 spacing=new(12,35),pitch=new(0.8f,1.15f),volume=new(0.15f,0.35f);
 AudioSource source;float next;
 void Start(){source=GetComponent<AudioSource>();next=Time.time+Random.Range(spacing.x,spacing.y);}
 void Update(){if(Time.time<next||clips.Length==0)return;source.pitch=Random.Range(pitch.x,pitch.y);source.PlayOneShot(clips[Random.Range(0,clips.Length)],Random.Range(volume.x,volume.y));next=Time.time+Random.Range(spacing.x,spacing.y);}
}}
