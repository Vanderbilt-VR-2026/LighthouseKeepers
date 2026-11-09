using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo
{
    // Additive House/doorway presentation. Does not modify the team's tower water renderer or shader.
    public sealed class WatchHouseFlood : MonoBehaviour
    {
        WatchDemoController watch;Mesh door,room;MeshRenderer doorRenderer,roomRenderer;Material material;
        readonly Vector3[] doorVertices=new Vector3[4],roomVertices=new Vector3[4];
        static readonly int FlowId=Shader.PropertyToID("_Flow");
        // Presentation-only floor clearance for the doorway sheet edge; 4 mm z-fought the throat/House floors.
        public const float MinSheetHeight=.01f;
        public static WatchHouseFlood Create(WatchDemoController w)
        {
            var view=new GameObject("House flood through tower doorway").AddComponent<WatchHouseFlood>();view.transform.SetParent(w.transform,false);view.watch=w;
            var original=GameObject.Find("Visible first-floor flood").GetComponent<Renderer>().sharedMaterial;
            view.material=new Material(Resources.Load<Shader>("DoorwayWater")){name="House receiving water"};
            view.material.SetColor("_BaseColor",original.HasProperty("_BaseColor")?original.GetColor("_BaseColor"):new Color(.14f,.28f,.30f));
            view.door=view.Patch("Doorway flowing sheet",out view.doorRenderer);view.room=view.Patch("House spreading front",out view.roomRenderer);return view;
        }
        Mesh Patch(string name,out MeshRenderer renderer)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            var mesh=new Mesh{name=name};mesh.MarkDynamic();mesh.vertices=new Vector3[4];mesh.triangles=new[]{0,2,1,0,3,2};
            go.AddComponent<MeshFilter>().sharedMesh=mesh;renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.enabled=false;return mesh;
        }
        void LateUpdate()
        {
            var s=watch.State.HouseFlood;if(s==null)return;
            doorRenderer.enabled=s.DoorFront>.005f;roomRenderer.enabled=s.RoomFront>.005f;
            float y=s.Height;
            Set(door,doorVertices,HouseFloodState.DoorWidth*.5f,HouseFloodState.DoorWidth*.5f,HouseFloodState.DoorNorthZ,HouseFloodState.DoorNorthZ-s.DoorFront,Mathf.Max(MinSheetHeight,watch.WaterHeight),y);
            // Clip to the House footprint; a front advances from the door toward the board/bench.
            Set(room,roomVertices,s.RoomNorthHalfWidth,s.RoomSouthHalfWidth,HouseFloodState.HouseNorthZ,HouseFloodState.HouseNorthZ-s.RoomFront,y,y);
            material.SetFloat(FlowId,watch.WaterHeight-s.Volume/HouseFloodState.Area);
        }
        static void Set(Mesh mesh,Vector3[] v,float northWidth,float southWidth,float northZ,float southZ,float northY,float southY)
        {v[0]=new Vector3(-northWidth,northY,northZ);v[1]=new Vector3(northWidth,northY,northZ);v[2]=new Vector3(southWidth,southY,southZ);v[3]=new Vector3(-southWidth,southY,southZ);mesh.vertices=v;mesh.RecalculateBounds();}
        void OnDestroy(){if(door)Destroy(door);if(room)Destroy(room);if(material)Destroy(material);}
    }
}
