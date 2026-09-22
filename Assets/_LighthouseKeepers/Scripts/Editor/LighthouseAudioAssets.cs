using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
namespace LighthouseKeepers.Editor {
public static class LighthouseAudioAssets {
 public static readonly string[] Groups={"Master","Weather Exterior","Weather Interior","Thunder","Ocean","Structure","Machinery","Electrical","Water","Radio","Player","UI"};
 public static readonly string[] Snapshots={"Exterior Storm","Interior Normal","Lower Mechanical","Generator Running","Power Failure","Flood Emergency","Lantern Balcony"};
 const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static object Call(object instance,string method,params object[] args)=>instance.GetType().GetMethod(method,Flags).Invoke(instance,args);
 static object Get(object instance,string property)=>instance.GetType().GetProperty(property,Flags).GetValue(instance);
 public static AudioMixer Create(){
  string path=LighthouseAssets.Root+"/Audio/Mixer/LK_Atmosphere.mixer";
  var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(path);if(mixer)return mixer;
  // Unity exposes no public mixer authoring API. Reflection is confined to this editor tool.
  var type=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController",true);
  mixer=(AudioMixer)type.GetMethod("CreateMixerControllerAtPath",Flags).Invoke(null,new object[]{path});
  var master=Get(mixer,"masterGroup");
  foreach(var name in Groups.Skip(1)){var group=Call(mixer,"CreateNewGroup",name,true);Call(mixer,"AddChildToParent",group,master);}
  var first=(AudioMixerSnapshot)Get(mixer,"TargetSnapshot");first.name=Snapshots[0];
  for(int i=1;i<Snapshots.Length;i++){Call(mixer,"CloneNewSnapshotFromTarget",false);((AudioMixerSnapshot)Get(mixer,"TargetSnapshot")).name=Snapshots[i];}
  foreach(var snapshot in (Array)Get(mixer,"snapshots")){
   string name=((AudioMixerSnapshot)snapshot).name;bool exterior=name=="Exterior Storm"||name=="Lantern Balcony";
   foreach(var group in mixer.FindMatchingGroups("")){
    float volume=group.name=="Master"?-5:group.name=="Weather Exterior"?(exterior?-3:-24):group.name=="Weather Interior"?(exterior?-22:-5):group.name=="Ocean"?(exterior?-8:-22):group.name=="Machinery"?(name=="Power Failure"?-80:name=="Generator Running"?-2:-12):group.name=="Water"?(name=="Flood Emergency"?0:-8):-6;
    Call(group,"SetValueForVolume",mixer,snapshot,volume);
   }
  }
  type.GetProperty("startSnapshot",Flags).SetValue(mixer,mixer.FindSnapshot("Interior Normal"));EditorUtility.SetDirty(mixer);AssetDatabase.SaveAssets();return mixer;
 }
 public static AudioSource Emitter(string name,Vector3 position,string clip,string group,float volume=0.3f,bool loop=true){
  var go=new GameObject(name);go.transform.position=position;var source=go.AddComponent<AudioSource>();source.clip=LighthouseAssets.Clip(clip);source.outputAudioMixerGroup=Create().FindMatchingGroups(group).First(g=>g.name==group);source.spatialBlend=1;source.minDistance=1.5f;source.maxDistance=12;source.rolloffMode=AudioRolloffMode.Logarithmic;source.volume=volume;source.loop=loop;source.playOnAwake=loop;return source;
 }
}}
