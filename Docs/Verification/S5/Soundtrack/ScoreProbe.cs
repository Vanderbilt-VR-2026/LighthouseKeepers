using System;using System.Linq;using UnityEngine;using LighthouseKeepers.WatchDemo;
public static class ScoreProbe {
 static WatchDemoController W=>UnityEngine.Object.FindAnyObjectByType<WatchDemoController>();
 static AdaptiveMusic M=>W.GetComponent<AdaptiveMusic>();
 public static object Facts()=>new{M.ContinuousScore,M.MainClip,M.MainTime,M.OverlayCount,M.OverlayPlaying,sources=M.GetComponentsInChildren<AudioSource>().Where(a=>a.name=="Adaptive music A"||a.name=="Adaptive music B").Select(a=>new{a.name,clip=a.clip?a.clip.name:"",a.isPlaying,a.loop,a.volume}).ToArray()};
 public static object Accent(){bool first=M.RequestAccent();bool duplicate=M.RequestAccent();return new{first,duplicate,facts=Facts()};}
 public static object Mode(){typeof(AdaptiveMusic).GetMethod("Transition",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(M,new object[]{2,W.State.Elapsed,0f});return Facts();}
 public static object Threat(){typeof(LighthouseKeepers.Core.Progression.CrewRecoveryState).GetMethod("Stage",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(W.State.Recovery,new object[]{3});return Facts();}
}
