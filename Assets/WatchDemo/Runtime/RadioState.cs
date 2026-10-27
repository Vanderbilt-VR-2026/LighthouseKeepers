using System;
using UnityEngine;
namespace LighthouseKeepers.WatchDemo
{
    // Day-owned rules. Physical controls and CodeSource are separate input/presentation objects.
    public sealed class RadioState
    {
        public readonly RadioProgram Program;
        public event Action BroadcastCompleted;
        public bool MainOn {get;private set;}
        public bool Locked {get;private set;}
        public bool Completed {get;private set;}
        public float FrequencyMHz {get;private set;}
        public float LockElapsed {get;private set;}
        public float TransmitElapsed {get;private set;}
        public int CodeProgress {get;private set;}
        bool power, held, trigger;
        public int Step=>Completed?4:!MainOn?0:!Locked?1:!CodeAccepted?2:3;
        public bool CodeAccepted=>CodeProgress==Program.callCode.Length;
        public bool Transmitting=>held&&trigger&&power&&MainOn&&Locked&&CodeAccepted&&!Completed;
        public RadioState(RadioProgram program){Program=program?program:throw new ArgumentNullException(nameof(program));Reset(false);}
        public void Reset(bool hasPower)
        {power=hasPower;MainOn=Locked=Completed=held=trigger=false;FrequencyMHz=Program.minimumMHz;LockElapsed=TransmitElapsed=0;CodeProgress=0;}
        public void SetPower(bool value)
        {
            power=value;
            if(!power){MainOn=false;Locked=false;LockElapsed=TransmitElapsed=0;held=trigger=false;}
        }
        public bool ToggleMain()
        {
            if(!power||MainOn)return false;
            MainOn=!MainOn;Locked=false;LockElapsed=TransmitElapsed=0;CodeProgress=0;return true;
        }
        public void Tune(float mhz)
        {
            if(Step!=1||float.IsNaN(mhz)||float.IsInfinity(mhz))return;
            FrequencyMHz=Mathf.Clamp(mhz,Program.minimumMHz,Program.maximumMHz);
            if(Mathf.Abs(FrequencyMHz-Program.targetMHz)>Program.toleranceMHz){Locked=false;LockElapsed=TransmitElapsed=0;}
        }
        // -1: inactive; 0: wrong code resets only sequence; 1: accepted symbol.
        public int PressCode(int colour)
        {
            if(!power||!MainOn||!Locked||Completed||CodeAccepted)return -1;
            if(colour!=Program.callCode[CodeProgress]){CodeProgress=0;TransmitElapsed=0;return 0;}
            CodeProgress++;return 1;
        }
        public void SetMicrophone(bool isHeld,bool isTriggered){held=isHeld;trigger=isTriggered;if(!held||!trigger)TransmitElapsed=0;}
        public void Tick(double seconds,bool canWork)
        {
            if(seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));
            if(!canWork||!power||!MainOn){TransmitElapsed=0;return;}
            if(!Locked && Mathf.Abs(FrequencyMHz-Program.targetMHz)<=Program.toleranceMHz)
            {LockElapsed=Mathf.Min(Program.lockSeconds,LockElapsed+(float)seconds);Locked=LockElapsed>=Program.lockSeconds;}
            if(!Transmitting){TransmitElapsed=0;return;}
            TransmitElapsed=Mathf.Min(Program.transmitSeconds,TransmitElapsed+(float)seconds);
            if(TransmitElapsed>=Program.transmitSeconds){Completed=true;BroadcastCompleted?.Invoke();}
        }
    }
}
