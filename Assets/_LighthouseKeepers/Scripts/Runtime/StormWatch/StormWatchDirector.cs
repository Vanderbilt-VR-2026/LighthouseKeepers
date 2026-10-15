using System.Collections;
using LighthouseKeepers.Core;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Unity.XR.CoreUtils;

namespace LighthouseKeepers.StormWatch
{
    public sealed class StormWatchDirector : MonoBehaviour
    {
        [SerializeField] WatchLink link;
        [SerializeField] WatchTuning tuning = new();
        [SerializeField] GameObject[] gates;
        [SerializeField] AudioClip feedbackClip;
        [SerializeField] GameObject rescueCatch, rescuePull;
        public WatchState State => link && link.Connected && !link.IsHost ? link.ReceivedState ?? rules.State : rules.State;
        public WatchTuning Tuning => tuning;
        public bool Ready { get; private set; }
        public bool SessionEnded { get; private set; }
        bool hadCrew;
        public int LocalSlot => link ? link.LocalSlot : 0;
        public bool CrewReady => Ready && link && link.Connected && link.PartnerConnected;
        public string Connection => link ? link.Status : "No network transport assigned.";
        public KeeperInput Local { get; } = new();
        public KeeperInput Partner => link ? link.RemoteInput : null;
        public StormWatchRules Rules => rules;
        public Transform Head => rig ? rig.Camera.transform : null;
        public Transform LeftHand { get; private set; }
        public Transform RightHand { get; private set; }
        public bool HoldingWrench { get; set; }
        public bool Down => Ready && State.Down(LocalSlot);
        StormWatchRules rules;
        XROrigin rig;
        FloodController flood;
        DayDirector days;
        GameObject leak, beam;
        AudioSource feedback;
        Transform remoteHead, remoteBody, remoteLeft, remoteRight;
        LineRenderer rope;
        float tick, transmit;
        int lastDay, lastMessage = -1, lastTeleport;
        WatchAction action;
        float effort;
        bool movingForRescue, movementSuspended;
        AudioSource generatorSound;
        bool lastPower=true;
        Behaviour[] locomotion;
        bool[] movementStates;

