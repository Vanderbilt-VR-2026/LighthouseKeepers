using UnityEngine;
namespace LighthouseKeepers.Flood {
public sealed class FloodSurface:MonoBehaviour {
 [SerializeField,Tooltip("Darken the water toward black as it rises. Uses a material property block; the shared material is never modified.")] bool escalateTone=true;
 [SerializeField] Color abyssColor=new(0.004f,0.012f,0.014f);
 static readonly int BaseColorId=Shader.PropertyToID("_BaseColor");
 FloodController flood; Renderer surfaceRenderer; MaterialPropertyBlock block; Color baseColor=Color.gray; bool baseCaptured;
 public float CurrentDarkening {get;private set;}
 void OnEnable(){surfaceRenderer=GetComponent<Renderer>();flood=FindAnyObjectByType<FloodController>();if(flood){flood.HeightChanged+=Move;Move(flood.Height);}}
 void OnDisable(){if(flood)flood.HeightChanged-=Move;}
 public float ToneFor(float h){if(!escalateTone)return 0;float min=flood?flood.MinimumHeight:-0.25f,max=flood?flood.MaximumHeight:10.5f;return Mathf.InverseLerp(min,max,h);}
 void Move(float y){var p=transform.position;p.y=y;transform.position=p;ApplyTone(y);}
 void ApplyTone(float y){CurrentDarkening=ToneFor(y);if(!surfaceRenderer)return;
 if(!baseCaptured){var mat=surfaceRenderer.sharedMaterial;if(mat&&mat.HasProperty(BaseColorId))baseColor=mat.GetColor(BaseColorId);baseCaptured=true;}
 block??=new MaterialPropertyBlock();surfaceRenderer.GetPropertyBlock(block);
 block.SetColor(BaseColorId,Color.Lerp(baseColor,abyssColor,CurrentDarkening));surfaceRenderer.SetPropertyBlock(block);}
}}