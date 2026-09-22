using UnityEngine;
namespace LighthouseKeepers.Flood {
public sealed class FloodSurface:MonoBehaviour {
 FloodController flood;
 void OnEnable(){flood=FindAnyObjectByType<FloodController>();if(flood){flood.HeightChanged+=Move;Move(flood.Height);}}
 void OnDisable(){if(flood)flood.HeightChanged-=Move;}
 void Move(float y){var p=transform.position;p.y=y;transform.position=p;}
}}
