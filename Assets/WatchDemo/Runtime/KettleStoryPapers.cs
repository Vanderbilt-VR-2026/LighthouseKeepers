using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using LighthouseKeepers.Interaction;
namespace LighthouseKeepers.WatchDemo
{
    // Four physical optional clues. Poses reuse the existing host grab lease; no lore gates progression.
    // All four roots exist on every client before prop registration, including hidden later-night papers.
    public sealed class KettleStoryPapers:MonoBehaviour
    {
        Material paper; Mesh[] meshes;
        public static void Create(WatchDemoController watch)
        {
            var atlas=Resources.Load<Texture2D>("KettleStory/papers");if(!atlas)return;
            var root=new GameObject("Kettle Rock story papers");root.transform.SetParent(watch.transform,false);
            var p=root.AddComponent<KettleStoryPapers>();p.Build(watch,atlas);
        }
        void Build(WatchDemoController watch,Texture2D atlas)
        {
            paper=new Material(Shader.Find("Universal Render Pipeline/Simple Lit")){name="Aged paper and ink atlas"};paper.SetTexture("_BaseMap",atlas);paper.SetColor("_BaseColor",Color.white);paper.SetFloat("_Smoothness",.08f);
            meshes=new Mesh[4];for(int i=0;i<4;i++)meshes[i]=PageMesh(i);
            var board=watch.BoardTransform;
            var desk=GameObject.Find("Wrench table top");var tabletop=desk?desk.GetComponent<Renderer>().bounds:new Bounds(new Vector3(-1,1,-8),Vector3.one);
            // Orders clip to the wall beside the existing board; letter lies at the back of the real bench.
            Add(watch,0,"Standing orders",1,new Vector3(3.34f,1.65f,-8.6f),Quaternion.Euler(0,90,0));
            Add(watch,1,"Lucy Gale letter",1,new Vector3(tabletop.center.x+.25f,tabletop.max.y+.008f,tabletop.max.z-.20f),Quaternion.Euler(90,0,0));
            var generator=FindAnyObjectByType<WatchGenerator>().transform;
            Add(watch,2,"Walter engine log",1,generator.TransformPoint(new Vector3(-1.15f,1.13f,.99f)),generator.rotation*Quaternion.Euler(0,180,0));
            var hook=generator.TransformPoint(new Vector3(-1.15f,1.36f,.99f));
            Bracket("Engine clipboard support",generator.TransformPoint(new Vector3(-.9f,1.36f,.85f)),hook);
            Bracket("Engine clipboard hook",hook,generator.TransformPoint(new Vector3(-1.15f,1.29f,.99f)));
            var mattress=GameObject.Find("Mattress and blanket");var bunk=mattress?mattress.GetComponent<Renderer>().bounds:new Bounds(BunkPlacement.FramePosition+Vector3.up*.28f,Vector3.one*.3f);
            Add(watch,3,"Tom unsent letter",1,new Vector3(bunk.center.x,bunk.max.y+.012f,bunk.center.z+.25f),Quaternion.Euler(90,0,0));
        }
        void Bracket(string name,Vector3 start,Vector3 end)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(transform,false);
            go.transform.SetPositionAndRotation((start+end)*.5f,Quaternion.LookRotation(end-start));go.transform.localScale=new Vector3(.018f,.018f,Vector3.Distance(start,end));
            go.GetComponent<Collider>().enabled=false;Destroy(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("VisualMaterials/Iron");
        }
        void Add(WatchDemoController w,int tile,string name,int night,Vector3 position,Quaternion rotation)
        {
            if(tile==0||tile==2){
                var backing=GameObject.CreatePrimitive(PrimitiveType.Cube);backing.name=name+" fixed clipboard";backing.transform.SetParent(transform,false);backing.transform.SetPositionAndRotation(position+rotation*Vector3.forward*.012f,rotation);backing.transform.localScale=new Vector3(.26f,.35f,.018f);backing.GetComponent<Collider>().enabled=false;Destroy(backing.GetComponent<Collider>());backing.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("VisualMaterials/Wood");
                var clip=GameObject.CreatePrimitive(PrimitiveType.Cube);clip.name=name+" retaining clip";clip.transform.SetParent(transform,false);clip.transform.SetPositionAndRotation(position+rotation*new Vector3(0,.16f,-.006f),rotation);clip.transform.localScale=new Vector3(.07f,.035f,.012f);clip.GetComponent<Collider>().enabled=false;Destroy(clip.GetComponent<Collider>());clip.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("VisualMaterials/Iron");
            }
            var go=new GameObject(name);go.SetActive(false);go.transform.SetParent(transform,false);go.transform.SetPositionAndRotation(position,rotation);
            go.AddComponent<MeshFilter>().sharedMesh=meshes[tile];var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=paper;renderer.shadowCastingMode=ShadowCastingMode.Off;
            var body=go.AddComponent<Rigidbody>();body.mass=.06f;body.isKinematic=true;body.useGravity=true;body.linearDamping=1;body.angularDamping=2;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
            var col=go.AddComponent<BoxCollider>();col.size=new Vector3(.24f,.32f,.006f);
            foreach(var rig in FindObjectsByType<CharacterController>(FindObjectsSortMode.None))Physics.IgnoreCollision(col,rig,true);
            var grab=go.AddComponent<XRGrabInteractable>();grab.colliders.Add(col);grab.movementType=XRBaseInteractable.MovementType.Kinematic;grab.useDynamicAttach=true;grab.matchAttachPosition=true;grab.matchAttachRotation=true;grab.throwOnDetach=false;
            var note=go.AddComponent<KettleReadable>();note.Initialize(w,night,body,renderer,col,grab);go.SetActive(true);
        }
        static Mesh PageMesh(int tile)
        {
            // Same legible front/back sheet, tiny thickness, no transparent overlay or live text renderer.
            float x=(tile%2)*.5f, y=(1-tile/2)*.5f, inset=.002f;
            var m=new Mesh{name="Story paper tile "+tile};
            m.vertices=new[]{new Vector3(-.12f,-.16f,-.001f),new Vector3(.12f,-.16f,-.001f),new Vector3(.12f,.16f,-.001f),new Vector3(-.12f,.16f,-.001f),new Vector3(.12f,-.16f,.001f),new Vector3(-.12f,-.16f,.001f),new Vector3(-.12f,.16f,.001f),new Vector3(.12f,.16f,.001f)};
            var uv=new[]{new Vector2(x+inset,y+inset),new Vector2(x+.5f-inset,y+inset),new Vector2(x+.5f-inset,y+.5f-inset),new Vector2(x+inset,y+.5f-inset)};m.uv=new[]{uv[0],uv[1],uv[2],uv[3],uv[0],uv[1],uv[2],uv[3]};m.triangles=new[]{0,2,1,0,3,2,4,6,5,4,7,6};m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        void OnDestroy(){if(paper)Destroy(paper);if(meshes!=null)foreach(var mesh in meshes)if(mesh)Destroy(mesh);}
    }
    public sealed class KettleReadable:MonoBehaviour,INetworkReleasedBody,INetworkPropRelease
    {
        WatchDemoController watch;int firstNight;Rigidbody body;Renderer face;Collider shape;XRGrabInteractable grab;INetworkPropStatus network;
        Vector3 home;Quaternion rotation;bool released;float dropped;
        public bool ReleasedBodyIsKinematic=>!released;
        public void Initialize(WatchDemoController w,int night,Rigidbody b,Renderer r,Collider c,XRGrabInteractable g)
        {watch=w;firstNight=night;body=b;face=r;shape=c;grab=g;home=transform.position;rotation=transform.rotation;grab.selectExited.AddListener(Released);}
        void Released(SelectExitEventArgs e){OnNetworkReleased();}
        public void OnNetworkReleased(){network??=GetComponent<INetworkPropStatus>();released=true;dropped=Time.time;if(network?.CanSimulateReleased!=false)body.isKinematic=false;}
        void LateUpdate()
        {
            bool visible=watch.State.Day>=firstNight;face.enabled=shape.enabled=visible;grab.enabled=visible;
            network??=GetComponent<INetworkPropStatus>();
            if(!released||grab.isSelected||network?.CanSimulateReleased==false)return;
            body.isKinematic=false;
            var head=Camera.main;bool observed=head&&Vector3.Distance(head.transform.position,transform.position)<4&&Vector3.Dot(head.transform.forward,(transform.position-head.transform.position).normalized)>.5f;
            if(Time.time-dropped>20&&!observed||transform.position.y<home.y-4)
            {body.linearVelocity=body.angularVelocity=Vector3.zero;released=false;body.isKinematic=true;transform.SetPositionAndRotation(home,rotation);}
        }
        void OnDestroy(){if(grab)grab.selectExited.RemoveListener(Released);}
    }
}
