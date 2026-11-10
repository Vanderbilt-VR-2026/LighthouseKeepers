using System.Collections.Generic;
using LighthouseKeepers.Core;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using LighthouseKeepers.Gameplay.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    // One side shaft, authored circular openings, three guarded entries. Never bypass floor collision.
    public sealed class PoleSlide:MonoBehaviour
    {
        public static readonly Vector3 ShaftAxis=new Vector3(1.7f,0,1.1f);
        public const float OpeningRadius=.58f;
        public static readonly float[] EntryHeights={3.3f,6.6f,9.9f};
        public float Top=>9.9f;
        public float Bottom=>.04f;
        public bool Latched=>latched;
        public float Speed=>speed;
        public bool CanEnterAt(float feetY){foreach(float y in EntryHeights)if(Mathf.Abs(feetY-y)<.38f)return true;return false;}
        WatchDemoController watch;XROrigin rig;XRSimpleInteractable grab;XRBodyTransformer body;GravityProvider gravity;ContinuousMoveProvider move;
        readonly XROriginMovement motion=new();readonly List<Collider> openedGuards=new();readonly List<Transform> gateHinges=new();readonly List<Collider> guards=new();
        CharacterController capsule;AudioSource sound;float speed,stalled,previousY;bool latched,oldGravity,oldMove,exiting;Vector3 exitTarget;Transform openedHinge,bedroomHatch;Mesh trimMesh,hatchMesh;
        public static void Create(WatchDemoController watch,XROrigin rig)
        {
            var go=new GameObject("Continuous brass descent pole F4 to F1");go.SetActive(false);var p=go.AddComponent<PoleSlide>();p.watch=watch;p.rig=rig;p.Build();go.SetActive(true);
        }
        void Build()
        {
            transform.position=ShaftAxis+Vector3.left*.28f;
            var parts=gameObject.AddComponent<M2StationParts>();parts.Initialize();parts.Brass.SetColor("_BaseColor",new Color(.32f,.20f,.07f));parts.Brass.SetColor("_EmissionColor",Color.black);parts.Brass.DisableKeyword("_EMISSION");parts.Brass.SetFloat("_Smoothness",.25f);parts.Brass.SetFloat("_Metallic",.55f);
            const float ceiling=12.9f;
            parts.Part("Continuous brass slide shaft",transform,Vector3.up*((ceiling+Bottom)*.5f),new Vector3(.055f,(ceiling-Bottom)*.5f,.055f),parts.Brass,PrimitiveType.Cylinder);
            foreach(float y in new[]{Bottom,ceiling})parts.Part("Pole mounting shoe",transform,Vector3.up*y,new Vector3(.14f,.025f,.14f),parts.Dark,PrimitiveType.Cylinder);
            foreach(float y in EntryHeights)BuildLanding(parts,y);
            var trigger=gameObject.AddComponent<BoxCollider>();trigger.isTrigger=true;trigger.center=Vector3.up*5.6f;trigger.size=new Vector3(.22f,11.2f,.22f);
            grab=gameObject.AddComponent<XRSimpleInteractable>();grab.colliders.Clear();grab.colliders.Add(trigger);grab.selectMode=InteractableSelectMode.Multiple;grab.selectEntered.AddListener(Grab);grab.enabled=watch.State.Master&&watch.State.Master.movement;
            capsule=rig.GetComponentInChildren<CharacterController>();body=rig.GetComponentInChildren<XRBodyTransformer>();gravity=rig.GetComponentInChildren<GravityProvider>();move=rig.GetComponentInChildren<ContinuousMoveProvider>();
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.loop=true;sound.spatialBlend=1;sound.minDistance=.5f;sound.maxDistance=6;sound.volume=.09f;sound.clip=Resources.Load<AudioClip>("ImmersionAudio/creak");KeeperAudioMix.Route(sound,"SFX_World");
        }
        void BuildLanding(M2StationParts parts,float y)
        {
            var root=M2StationParts.Root("Circular pole landing F"+(1+Mathf.RoundToInt(y/3.3f)),transform,Vector3.zero);root.position=ShaftAxis+Vector3.up*y;
            // The bedroom uses a flush floor hatch, not a cage beside the bunk.
            if(Mathf.Abs(y-6.6f)<.01f)BuildBedroomHatch(parts,root);
            else {
            // 300 degree railing, open west toward the room; gate closes the remaining arc.
            for(int i=0;i<12;i++){
                float a=(i*30-150)*Mathf.Deg2Rad,b=(i*30-120)*Mathf.Deg2Rad;
                if(i>=10)continue;
                Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.67f,q=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*.67f;
                if(i%2==0)parts.Part("Pole guard upright",root,p+Vector3.up*.55f,new Vector3(.035f,1.1f,.035f),parts.Dark);
                foreach(float height in new[]{.55f,1.08f}){var rail=parts.Part("Circular shaft rail",root,(p+q)*.5f+Vector3.up*height,new Vector3(.035f,.035f,(q-p).magnitude+.02f),parts.Dark);rail.transform.localRotation=Quaternion.LookRotation(q-p);}
                var guard=new GameObject("Pole opening fall guard").AddComponent<BoxCollider>();guard.transform.SetParent(root,false);guard.transform.localPosition=(p+q)*.5f+Vector3.up*.58f;guard.transform.localRotation=Quaternion.LookRotation(q-p);guard.size=new Vector3(.07f,1.16f,(q-p).magnitude+.03f);guards.Add(guard);
            }
            // West entrance: reachable shaft 0.42 m beyond a standing keeper, unobstructed by signage.
            var hinge=M2StationParts.Root("Pole entrance gate",root,new Vector3(-.58f,.80f,-.36f));gateHinges.Add(hinge);
            var bar=parts.Part("Pole gate bar",hinge,new Vector3(0,0,.36f),new Vector3(.035f,.035f,.72f),parts.Dark);
            var gate=bar.AddComponent<BoxCollider>();gate.size=new Vector3(1,30,1);guards.Add(gate);
            }
            parts.Part("Pole sign post",root,new Vector3(-.58f,.64f,.52f),new Vector3(.035f,1.28f,.035f),parts.Dark);
            var sign=M2StationParts.Root("Pole side-mounted grip plate",root,new Vector3(-.60f,1.24f,.52f));sign.localRotation=Quaternion.Euler(0,-90,0);
            parts.Part("Pole engraved plate",sign,Vector3.zero,new Vector3(.32f,.18f,.018f),parts.Brass);
            parts.Text("Pole grip instruction",KeeperStrings.Get("pole.grip"),sign,new Vector3(0,0,.012f),.0024f);
            // A visible metal liner makes the cut read as a constructed circular shaft, not a missing floor.
            if(!trimMesh)trimMesh=BuildRim();var rim=new GameObject("Circular floor opening brass rim");rim.transform.SetParent(root,false);rim.AddComponent<MeshFilter>().sharedMesh=trimMesh;rim.AddComponent<MeshRenderer>().sharedMaterial=parts.Brass;
        }
        void BuildBedroomHatch(M2StationParts parts,Transform root)
        {
            bedroomHatch=M2StationParts.Root("Bedroom flush pole hatch",root,new Vector3(OpeningRadius,0,0));
            for(int i=-5;i<=5;i++){float z=i*.10f;float width=2*Mathf.Sqrt(OpeningRadius*OpeningRadius-z*z);parts.Part("Flush hatch grate",bedroomHatch,new Vector3(-OpeningRadius,.012f,z),new Vector3(width,.02f,.075f),parts.Dark);}
            var v=new Vector3[33];var triangles=new int[96];
            for(int i=0;i<32;i++){float a=i*Mathf.PI/16;v[i+1]=new Vector3(Mathf.Cos(a)*OpeningRadius,0,Mathf.Sin(a)*OpeningRadius);triangles[i*3]=0;triangles[i*3+1]=(i+1)%32+1;triangles[i*3+2]=i+1;}
            hatchMesh=new Mesh{name="Bedroom flush hatch support"};hatchMesh.vertices=v;hatchMesh.triangles=triangles;hatchMesh.RecalculateBounds();hatchMesh.RecalculateNormals();
            var support=new GameObject("Bedroom hatch fall protection").AddComponent<MeshCollider>();support.transform.SetParent(root,false);support.sharedMesh=hatchMesh;guards.Add(support);
        }
        void UpdateHatch(float dt)
        {
            if(!bedroomHatch)return;bool open=latched;
            if(!open&&watch.State.Coop is LighthouseKeepers.Lobby.ICoopKeeperLocations crew){
                crew.Locations(out int first,out Vector3 a,out int second,out Vector3 b);
                bool Near(Vector3 head)=>Vector2.Distance(new Vector2(head.x,head.z),new Vector2(ShaftAxis.x,ShaftAxis.z))<.42f&&Mathf.Abs(head.y-7.7f)<2;
                open=(first>0&&first!=crew.LocalActor&&Near(a))||(second>0&&second!=crew.LocalActor&&Near(b));
            }
            bedroomHatch.localRotation=Quaternion.RotateTowards(bedroomHatch.localRotation,Quaternion.Euler(0,0,open?-85:0),180*dt);
        }
        static Mesh BuildRim()
        {
            const int n=32;var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;var u=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var w=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));
                void Quad(Vector3 aa,Vector3 bb,Vector3 cc,Vector3 dd){int k=v.Count;v.AddRange(new[]{aa,bb,cc,dd});t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
                Quad(u*(OpeningRadius+.035f)+Vector3.up*.006f,w*(OpeningRadius+.035f)+Vector3.up*.006f,w*OpeningRadius+Vector3.up*.006f,u*OpeningRadius+Vector3.up*.006f);
                Quad(u*OpeningRadius,u*OpeningRadius-Vector3.up*.35f,w*OpeningRadius-Vector3.up*.35f,w*OpeningRadius);
            }
            var m=new Mesh{name="Circular pole floor liner"};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        void Grab(SelectEnterEventArgs e)
        {
            if(watch.State.Failed||watch.ObjectivePresentationBlocked||!body)return;
            if(!latched&&!CanEnterAt(rig.transform.position.y))return;
            if(!latched){oldGravity=gravity&&gravity.enabled;oldMove=move&&move.enabled;if(gravity)gravity.enabled=false;if(move)move.enabled=false;latched=true;
                float y=rig.transform.position.y;foreach(var hinge in gateHinges)if(Mathf.Abs(hinge.position.y-.8f-y)<.38f){openedHinge=hinge;hinge.Rotate(0,85,0);break;}
                // Personal passage only; the second keeper retains every guard collider.
                foreach(var guard in guards)if(capsule){Physics.IgnoreCollision(capsule,guard,true);openedGuards.Add(guard);}
                previousY=y;stalled=0;
            }
            var hand=e.interactorObject.transform.GetComponentInParent<HandRig>();if(hand)hand.Pulse(.18f,.08f);
        }
        void Update()
        {
            float dt=Time.deltaTime;UpdateHatch(dt);if(!latched)return;
            bool held=grab.isSelected&&!watch.ObjectivePresentationBlocked;
            var feet=rig.Camera.transform.position;feet.y=rig.transform.position.y;
            // Release near a landing is an intentional exit, not an indefinite mid-air latch.
            if(!held&&!exiting)foreach(float y in EntryHeights)if(Mathf.Abs(feet.y-y)<.45f){exitTarget=ShaftAxis+Vector3.left*1.08f+Vector3.up*(y+.04f);exiting=true;
                if(openedHinge){openedHinge.Rotate(0,-85,0);openedHinge=null;}
                foreach(var hinge in gateHinges)if(Mathf.Abs(hinge.position.y-.8f-y)<.01f){openedHinge=hinge;hinge.Rotate(0,85,0);break;}break;}
            if(exiting){speed=0;if(sound)sound.Stop();var toExit=exitTarget-feet;
                if(toExit.magnitude<.045f){Finish();return;}
                motion.motion=feet.y<exitTarget.y-.02f?Vector3.up*Mathf.Min(toExit.y,dt*.9f):Vector3.ClampMagnitude(toExit,dt*.9f);body.QueueTransformation(motion,100);return;}
            speed=Mathf.MoveTowards(speed,held?watch.State.Master.poleSpeed:0,watch.State.Master.poleAcceleration*dt);
            var p=rig.Camera.transform.position;p.y=rig.transform.position.y;var delta=Vector3.ProjectOnPlane(ShaftAxis-p,Vector3.up);float vertical=delta.magnitude<.08f?Mathf.Min(speed*dt,Mathf.Max(0,p.y-Bottom)):0;
            motion.motion=(held?Vector3.ClampMagnitude(delta,dt*.7f):Vector3.zero)+Vector3.down*vertical;body.QueueTransformation(motion,100);
            if(p.y<=Bottom+.08f){Finish();return;}if(held&&Mathf.Abs(p.y-previousY)<.0001f&&delta.magnitude<.08f)stalled+=dt;else stalled=0;previousY=p.y;if(stalled>1){Finish();return;}
            if(speed>.1f){if(sound.clip&&!sound.isPlaying)sound.Play();}else sound.Stop();
        }
        void Finish(){foreach(var guard in openedGuards)if(guard&&capsule)Physics.IgnoreCollision(capsule,guard,false);openedGuards.Clear();latched=false;exiting=false;speed=0;if(gravity)gravity.enabled=oldGravity;if(move)move.enabled=oldMove;if(openedHinge){openedHinge.Rotate(0,-85,0);openedHinge=null;}if(sound)sound.Stop();}
        void OnDestroy(){if(latched)Finish();if(grab)grab.selectEntered.RemoveListener(Grab);if(trimMesh)Destroy(trimMesh);if(hatchMesh)Destroy(hatchMesh);}
    }
}
