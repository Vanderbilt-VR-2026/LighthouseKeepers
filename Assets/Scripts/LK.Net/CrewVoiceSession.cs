using System.Collections;
using Fusion;
using LighthouseKeepers.Core;
using LighthouseKeepers.Lobby;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using UnityEngine;

namespace LighthouseKeepers.Net
{
    /// <summary>Photon's Fusion room lifecycle, with primary open-mic audio attached to our existing pose avatars.</summary>
    public sealed class CrewVoiceSession: FusionVoiceClient, ICrewVoice
    {
        FusionMessageChannel channel;
        S4Tuning tuning;
        bool permission,requesting;
        string permissionStatus="Requesting microphone permission";
        public bool Muted {get;private set;}
        public bool MicrophoneAllowed=>permission;
        public override bool AlwaysUsePrimaryRecorder=>true;
        public string StatusText=>!permission?permissionStatus:Muted?"MIC MUTED":"MIC ON · "+ClientState;

        public static void Ensure(NetworkRunner runner,FusionMessageChannel channel)
        {
            var t=S4Tuning.Current;
            if(!ShipWorld.CoopEnabled||!t||!t.crewVoice||!channel.Connected||runner.GameMode==GameMode.Single||runner.GetComponent<CrewVoiceSession>())return;
            if(!System.Guid.TryParse(t.voiceAppId,out _)){Debug.LogWarning("CREW_VOICE missing Photon Voice App ID");return;}
            var voice=runner.gameObject.AddComponent<CrewVoiceSession>();voice.channel=channel;voice.tuning=t;CrewVoiceBridge.Current=voice;
        }
        protected override void Awake(){AutoConnectAndJoin=false;base.Awake();}
        protected override void Start()
        {
            if(!channel||!channel.Connected){Destroy(this);return;}
            UseFusionAppSettings=false;
            Settings=new Photon.Realtime.AppSettings{AppIdVoice=tuning.voiceAppId,AppVersion="LK-S4-Voice-1",FixedRegion=channel.Runner.SessionInfo.Region,UseNameServer=true};
            var go=new GameObject("Crew microphone (permission gated)");go.transform.SetParent(transform,false);
            PrimaryRecorder=go.AddComponent<Recorder>();PrimaryRecorder.RecordingEnabled=false;PrimaryRecorder.TransmitEnabled=false;
            PrimaryRecorder.UserData=channel.LocalActorId;PrimaryRecorder.VoiceDetection=false;
            PrimaryRecorder.StopRecordingWhenPaused=true;PrimaryRecorder.Bitrate=24000;
            base.Start();AddRecorder(PrimaryRecorder);channel.Runner.AddCallbacks(this);
            AutoConnectAndJoin=true;ConnectAndJoinRoom();
            StartCoroutine(RequestMicrophone());
            Debug.Log("CREW_VOICE joined crew; permission before capture; region="+Settings.FixedRegion);
        }
        IEnumerator RequestMicrophone()
        {
            if(requesting)yield break;requesting=true;
#if UNITY_ANDROID && !UNITY_EDITOR
            permission=UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
            if(!permission)
            {
                bool finished=false;
                var callbacks=new UnityEngine.Android.PermissionCallbacks();
                callbacks.PermissionGranted+=_=>{permission=true;finished=true;};
                callbacks.PermissionDenied+=_=>{permission=false;finished=true;};
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone,callbacks);
                while(!finished)yield return null;
            }
#else
            if(!Application.HasUserAuthorization(UserAuthorization.Microphone))yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            permission=Application.HasUserAuthorization(UserAuthorization.Microphone);
#endif
            requesting=false;permissionStatus=permission?"Microphone allowed":"MIC PERMISSION DENIED · tap to retry";
            ApplyMute();Debug.Log("CREW_VOICE microphone permission="+permission);
        }
        public void ToggleMute()
        {
            if(!permission){StartCoroutine(RequestMicrophone());return;}
            Muted=!Muted;ApplyMute();
        }
        void ApplyMute()
        {
            if(!PrimaryRecorder)return;
            PrimaryRecorder.TransmitEnabled=permission&&!Muted;PrimaryRecorder.RecordingEnabled=permission&&!Muted;
        }
        protected override Speaker InstantiateSpeakerForRemoteVoice(int playerId,byte voiceId,object userData)
        {
            if(!(userData is int actor)||!channel||actor==channel.LocalActorId||!channel.IsActorConnected(actor))return null;
            var go=new GameObject("Remote keeper voice "+actor);go.transform.SetParent(transform,false);
            var audio=go.AddComponent<AudioSource>();audio.playOnAwake=false;audio.spatialBlend=1;audio.dopplerLevel=0;
            audio.rolloffMode=AudioRolloffMode.Linear;audio.minDistance=tuning.voiceNearM;audio.maxDistance=tuning.voiceFarM;audio.volume=tuning.voiceVolume;
            var follow=go.AddComponent<CrewVoiceHead>();follow.Actor=actor;
            var speaker=go.AddComponent<Speaker>();speaker.OnRemoteVoiceRemoveAction=s=>{if(s)Destroy(s.gameObject);};return speaker;
        }
        protected override void OnDestroy()
        {
            if(ReferenceEquals(CrewVoiceBridge.Current,this))CrewVoiceBridge.Current=null;
            if(PrimaryRecorder)PrimaryRecorder.RecordingEnabled=false;
            if(channel&&channel.Runner)channel.Runner.RemoveCallbacks(this);
            base.OnDestroy();
        }
    }
    sealed class CrewVoiceHead:MonoBehaviour
    {
        public int Actor;
        void LateUpdate()
        {
            var poses=CrewPoseStream.Active;
            if(poses&&poses.SampleVisual(Actor,out var pose))transform.position=pose.Head;
        }
    }
}
