using System;
namespace LighthouseKeepers.Lobby {
public enum CrewDuty { None=0, Lightkeeper=1, Machinist=2 }
public static class CrewDuties {
 public static bool Valid(CrewDuty duty)=>duty>=CrewDuty.None&&duty<=CrewDuty.Machinist;
 public static CrewDuty Other(CrewDuty duty)=>duty==CrewDuty.Lightkeeper?CrewDuty.Machinist:duty==CrewDuty.Machinist?CrewDuty.Lightkeeper:CrewDuty.None;
 public static CrewDuty Preference {
  get {var duty=(CrewDuty)UnityEngine.PlayerPrefs.GetInt("LK.Crew.Duty",1);return Valid(duty)&&duty!=CrewDuty.None?duty:CrewDuty.Lightkeeper;}
  set {if(!Valid(value)||value==CrewDuty.None)return;UnityEngine.PlayerPrefs.SetInt("LK.Crew.Duty",(int)value);UnityEngine.PlayerPrefs.Save();}
 }
}
/// <summary>One roster entry. Plain data so any transport (local stub today,
/// Photon Fusion online) can populate it without touching UI code.</summary>
[Serializable]
public sealed class LobbyPlayer {
 public CrewDuty Duty;
 public CrewDuty RequestedDuty;
 public string Id;
 public string DisplayName;
 public bool IsHost;
 public bool IsReady;
 public LobbyPlayer(string id,string displayName,bool isHost=false){Id=id;DisplayName=displayName;IsHost=isHost;}
 public override string ToString(){var role=IsHost?"HOST":"guest";var ready=IsReady?"READY":"not ready";return $"{DisplayName} [{role}, {ready}]";}
}}
