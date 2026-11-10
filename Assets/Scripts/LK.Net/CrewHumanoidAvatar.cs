using UnityEngine;
using Unity.XR.CoreUtils;
using NVector3 = System.Numerics.Vector3;

namespace LighthouseKeepers.Net
{
    /// <summary>Cosmetic three-point VR body: tracked eyes/wrists, inferred hips, knees and feet.
    /// Owns no gameplay state, input, collision, Animator root motion or network authority.</summary>
    [DisallowMultipleComponent]
    public sealed class CrewHumanoidAvatar : MonoBehaviour
    {
        [SerializeField] TextAsset rigSource;
        [SerializeField] Material surface;
        [SerializeField, Tooltip("Static walkable layers, matching GroundedSlopeMovement.")] LayerMask groundLayers=(1<<8)|1;
        [SerializeField, Range(1,1.8f)] float maximumArmStretch=1.5f;
        [SerializeField, Range(.1f,.4f)] float stepDistance=.18f;
        [SerializeField, Range(.12f,.5f)] float stepSeconds=.22f;
        [SerializeField, Range(.02f,.2f)] float stepHeight=.08f;
        [SerializeField, Tooltip("Additional controller-to-glove rotation correction in degrees.")] Vector3 leftWristCorrection,rightWristCorrection;
        CrewHumanoidRig rig;
        Mesh ownedMesh;
        Transform[] bones;
        Vector3[] positions;
        Quaternion[] rotations;
        float[] stretches;
        Vector3[] localPositions;
        Quaternion[] localRotations;
        byte[] curlChannels;
        readonly RaycastHit[] groundHits=new RaycastHit[16];
        readonly CrewFootstep leftStep=new(),rightStep=new();
        Transform ignoredOrigin;
        int head,neck,leftArm,leftElbow,leftWrist,rightArm,rightElbow,rightWrist,leftHip,leftKnee,leftAnkle,rightHip,rightKnee,rightAnkle,leftToes,rightToes;
        Vector3 eyeInHead,lastHead;
        Quaternion leftWristOffset,rightWristOffset;
        float floorY,lastPoseTime;
        bool placed,attempted;
        public SkinnedMeshRenderer Skin {get;private set;}
        public Vector3 EyePosition=>bones==null?transform.position:bones[head].TransformPoint(eyeInHead);
        public Transform Joint(string name)=>rig!=null&&rig.Indices.TryGetValue(name,out int index)?bones[index]:null;
        public bool TryValidate(out string error)
        {
            if(!surface){error="Assign the full-body surface material.";return false;}
            return CrewHumanoidRig.TryRead(rigSource,out _,out error);
        }
        public bool TryInitialize(out string error)
        {
            error=null;if(rig!=null)return true;
            if(attempted){error="Full-body initialization previously failed.";return false;}attempted=true;
            if(!surface){error="Assign the full-body surface material.";return false;}
            if(!CrewHumanoidRig.TryRead(rigSource,out var data,out error))return false;
            rig=data;int count=rig.joints.Length;
            bones=new Transform[count];positions=new Vector3[count];rotations=new Quaternion[count];stretches=new float[count];
            localPositions=new Vector3[count];localRotations=new Quaternion[count];curlChannels=new byte[count];
            for(int i=0;i<count;i++)
            {
                var joint=rig.joints[i];
                if(joint.name.Contains("Proximal")||joint.name.Contains("Intermediate")||joint.name.Contains("Distal"))
                    curlChannels[i]=(byte)((joint.name.StartsWith("Left")?0:3)+(joint.name.Contains("Index")?2:joint.name.Contains("Thumb")?3:1));
                // Sibling skin transforms avoid inherited shear when one long bone stretches.
                var bone=new GameObject(joint.name).transform;bone.SetParent(transform,false);bones[i]=bone;
                bone.SetLocalPositionAndRotation(joint.position,joint.rotation);
                if(joint.parent>=0)
                {
                    var parent=rig.joints[joint.parent];var inverse=Quaternion.Inverse(parent.rotation);
                    localPositions[i]=inverse*(joint.position-parent.position);localRotations[i]=inverse*joint.rotation;
                }
            }
            head=Index("Head");neck=Index("Neck");leftToes=Index("LeftToes");rightToes=Index("RightToes");
            leftArm=Index("LeftUpperArm");leftElbow=Index("LeftLowerArm");leftWrist=Index("LeftHand");
            rightArm=Index("RightUpperArm");rightElbow=Index("RightLowerArm");rightWrist=Index("RightHand");
            leftHip=Index("LeftUpperLeg");leftKnee=Index("LeftLowerLeg");leftAnkle=Index("LeftFoot");
            rightHip=Index("RightUpperLeg");rightKnee=Index("RightLowerLeg");rightAnkle=Index("RightFoot");
            eyeInHead=Quaternion.Inverse(rig.joints[head].rotation)*(rig.eyeReference-rig.joints[head].position);
            // In the generated hanging rest pose the fingers point down. Map them to controller +Z.
            leftWristOffset=Quaternion.Euler(-90,0,0)*rig.joints[leftWrist].rotation;
            rightWristOffset=Quaternion.Euler(-90,0,0)*rig.joints[rightWrist].rotation;
            ownedMesh=rig.BuildMesh();Skin=gameObject.AddComponent<SkinnedMeshRenderer>();
            Skin.sharedMesh=ownedMesh;Skin.sharedMaterial=surface;Skin.bones=bones;Skin.rootBone=transform;
            Skin.quality=SkinQuality.Bone4;Skin.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            Skin.localBounds=new Bounds(new Vector3(0,.9f,0),new Vector3(3,3,3));
            // Release large parsed mesh arrays after upload. Only the small rest skeleton remains.
            rig.vertices=rig.normals=rig.tangents=rig.uv=rig.boneWeights=null;rig.boneIndices=rig.triangles=null;
            return true;
        }
        int Index(string name)=>rig.Indices[name];
        void OnEnable()=>placed=false;
        void OnDestroy(){if(ownedMesh){if(Application.isPlaying)Destroy(ownedMesh);else DestroyImmediate(ownedMesh);}}
        static NVector3 N(Vector3 v)=>new NVector3(v.x,v.y,v.z);
        static Vector3 U(NVector3 v)=>new Vector3(v.X,v.Y,v.Z);

