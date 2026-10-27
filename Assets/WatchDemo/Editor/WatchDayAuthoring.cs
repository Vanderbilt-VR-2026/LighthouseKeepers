using LighthouseKeepers.Core.Progression;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo.Editor
{
    public sealed class WatchDayAuthoring : AssetPostprocessor, IPreprocessBuildWithReport
    {
        public int callbackOrder => 10;
        public void OnPreprocessBuild(BuildReport report) => RefreshCatalog();
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] old)
        {
            if(imported.Concat(deleted).Concat(moved).Concat(old).Any(p=>p.StartsWith("Assets/Data/Days/",StringComparison.Ordinal)))
                EditorApplication.delayCall += RefreshCatalog;
        }
        [MenuItem("Lighthouse/Watch/Refresh day catalog")]
        public static void RefreshCatalog()
        {
            var days=AssetDatabase.FindAssets("t:DayDefinition",new[]{"Assets/Data/Days"})
                .Select(g=>AssetDatabase.LoadAssetAtPath<DayDefinition>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(d=>d.day).ToArray();
            if(days.Length==0)throw new BuildFailedException("Create at least one day in Assets/Data/Days.");
            for(int i=0;i<days.Length;i++)
            {
                var d=days[i];
                if(d.day!=i+1 || d.dayLengthSeconds<30 || float.IsNaN(d.dayLengthSeconds) || float.IsInfinity(d.dayLengthSeconds) || d.tasks==null || d.tasks.Length==0)
                    throw new BuildFailedException("Invalid/duplicate/non-contiguous day: "+d.name);
                var ids=d.tasks.Select(t=>t.stationId).ToArray();
                if(ids.Distinct().Count()!=ids.Length || d.tasks.Any(t=>string.IsNullOrWhiteSpace(t.taskText)|| (t.stationId!=WatchState.PipeStation && t.stationId!=WatchState.GeneratorStation && t.stationId!=WatchState.RadioStation)) || d.startingFaults==null || d.startingFaults.Any(f=>!ids.Contains(f)))
                    throw new BuildFailedException("Invalid task station/text or starting fault: "+d.name);
            }
            const string path="Assets/WatchDemo/Resources/WatchDays.asset";
            if(!AssetDatabase.IsValidFolder("Assets/WatchDemo/Resources"))AssetDatabase.CreateFolder("Assets/WatchDemo","Resources");
            var catalog=AssetDatabase.LoadAssetAtPath<DayCatalog>(path);
            if(!catalog){catalog=ScriptableObject.CreateInstance<DayCatalog>();AssetDatabase.CreateAsset(catalog,path);}
            if(catalog.days==null || !catalog.days.SequenceEqual(days)){catalog.days=days;EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();}
        }
        public static void Seed()
        {
            if(!AssetDatabase.IsValidFolder("Assets/Data/Days"))AssetDatabase.CreateFolder("Assets/Data","Days");
            for(int day=1;day<=3;day++)
            {
                string path=$"Assets/Data/Days/Day{day:00}.asset";
                var d=AssetDatabase.LoadAssetAtPath<DayDefinition>(path);
                if(!d){d=ScriptableObject.CreateInstance<DayDefinition>();d.day=day;AssetDatabase.CreateAsset(d,path);}
                if(d.tasks.Length>0)continue;
                d.tasks=new[]{new WatchTaskStep(WatchState.GeneratorStation,"Restart the generator"),new WatchTaskStep(WatchState.PipeStation,"Repair the pipe")};
                d.startingFaults=new[]{WatchState.GeneratorStation,WatchState.PipeStation};
                if(day==1){d.tasks=new[]{new WatchTaskStep(WatchState.PipeStation,"Repair the pipe")};d.startingFaults=new[]{WatchState.PipeStation};}
                EditorUtility.SetDirty(d);
            }
            RefreshCatalog();
        }
    }
}
