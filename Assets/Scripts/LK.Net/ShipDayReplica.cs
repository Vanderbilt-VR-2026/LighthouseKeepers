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
        public LighthouseKeepers.Presentation.Board.DayBoardPresenter Presenter {get;set;}
        public float TimeLeftSeconds {get;private set;}
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
            w.Write(2);w.Write(days.CurrentDay);w.Write((int)days.Stage);w.Write(days.CurrentObjectiveText??"");
            w.Write(days.Flow.ElapsedSeconds);
            var clock=days.GetType().GetProperty("TimeLeftSeconds");w.Write(clock!=null?(float)clock.GetValue(days):0f);
            w.Write(days.Steps.Count);foreach(var step in days.Steps){w.Write(step.Text??"");w.Write(step.Complete);}
            var bytes=s.ToArray();if(bytes.Length<=FusionMessageChannel.MaxBytes)channel.Broadcast(Kind,bytes);
        }
        void Receive(int sender,int kind,byte[] bytes)
        {
            if(sender!=0 || kind!=Kind || channel.IsHost || bytes.Length>FusionMessageChannel.MaxBytes)return;
            try
            {
                using var s=new MemoryStream(bytes,false);using var r=new BinaryReader(s);
                if(r.ReadInt32()!=2)return;int day=r.ReadInt32(),stage=r.ReadInt32();string objective=r.ReadString();
                float elapsed=r.ReadSingle(),left=r.ReadSingle();int count=r.ReadInt32();
                if(!KeeperPoseFrame.Finite(elapsed) || !KeeperPoseFrame.Finite(left) || elapsed<0 || left<0)return;
                if(day<1 || day>7 || !Enum.IsDefined(typeof(DayStage),stage) || count<0 || count>6 || objective.Length>512)return;
                var next=new ObjectiveStep[count];
                for(int i=0;i<count;i++){string text=r.ReadString();if(text.Length>256)return;next[i]=new ObjectiveStep(text,r.ReadBoolean());}
                if(s.Position!=s.Length)return;
                bool started=CurrentDay!=day;CurrentDay=day;CurrentObjectiveText=objective;steps=next;TimeLeftSeconds=left;
                if(Presenter)
                {
                    var update=Presenter.GetType().GetMethod("UpdateReadOnlyPresentation");
                    object[] args=update.GetParameters().Length==6?new object[]{(DayStage)stage,default(DayResult),elapsed,"KEEPER WATCH — DAY "+day,"",left}:new object[]{(DayStage)stage,default(DayResult),elapsed,"KEEPER WATCH — DAY "+day,""};
                    update.Invoke(Presenter,args);
                }
                if(started)OnDayStarted?.Invoke(day);
                if(stage!=previousStage){if(stage==(int)DayStage.Results)OnDayCompleted?.Invoke(day);if(stage==(int)DayStage.Failed)OnDayFailed?.Invoke(day);previousStage=stage;}
            }
            catch(IOException){}catch(ArgumentException){}
        }
        void OnDestroy(){if(channel)channel.Received-=Receive;}
    }
}
