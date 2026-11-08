using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    public sealed partial class WatchDemoController
    {
        readonly HandheldCrewRadio[] crewRadios=new HandheldCrewRadio[2];
        void CreateCrewRadios(){if(state.CrewRadio==null)return;for(int i=0;i<2;i++)crewRadios[i]=HandheldCrewRadio.Create(this,i);}
        void UpdateCrewRadios(){if(state?.CrewRadio!=null&&!state.ReadOnly)state.CrewRadio.Tick(Time.unscaledDeltaTime);}
        public bool RadioCommand(int actor,int index,bool active)=>!state.ReadOnly&&state.Coop!=null&&state.CrewRadio!=null&&state.CrewRadio.Set(index,actor,active);
        public bool IsRadioTransmission(int actor)=>state.CrewRadio?.Transmitting(actor)==true;
        public int RadioTransmitter(int index)=>state.CrewRadio?.Actor(index)??0;
        public void RadioActorLeft(int actor)=>state.CrewRadio?.Remove(actor);
        public Transform LocalRadio
        {get{foreach(var radio in crewRadios)if(radio&&radio.Held)return radio.transform;return null;}}
    }
}
