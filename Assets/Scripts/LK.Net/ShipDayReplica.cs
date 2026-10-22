using System;
using System.Collections.Generic;
using System.IO;
using LighthouseKeepers.Core.Progression;
using UnityEngine;
namespace LighthouseKeepers.Net
{
    /// <summary>Read-only board projection; clients never run their own watch/checkpoint clock.</summary>
    public sealed class ShipDayReplica : MonoBehaviour, IDayDirector
    {
        const int Kind=40;
        FusionMessageChannel channel;
        ObjectiveStep[] steps=Array.Empty<ObjectiveStep>();
        public int CurrentDay {get;private set;}
        public string CurrentObjectiveText {get;private set;}="Preparing the watch...";
        public IReadOnlyList<ObjectiveStep> Steps=>steps;
        public event Action<int> OnDayStarted;
        public event Action<int> OnDayCompleted;
        public event Action<int> OnDayFailed;
        int previousStage=-1;
        public void Bind(FusionMessageChannel stream){channel=stream;channel.Received+=Receive;}
        public static void Publish(FusionMessageChannel channel,DayDirector days)
        {
            using var s=new MemoryStream();using var w=new BinaryWriter(s);
            w.Write(1);w.Write(days.CurrentDay);w.Write((int)days.Stage);w.Write(days.CurrentObjectiveText??"");
            w.Write(days.Steps.Count);foreach(var step in days.Steps){w.Write(step.Text??"");w.Write(step.Complete);}
            var bytes=s.ToArray();if(bytes.Length<=FusionMessageChannel.MaxBytes)channel.Broadcast(Kind,bytes);
        }
        void Receive(int sender,int kind,byte[] bytes)
        {
            if(sender!=0 || kind!=Kind || channel.IsHost || bytes.Length>FusionMessageChannel.MaxBytes)return;
            try
            {
                using var s=new MemoryStream(bytes,false);using var r=new BinaryReader(s);
                if(r.ReadInt32()!=1)return;int day=r.ReadInt32(),stage=r.ReadInt32();string objective=r.ReadString();int count=r.ReadInt32();
                if(day<1 || day>7 || !Enum.IsDefined(typeof(DayStage),stage) || count<0 || count>6 || objective.Length>512)return;
                var next=new ObjectiveStep[count];
                for(int i=0;i<count;i++){string text=r.ReadString();if(text.Length>256)return;next[i]=new ObjectiveStep(text,r.ReadBoolean());}
                if(s.Position!=s.Length)return;
                bool started=CurrentDay!=day;CurrentDay=day;CurrentObjectiveText=objective;steps=next;
                if(started)OnDayStarted?.Invoke(day);
                if(stage!=previousStage){if(stage==(int)DayStage.Results)OnDayCompleted?.Invoke(day);if(stage==(int)DayStage.Failed)OnDayFailed?.Invoke(day);previousStage=stage;}
            }
            catch(IOException){}catch(ArgumentException){}
        }
        void OnDestroy(){if(channel)channel.Received-=Receive;}
    }
}