        void Awake() => rules = new StormWatchRules(tuning);
        IEnumerator Start()
        {
            var bootstrap = FindAnyObjectByType<EnvironmentBootstrap>();
            while (bootstrap && !bootstrap.Ready) yield return null;
            rig = FindAnyObjectByType<XROrigin>(); flood = FindAnyObjectByType<FloodController>(); days = FindAnyObjectByType<DayDirector>();
            if (!rig || !rig.Camera || !flood || !days || !link || gates == null || gates.Length != 3 || System.Array.Exists(gates, gate => !gate) || !rescueCatch || !rescuePull)
            { Debug.LogError("Storm Watch needs an XR rig, flood, weather, link and three gates.", this); yield break; }
            foreach (var t in rig.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Left Controller") LeftHand = t;
                if (t.name == "Right Controller") RightHand = t;
            }
            if (!LeftHand) LeftHand = rig.Camera.transform;
            if (!RightHand) RightHand = rig.Camera.transform;
            var movement = new System.Collections.Generic.List<Behaviour>();
            foreach(var component in rig.GetComponentsInChildren<Behaviour>(true))
                if(component is LocomotionProvider || component is LighthouseKeepers.Player.PlayerComfort ||
                   component is LighthouseKeepers.Player.DesktopPreview || component is LighthouseKeepers.Player.GroundedSlopeMovement) movement.Add(component);
            locomotion = movement.ToArray();
            movementStates = new bool[locomotion.Length];
            for (int i=0;i<locomotion.Length;i++) movementStates[i]=locomotion[i].enabled;
            leak = GameObject.Find("Active pipe leak"); beam = GameObject.Find("Beacon beam light");
            feedback = gameObject.AddComponent<AudioSource>(); feedback.playOnAwake = false; feedback.volume = .25f;
            rig.Camera.clearFlags=CameraClearFlags.SolidColor;
            rig.Camera.backgroundColor=new Color(.009f,.017f,.032f);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.045f,.055f,.075f);RenderSettings.sun=null;RenderSettings.fogColor=rig.Camera.backgroundColor;
            generatorSound=GameObject.Find("Room-specific placeholder Generator")?.GetComponent<AudioSource>();
            BuildPartner(); Ready = true;
        }
        public void Connect(bool host, string room) { if (Ready && !link.Connected) { rules.Reset(); lastTeleport=0; hadCrew=false; SessionEnded=false; Local.command=WatchAction.None; link.Connect(host, room); } }
        public void Leave()
        {
            link?.Leave(); rules.Reset(); lastTeleport=0; hadCrew=false; SessionEnded=false;
            Local.command=WatchAction.None;
            if(Ready)StartCoroutine(Relocate(new Vector3(-8,.1f,0)));
        }
        public void Command(WatchAction command) { Local.command = command; Local.sequence++; }
        public void Work(WatchAction which, float amount = 1)
        {
            if (!Ready || Down && which != WatchAction.PullLine) return;
            // One keeper performs one job at a time; a hand cannot also provide their own brace/pump.
            action = which; effort = Mathf.Clamp01(amount);
        }
        void Update()
        {
            if (!Ready) return;
            if(hadCrew && !CrewReady)SessionEnded=true;
            if(CrewReady)hadCrew=true;
            Local.connected = link.Connected;
            Local.feet = rig.transform.position; Local.head = Head.position; Local.headRotation = Head.rotation;
            Local.left = LeftHand.position; Local.leftRotation = LeftHand.rotation;
            Local.right = RightHand.position; Local.rightRotation = RightHand.rotation; if (Local.command != WatchAction.CastLine) Local.aim = RightHand.forward;
            Desktop();
            Local.action = action; Local.effort = effort;
            tick += Time.deltaTime; transmit += Time.deltaTime;
            // Bound catch-up after suspension while preserving the normal 20 Hz simulation rate.
            tick = Mathf.Min(tick, .25f);
            while (tick >= .05f)
            {
                tick -= .05f;
                if (link.IsHost && link.Connected) rules.Tick(.05f, Local, Partner);
            }
            if (transmit >= .1f)
            {
                transmit = 0; link.SendInput(Local);
                if (link.IsHost) link.Publish(State);
                Local.command = WatchAction.None;
            }
            ApplyWorld();
            action = WatchAction.None; effort = 0;
        }
        void ApplyWorld()
        {
            var s = State;
            if (lastDay != s.Day) { lastDay=s.Day; days.ApplyDay(lastDay); }
            flood.Paused = true; if (Mathf.Abs(flood.Height-s.water)>.001f) flood.SetHeight(s.water);
            if (leak && leak.activeSelf != (s.pipeSteps < 3)) leak.SetActive(s.pipeSteps < 3);
            if (beam && beam.activeSelf != s.beacon) beam.SetActive(s.beacon);
            for(int i=0;i<3;i++){bool open=i==0?s.gateOne:i==1?s.gateTwo:s.gateThree;if(gates[i] && gates[i].activeSelf==open)gates[i].SetActive(!open);}
            if(rescueCatch){rescueCatch.SetActive(s.ropeTarget==LocalSlot);rescueCatch.transform.position=Head.position+Head.forward*.45f-Vector3.up*.2f;}
            if(rescuePull){rescuePull.SetActive(s.ropeOwner==LocalSlot);rescuePull.transform.position=Head.position+Head.forward*.4f-Vector3.up*.4f;}
            if (lastMessage != s.messageRevision)
            {
                lastMessage=s.messageRevision;
                if (feedbackClip) { feedback.pitch=s.Terminal?1.4f:1; feedback.PlayOneShot(feedbackClip); }
            }
            bool suspend=Down||movingForRescue;
            if(suspend!=movementSuspended)
            {
                for(int i=0;i<locomotion.Length;i++)if(locomotion[i])
                {if(suspend){movementStates[i]=locomotion[i].enabled;locomotion[i].enabled=false;}else locomotion[i].enabled=movementStates[i];}
                movementSuspended=suspend;
            }
            if(s.power!=lastPower)
            {
                lastPower=s.power;
                if(generatorSound){if(s.power)generatorSound.Play();else generatorSound.Stop();}
                RenderSettings.ambientLight=s.power?new Color(.045f,.055f,.075f):new Color(.014f,.019f,.028f);
            }
            int teleport = LocalSlot==0?s.teleport0:s.teleport1;
            if(teleport!=lastTeleport)
            { lastTeleport=teleport; StartCoroutine(Relocate(LocalSlot==0?s.destination0:s.destination1)); }
            UpdatePartner();
        }
        IEnumerator Relocate(Vector3 destination)
        {
            // The caught lifeline / explicit retry authorizes this relocation. Fade before moving the rig.
            movingForRescue=true;
            var canvasGo=new GameObject("Rescue blink",typeof(Canvas)); var canvas=canvasGo.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=rig.Camera;canvas.planeDistance=.15f;canvas.sortingOrder=32000;
            var cover=new GameObject("Black",typeof(RectTransform),typeof(UnityEngine.UI.Image));cover.transform.SetParent(canvasGo.transform,false);
            var rect=cover.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            cover.GetComponent<UnityEngine.UI.Image>().color=Color.black;
            yield return new WaitForSeconds(.18f);
            var character=rig.GetComponent<CharacterController>();bool enabled=character&&character.enabled;if(character)character.enabled=false;
            var offset=Vector3.ProjectOnPlane(Head.position-rig.transform.position,Vector3.up);
            rig.transform.position=destination-offset;Physics.SyncTransforms();if(character)character.enabled=enabled;
            yield return new WaitForSeconds(.18f);Destroy(canvasGo);movingForRescue=false;
        }
        void Desktop()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(XRSettings.isDeviceActive||Keyboard.current==null)return;
            var k=Keyboard.current;
            if(k.enterKey.wasPressedThisFrame)Command(State.act==WatchAct.Briefing?WatchAction.Begin:State.Terminal?WatchAction.Retry:WatchAction.Continue);
            if(k.bKey.isPressed)Work(WatchAction.Brace);
            if(k.rKey.isPressed)Work(WatchAction.Pipe);
            if(k.pKey.isPressed)Work(WatchAction.Pump);
            if(k.gKey.isPressed)Work(WatchAction.Generator);
            if(k.tKey.isPressed)Work(WatchAction.Radio);
            if(k.digit1Key.isPressed)Work(WatchAction.BeaconA);
            if(k.digit2Key.isPressed)Work(WatchAction.BeaconB);
            if(k.digit3Key.isPressed)Work(WatchAction.BeaconC);
            if(k.lKey.wasPressedThisFrame) { Local.aim = Head.forward; Command(WatchAction.CastLine); }
            if(k.kKey.wasPressedThisFrame)Command(WatchAction.CatchLine);
            if(k.jKey.isPressed)Work(WatchAction.PullLine);
#endif
        }
        void BuildPartner()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.color=new Color(.95f,.64f,.17f);
            Transform Part(string name, PrimitiveType type, Vector3 scale)
            {
                var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(transform);go.transform.localScale=scale;
                Destroy(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;
            }
            remoteHead=Part("Other keeper head",PrimitiveType.Sphere,new(.23f,.27f,.23f));
            remoteBody=Part("Other keeper coat",PrimitiveType.Capsule,new(.38f,.48f,.3f));
            remoteLeft=Part("Other keeper left glove",PrimitiveType.Cube,new(.1f,.1f,.15f));
            remoteRight=Part("Other keeper right glove",PrimitiveType.Cube,new(.1f,.1f,.15f));
            rope=new GameObject("Rescue lifeline",typeof(LineRenderer)).GetComponent<LineRenderer>();rope.transform.SetParent(transform);
            rope.sharedMaterial=material;rope.startWidth=rope.endWidth=.018f;rope.positionCount=2;
        }
        void UpdatePartner()
        {
            bool visible=link.PartnerConnected && Partner!=null;
            remoteHead.gameObject.SetActive(visible);remoteBody.gameObject.SetActive(visible);remoteLeft.gameObject.SetActive(visible);remoteRight.gameObject.SetActive(visible);
            if(visible)
            {
                float blend=1-Mathf.Exp(-18*Time.deltaTime);var p=Partner;
                remoteHead.SetPositionAndRotation(Vector3.Lerp(remoteHead.position,p.head,blend),Quaternion.Slerp(remoteHead.rotation,p.headRotation,blend));
                remoteBody.position=p.head-Vector3.up*.65f;remoteLeft.SetPositionAndRotation(p.left,p.leftRotation);remoteRight.SetPositionAndRotation(p.right,p.rightRotation);
            }
            rope.enabled=visible&&State.ropeOwner>=0;
            if(rope.enabled){rope.SetPosition(0,Local.right);rope.SetPosition(1,Partner.head-Vector3.up*.25f);}
        }
        void OnDisable(){if(flood)flood.Paused=true;}
    }
}
