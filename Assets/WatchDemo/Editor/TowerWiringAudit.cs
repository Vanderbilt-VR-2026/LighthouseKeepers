using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LighthouseKeepers.WatchDemo;
namespace LighthouseKeepers.Editor
{
    public static class TowerWiringAudit
    {
        [MenuItem("Lighthouse Keepers/M3/Wiring/Audit Preview Clearances")]
        public static void Menu()=>Run("artifacts/m3/wiring/clearance-audit.txt");
        public static string Run(string path)
        {
            var d=Resources.Load<TowerWiringAsset>("TowerWiring/Layout");if(!d||!EditorApplication.isPlaying)throw new InvalidOperationException("Open the Lab in Play Mode first.");
            var lines=new List<string>();int errors=0;Physics.SyncTransforms();
            foreach(var r in d.runs)
            {
                var seen=new HashSet<string>();for(int i=1;i<r.points.Length;i++)foreach(var c in Physics.OverlapCapsule(r.points[i-1],r.points[i],r.radius+.002f,~0,QueryTriggerInteraction.Ignore))
                {
                    var n=c.name;if(n.Contains("Player")||n.Contains("Camera")||n.Contains("Hand")||n.Contains("GRIP")||n.Contains("Marker")||!seen.Add(n))continue;
                    string reason=null;
                    if(r.name.Contains("riser")&&(n.Contains("Floor")||n.Contains("Ceiling")||n.Contains("Frame_Lantern_Base")))reason="Collared floor penetration";
                    if(n.StartsWith("UCX_SM_Safety_")||n.StartsWith("Playtest edge guard"))reason="Invisible safety volume behind fitting; no rendered rail contact";
                    if(r.name.StartsWith("SM_Gallery_")&&n.StartsWith("UCX_SM_Frame_Lantern_"))reason="Opaque header service gland above window glass";
                    if(r.name.StartsWith("Weatherproof")&&(n=="UCX_SM_Wall_F1_52_0_01"||n=="UCX_SM_Wall_House_Side_N1_01"))reason="Weatherproof wall feed-through";
                    if(r.name=="Brass PA speaker 5 House drop"&&n=="UCX_SM_Wall_House_South_1_01")reason="External PA wall feed-through";
                    if(r.name=="Beacon motor floor conduit"&&n=="Beacon drive housing")reason="Motor terminal connection";
                    if(reason==null){errors++;reason="UNEXPECTED CONTACT";}lines.Add(r.name+" | "+n+" | "+reason);
                }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllLines(path,lines.Prepend("Unexpected contacts="+errors+"; triangles="+d.triangles+"; endpoints="+d.endpoints.Length));
            if(errors!=0)throw new Exception("Wiring clearance audit: "+errors+" unexpected contacts; see "+path);
            return "PASS: no unexpected collision contacts. Intentional feed-throughs and invisible safety volumes listed; device visual sweep pending.";
        }
    }
}
