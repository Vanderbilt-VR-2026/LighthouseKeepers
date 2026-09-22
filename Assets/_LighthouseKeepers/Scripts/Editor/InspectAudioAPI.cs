using System;using System.IO;using System.Linq;using System.Reflection;using UnityEditor;
namespace LighthouseKeepers.Editor {public static class InspectAudioAPI {public static void Run(){
var types=typeof(UnityEditor.Editor).Assembly.GetTypes().Where(t=>t.FullName.Contains("AudioMixer")&& !t.FullName.Contains("GUI"));
File.WriteAllText("/tmp/lk-audio-api.txt",string.Join("\n",types.Select(t=>t.FullName+"\n"+string.Join("\n",t.GetMembers(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static).Where(m=>m.MemberType==MemberTypes.Method||m.MemberType==MemberTypes.Property).Select(m=>m.ToString())))));}}}