        float Ground(Vector3 point,float expected,float rise=.45f)
        {
            // Ignore local tracking-rig colliders and dynamic props. Scan all hits, not the first wall/prop.
            int count=Physics.RaycastNonAlloc(new Vector3(point.x,point.y+.1f,point.z),Vector3.down,groundHits,3.5f,groundLayers,QueryTriggerInteraction.Ignore);
            float best=float.NegativeInfinity,result=expected;
            for(int i=0;i<count;i++)
            {
                var hit=groundHits[i];var collider=hit.collider;
                if(hit.normal.y<.55f||collider.attachedRigidbody||ignoredOrigin&&collider.transform.IsChildOf(ignoredOrigin))continue;
                // Prefer the upper walkable surface within one step, so a lower floor
                // underneath stairs does not hold the boots downstairs. Reject tabletops.
                float height=hit.point.y;
                if(height<=expected+rise&&height>=expected-1.1f&&height>best){best=height;result=height;}
            }
            return result;
        }
        Vector3 FootGoal(int ankle,Vector3 velocity)
        {
            var point=transform.TransformPoint(rig.joints[ankle].position)+Vector3.ProjectOnPlane(velocity,Vector3.up)*.10f;
            point.y=Ground(point+Vector3.up*.65f,floorY)+rig.joints[ankle].position.y-rig.floorHeight;
            return point;
        }
        public void ApplyPose(KeeperPoseFrame pose,float seconds)
        {
            if(!pose.Valid||!float.IsFinite(seconds)||seconds<0)return;
            if(rig==null&&!TryInitialize(out _))return;
            bool reset=!placed||Time.unscaledTime-lastPoseTime>.5f||(pose.Head-lastHead).sqrMagnitude>2.25f;
            lastPoseTime=Time.unscaledTime;
            if(reset)
            {
                var origin=FindFirstObjectByType<XROrigin>();ignoredOrigin=origin?origin.transform:null;
                floorY=pose.Head.y-rig.eyeReference.y+rig.floorHeight;
            }
            var forward=Vector3.ProjectOnPlane(pose.HeadRotation*Vector3.forward,Vector3.up);
            var yaw=forward.sqrMagnitude>.01f?Quaternion.LookRotation(forward,Vector3.up):transform.rotation;
            var rotation=reset?yaw:Quaternion.Slerp(transform.rotation,yaw,1-Mathf.Exp(-6*seconds));
            floorY=Ground(pose.Head,floorY,reset?.8f:.45f);
            transform.SetPositionAndRotation(new Vector3(pose.Head.x,floorY-rig.floorHeight,pose.Head.z),rotation);
            var velocity=reset||seconds<=.0001f?Vector3.zero:Vector3.ClampMagnitude((pose.Head-lastHead)/seconds,3);
            lastHead=pose.Head;
            float squat=Mathf.Clamp(pose.Head.y-floorY-(rig.eyeReference.y-rig.floorHeight),-.58f,.25f);
            var shift=Vector3.up*squat;
            for(int i=0;i<bones.Length;i++)
            {
                positions[i]=transform.TransformPoint(rig.joints[i].position)+shift;
                rotations[i]=rotation*rig.joints[i].rotation;stretches[i]=1;
            }
            rotations[head]=pose.HeadRotation*rig.joints[head].rotation;
            positions[head]=pose.Head-rotations[head]*eyeInHead;
            Aim(neck,head,1.3f);
            Limb(leftArm,leftElbow,leftWrist,pose.Left,positions[leftArm]+rotation*new Vector3(-.3f,-.3f,-.25f),maximumArmStretch);
            Limb(rightArm,rightElbow,rightWrist,pose.Right,positions[rightArm]+rotation*new Vector3(.3f,-.3f,-.25f),maximumArmStretch);
            rotations[leftWrist]=pose.LeftRotation*Quaternion.Euler(leftWristCorrection)*leftWristOffset;
            rotations[rightWrist]=pose.RightRotation*Quaternion.Euler(rightWristCorrection)*rightWristOffset;
            var leftGoal=FootGoal(leftAnkle,velocity);var rightGoal=FootGoal(rightAnkle,velocity);
            if(reset){leftStep.Reset(N(leftGoal));rightStep.Reset(N(rightGoal));}
            if(!leftStep.Moving&&!rightStep.Moving)
            {
                float l=Vector3.Distance(U(leftStep.Position),leftGoal),r=Vector3.Distance(U(rightStep.Position),rightGoal);
                if(Mathf.Max(l,r)>stepDistance)
                {if(l>=r)leftStep.Begin(N(leftGoal),stepSeconds,stepHeight);else rightStep.Begin(N(rightGoal),stepSeconds,stepHeight);}
            }
            leftStep.Tick(seconds);rightStep.Tick(seconds);
            Limb(leftHip,leftKnee,leftAnkle,U(leftStep.Position),positions[leftHip]+rotation*Vector3.forward,1.4f);
            Limb(rightHip,rightKnee,rightAnkle,U(rightStep.Position),positions[rightHip]+rotation*Vector3.forward,1.4f);
            // Apply logical FK only to finger and toe chains; arms/legs above use world-space IK.
            var leftCurls=FingerCurls(pose.LeftPose,pose.LeftGrip,pose.LeftTrigger);
            var rightCurls=FingerCurls(pose.RightPose,pose.RightGrip,pose.RightTrigger);
            for(int i=0;i<bones.Length;i++)
            {
                var joint=rig.joints[i];int channel=curlChannels[i];bool finger=channel>0;
                if(finger||i==leftToes||i==rightToes)
                {
                    int parent=joint.parent;
                    positions[i]=positions[parent]+rotations[parent]*localPositions[i];
                    var curls=channel<=3?leftCurls:rightCurls;
                    float curl=finger?curls[(channel-1)%3]:0;
                    rotations[i]=rotations[parent]*localRotations[i]*Quaternion.AngleAxis(curl*75,joint.curlAxis);
                }
                bones[i].SetPositionAndRotation(positions[i],rotations[i]);bones[i].localScale=new Vector3(1,stretches[i],1);
            }
            placed=true;
        }
        static Vector3 FingerCurls(byte pose,float grip,float trigger)
        {
            if(pose==1||pose==2||pose==4){grip=Mathf.Max(grip,.85f);trigger=Mathf.Max(trigger,.85f);}
            if(pose==3)trigger=Mathf.Max(trigger,.85f);
            return new Vector3(grip,trigger,Mathf.Max(trigger,grip)*.65f);
        }
        void Aim(int bone,int end,float maxStretch)
        {
            var delta=positions[end]-positions[bone];var rest=rig.joints[end].position-rig.joints[bone].position;
            var restWorld=transform.rotation*rest;
            if(delta.sqrMagnitude>.000001f)rotations[bone]=Quaternion.FromToRotation(restWorld,delta)*transform.rotation*rig.joints[bone].rotation;
            stretches[bone]=Mathf.Clamp(delta.magnitude/rest.magnitude,1,maxStretch);
        }
        void Limb(int upper,int lower,int end,Vector3 target,Vector3 pole,float maxStretch)
        {
            float a=Vector3.Distance(rig.joints[upper].position,rig.joints[lower].position);
            float b=Vector3.Distance(rig.joints[lower].position,rig.joints[end].position);
            var solution=CrewLimbIK.Solve(N(positions[upper]),N(target),N(pole),a,b,maxStretch);
            positions[lower]=U(solution.Bend);positions[end]=U(solution.End);
            Aim(upper,lower,maxStretch);Aim(lower,end,maxStretch);
        }
    }
}
