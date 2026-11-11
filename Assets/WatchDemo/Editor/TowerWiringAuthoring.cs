using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LighthouseKeepers.Core;
using LighthouseKeepers.WatchDemo;

namespace LighthouseKeepers.Editor
{
    public static partial class TowerWiringAuthoring
    {
        const string Folder="Assets/WatchDemo/Resources/TowerWiring";
        const float RiserAngle=325;
        static readonly float[] heights={2.85f,6.15f,9.45f,12.75f};
        static readonly Color Steel=new(.36f,.32f,.27f),Rust=new(.27f,.16f,.085f),Ceramic=new(.85f,.82f,.7f),Cloth=new(.12f,.085f,.05f);
        static Vector3 Wall(float angle,float y,float inset=.055f)
        {
            var dir=Quaternion.Euler(0,angle,0)*Vector3.forward;var origin=Vector3.up*y;
            var hits=Physics.RaycastAll(origin,dir,5,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("UCX_")&&(h.collider.name.Contains("Wall")||h.collider.name.Contains("Frame_Lantern"))).OrderBy(h=>h.distance).ToArray();
            if(hits.Length>0)return hits[0].point+hits[0].normal*inset;
            float radius=y<3.3f?3.83f:y<6.6f?3.67f:y<9.9f?3.62f:2.67f;return origin+dir*(radius-inset);
        }
        static float Angle(Vector3 p)=>Mathf.Atan2(p.x,p.z)*Mathf.Rad2Deg;
        static int Floor(Vector3 p)=>p.z< -4.4f?0:Mathf.Clamp(Mathf.FloorToInt(p.y/3.3f)+1,1,4);
        static Vector3[] Arc(float from,float to,float y,float inset=.055f)
        {float delta=Mathf.DeltaAngle(from,to);int n=Mathf.Max(1,Mathf.CeilToInt(Mathf.Abs(delta)/20));var p=new Vector3[n+1];for(int i=0;i<=n;i++)p[i]=Wall(from+delta*i/n,y,inset);return p;}
        [MenuItem("Lighthouse Keepers/M3/Wiring/Capture Lab Routes (Play Mode)")]
        public static string CaptureAndBake()
        {
            EditorUtility.audioMasterMute=true;
            if(!EditorApplication.isPlaying||!UnityEngine.Object.FindAnyObjectByType<WatchDemoController>())throw new InvalidOperationException("Run the Lab preview first so final wall mounts and lamp positions are available.");
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var data=AssetDatabase.LoadAssetAtPath<TowerWiringAsset>(Folder+"/Layout.asset");if(!data){data=ScriptableObject.CreateInstance<TowerWiringAsset>();AssetDatabase.CreateAsset(data,Folder+"/Layout.asset");}
            var runs=new List<TowerWiringAsset.Run>();var endpoints=new List<TowerWiringAsset.Endpoint>();
            void Add(string name,int circuit,int floor,float radius,IEnumerable<Vector3> points){var p=points.ToArray();if(p.Length>1)runs.Add(new TowerWiringAsset.Run{name=name,circuit=circuit,floor=floor,radius=radius,points=p});}
            var gen=GameObject.Find("STN_GENERATOR").transform;var panel=UnityEngine.Object.FindAnyObjectByType<WatchPowerPanel>().transform;
            var generator=gen.TransformPoint(new Vector3(0,1.25f,.18f));var breaker=panel.TransformPoint(new Vector3(0,1.65f,-.035f));data.breaker=panel.TransformPoint(new Vector3(0,1.5f,.24f));
            var feed=new List<Vector3>{generator,Wall(Angle(generator),generator.y)};feed.AddRange(Arc(Angle(generator),Angle(breaker)+20,5.95f));feed.Add(Wall(Angle(breaker)+20,breaker.y));feed.Add(breaker);Add("Generator output to F2 breaker",4,2,.025f,feed);
            for(int circuit=0;circuit<4;circuit++)
            {
                float offset=.055f;float angle=RiserAngle+circuit*1.35f;
                var socket=panel.TransformPoint(new Vector3(-.27f+circuit*.18f,1.83f,-.035f));float trunkY=heights[1]-.02f-circuit*.055f;var trunk=new List<Vector3>{socket,new Vector3(socket.x,trunkY,socket.z)};trunk.AddRange(Arc(Angle(socket),angle,trunkY));Add("Breaker to "+(PowerCircuit)circuit+" riser",circuit,2,.016f,trunk);
                // Continuous circuits run in parallel, collars sit on both faces of each slab.
                for(int floor=1;floor<=4;floor++){if(circuit==0&&floor>2||circuit==1&&floor<2)continue;var points=new List<Vector3>{Wall(angle,(floor-1)*3.3f+.035f,floor==4?.20f:offset)};
                    if(floor==4)points.Add(Wall(angle,10.22f,offset));points.Add(Wall(angle,heights[floor-1],offset));
                    if(floor==3){var inner=Wall(angle,9.935f,.20f);points.Add(new Vector3(inner.x,9.45f,inner.z));points.Add(inner);}
                    else if(floor<4)points.Add(Wall(angle,floor*3.3f+.035f,offset));
                    if(floor==1)points.Reverse();Add(((PowerCircuit)circuit)+" riser F"+floor,circuit,floor,.016f,points);}
            }
            // Lighting trunk follows the ceiling/wall perimeter. Equipment uses dedicated circuits.
            for(int floor=1;floor<=4;floor++)
            {var ring=new List<Vector3>();for(int a=0;a<=18;a++)ring.Add(Wall(RiserAngle+3*1.35f+a*20,heights[floor-1]));Add("F"+floor+" lighting perimeter",3,floor,.016f,ring);}
            var transforms=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach(var t in transforms.OrderBy(t=>t.name).ThenBy(t=>t.position.x).ThenBy(t=>t.position.y))
            {
                bool lamp=t.name=="Caged practical bulb"||t.name=="Frosted service lamp"||t.name.StartsWith("SM_Lamp_")||t.name.StartsWith("SM_EmergencyLamp_")||t.name.StartsWith("SM_Gallery_LampHousing_")||t.name.StartsWith("SM_Gallery_EmergencyLamp_");
                bool pa=t.name.StartsWith("Brass PA speaker ");if(!lamp&&!pa)continue;
                var renderer=t.GetComponent<Renderer>();var pos=renderer?renderer.bounds.center:t.position;int floor=Floor(pos);int circuit=lamp?3:2;
                endpoints.Add(new TowerWiringAsset.Endpoint{name=t.name,circuit=circuit,position=pos});
                if(floor==0)continue;float angle=Angle(pos);var points=new List<Vector3>();
                if(pa)points.AddRange(Arc(RiserAngle+2*1.35f,angle,heights[floor-1]-.11f));else points.Add(Wall(angle,heights[floor-1]));
                // Cabinet back connection, then a short flexible drop to the fitting.
                var wall=Wall(angle,pos.y+.18f);
                bool reveal=Physics.OverlapCapsule(Wall(angle,heights[floor-1]),wall,.016f,~0,QueryTriggerInteraction.Ignore).Any(c=>c.name.Contains("Reveal")||c.name.Contains("Glass"));
                if(reveal){float around=angle+20;points.AddRange(Arc(angle,around,heights[floor-1]));points.Add(Wall(around,wall.y));points.AddRange(Arc(around,angle,wall.y));}
                if(t.name.StartsWith("SM_Gallery_")){points.Add(Wall(angle,13.25f));points.Add(new Vector3(pos.x,13.25f,pos.z));wall=new Vector3(pos.x,pos.y+.10f,pos.z);}
                points.Add(wall);var socket=pos+Vector3.up*.10f;if(Vector3.Distance(wall,socket)>.25f){points.Add(socket);wall=socket;}Add(t.name+" conduit",circuit,floor,.012f,points);
                Add(t.name+(lamp?" braided lamp drop":" PA feed"),circuit,floor,lamp?.0035f:.009f,new[]{wall,pos+Vector3.up*.065f,pos});
            }
            void Equipment(string name,int circuit,Vector3 end)
            {
                int floor=Floor(end);float y=heights[floor-1]-.18f-circuit*.025f;
                var points=new List<Vector3>(Arc(RiserAngle+circuit*1.35f,Angle(end),y));points.Add(Wall(Angle(end),end.y));points.Add(end);
                Add(name,circuit,floor,.016f,points);endpoints.Add(new TowerWiringAsset.Endpoint{name=name,circuit=circuit,position=end});
            }
            var pump=GameObject.Find("STN_ELECTRIC_PUMP").transform;Equipment("F1 electric pump terminal",0,pump.position+Vector3.up*.52f);
            Equipment("Pump instrument backplate",0,pump.position+Vector3.up*.76f);
            var radio=UnityEngine.Object.FindAnyObjectByType<WatchRadioConsole>().transform;Equipment("Radio cabinet terminal",2,radio.TransformPoint(new Vector3(0,1.6f,-.15f)));
            // Central beacon receives its cable at the base, along the floor, never across headroom.
            var beaconPath=new List<Vector3>(Arc(RiserAngle+1.35f,90,12.57f));beaconPath.Add(Wall(90,10.02f));beaconPath.Add(new Vector3(0,10.02f,0));beaconPath.Add(new Vector3(0,10.2f,0));Add("Beacon motor floor conduit",1,4,.018f,beaconPath);endpoints.Add(new TowerWiringAsset.Endpoint{name="Beacon motor terminal",circuit=1,position=new Vector3(0,10.2f,0)});
            // A pair of exterior cables follows the tower, then the House eaves; no free diagonal span.
            for(int circuit=2;circuit<=3;circuit++)
            {
                float y=2.53f-(circuit-2)*.07f;var weather=new List<Vector3>(Arc(RiserAngle+circuit*1.35f,195,y));
                weather.AddRange(Arc(195,195,y,-.44f));weather.Add(new Vector3(-1.12f,y,-4.34f));weather.Add(new Vector3(-4.01f,y,-4.34f));weather.Add(new Vector3(-4.01f,y,-8));weather.Add(new Vector3(-3.43f,y,-8));Add("Weatherproof House "+(PowerCircuit)circuit,circuit,0,.018f,weather);
                Add("House eaves "+(PowerCircuit)circuit,circuit,0,.012f,new[]{new Vector3(-3.43f,y,-8),new Vector3(-3.43f,y,-5.62f),new Vector3(3.43f,y,-5.62f),new Vector3(3.43f,y,-9.80f),new Vector3(-3.43f,y,-9.80f),new Vector3(-3.43f,y,-8)});
                foreach(var e in endpoints.Where(e=>Floor(e.position)==0&&e.circuit==circuit))
                {
                    var p=e.position;bool outside=p.z< -10.3f;float side=p.x<0?-3.43f:3.43f;float z=Mathf.Abs(p.z+5.62f)<Mathf.Abs(p.z+9.80f)?-5.62f:-9.80f;
                    Vector3 start=Mathf.Abs(p.x)>3?new Vector3(side,y,Mathf.Clamp(p.z,-9.8f,-5.62f)):new Vector3(p.x,y,z);
                    if(e.name.Contains("Pendant"))start=new Vector3(0,y,-5.62f); // follows the centre ceiling joist to the existing pendant stem
                    Add(e.name+" House drop",circuit,0,.009f,new[]{start,new Vector3(p.x,y,p.z),p+Vector3.up*.05f,p});
                }
            }
            data.junctions=new[]{new Vector3(-3.40f,2.58f,-8),Wall(RiserAngle,2.58f),Wall(RiserAngle,5.88f),Wall(RiserAngle,9.18f),Wall(RiserAngle,12.48f)};data.runs=runs.ToArray();data.endpoints=endpoints.ToArray();EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();return Bake();
        }
        [MenuItem("Lighthouse Keepers/M3/Wiring/Rebuild Saved Waypoints")]
        public static string Bake()
        {
            var data=AssetDatabase.LoadAssetAtPath<TowerWiringAsset>(Folder+"/Layout.asset");if(!data)throw new InvalidOperationException("Capture the Lab once before rebuilding.");
            BuildAtlas();
            var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Conduit.mat");if(!material){material=new Material(Shader.Find("LighthouseKeepers/Tower conduit"));AssetDatabase.CreateAsset(material,Folder+"/Conduit.mat");}material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/Atlas.png"));EditorUtility.SetDirty(material);material.enableInstancing=true;data.material=material;
            var groups=Enumerable.Range(0,5).Select(_=>new Geometry()).ToArray();
            foreach(var run in data.runs)
            {
                var g=groups[run.floor];var points=Round(run.points);float length=0;for(int i=1;i<points.Count;i++)length+=Vector3.Distance(points[i-1],points[i]);float at=0;
                float baseTime=run.circuit==4?0:Mathf.Clamp01(.12f+Mathf.Abs(run.floor-2)*.19f);float timeSpan=run.circuit==4?.12f:.25f;
                for(int i=1;i<points.Count;i++)
                {
                    float d=Vector3.Distance(points[i-1],points[i]);if(d<.001f)continue;
                    var colour=run.floor==1?Color.Lerp(Steel,Rust,.25f):Steel;
                    if(run.radius<.005f)
                    {for(int strand=0;strand<2;strand++)g.Braid(points[i-1],points[i],run.radius,Cloth,run.circuit,baseTime+at/length*timeSpan,strand);}
                    else g.Tube(points[i-1],points[i],run.radius*.85f,colour,run.circuit,baseTime+at/length*timeSpan,baseTime+(at+d)/length*timeSpan,6);
                    at+=d;
                }
                // Saddles are baked into the same mesh; no per-strap Renderer or collider.
                if(run.radius>=.009f)for(float s=.3f;s<length;s+=.6f)
                {var p=Along(points,s,out var direction);var inward=run.floor==0?Vector3.right:-new Vector3(p.x,0,p.z).normalized;var q=Quaternion.LookRotation(inward);g.Saddle(p-inward*.018f,new Vector3(.075f,.025f,.035f),q,Steel,run.circuit,baseTime);}
                else
                {var p=run.points[0];g.Tube(p-Vector3.up*.03f,p+Vector3.up*.02f,.015f,Ceramic,run.circuit,baseTime,baseTime,6);}
            }
            for(int floor=0;floor<5;floor++)
            {
                var g=groups[floor];var p=data.junctions[floor];var normal=floor==0?Vector3.right:-new Vector3(p.x,0,p.z).normalized;var q=Quaternion.LookRotation(normal);p+=normal*.045f;
                g.Box(p,new Vector3(.28f,.21f,.065f),q,Steel,3,.5f);g.Box(p+normal*.039f,new Vector3(.25f,.18f,.012f),q,new Color(.25f,.28f,.25f),3,.5f);
                foreach(float x in new[]{-.105f,.105f})foreach(float y in new[]{-.07f,.07f})g.Tube(p+q*new Vector3(x,y,.044f),p+q*new Vector3(x,y,.05f),.008f,Steel,3,.5f,.5f,6);
                g.Label(p+normal*.048f,q,new Vector2(.22f,.075f),floor,3);
                if(floor>1)foreach(var run in data.runs.Where(r=>r.floor==floor&&r.name.Contains(" riser F")))
                {var c=run.points[0];foreach(float dy in new[]{-.07f,.025f})g.Tube(c+Vector3.up*dy,c+Vector3.up*(dy+.025f),.035f,Steel,run.circuit,.5f,.5f,8);}
            }
            foreach(var run in data.runs.Where(r=>r.name.StartsWith("Weatherproof")||r.name.StartsWith("SM_Gallery_")&&r.name.EndsWith("conduit")||r.name=="Brass PA speaker 5 House drop"))
            {
                var g=groups[run.floor];var p=run.points;
                // Feed-through bushings around intentional service holes; never in window glass.
                int a=p.Length-2;var d=(p[a+1]-p[a]).normalized;
                g.Tube(p[a]-d*.02f,p[a]+d*.02f,.036f,Steel,run.circuit,.65f,.65f,8);
                if(run.name.StartsWith("Weatherproof")){var entry=p.First(v=>new Vector2(v.x,v.z).magnitude>4.1f);g.Tube(entry-d*.02f,entry+d*.02f,.036f,Steel,run.circuit,.65f,.65f,8);}
            }
            var pumpTerminal=data.endpoints.First(e=>e.name=="F1 electric pump terminal").position;
            var pumpBase=pumpTerminal-Vector3.up*.52f;var pg=groups[1];var pumpRotation=Quaternion.LookRotation(-new Vector3(pumpBase.x,0,pumpBase.z).normalized);
            pg.Box(pumpBase+Vector3.up*.10f,new Vector3(.43f,.14f,.38f),pumpRotation,Steel,0,.7f);
            pg.Tube(pumpBase+Vector3.up*.29f+pumpRotation*new Vector3(-.13f,0,0),pumpBase+Vector3.up*.29f+pumpRotation*new Vector3(.13f,0,0),.13f,new Color(.12f,.18f,.16f),0,.7f,.7f,8);
            foreach(float side in new[]{-1f,1f})pg.Disk(pumpBase+Vector3.up*.29f+pumpRotation*new Vector3(side*.131f,0,0),pumpRotation*Vector3.right*side,.13f,new Color(.12f,.18f,.16f),0);
            for(int fin=0;fin<4;fin++)pg.Tube(pumpBase+Vector3.up*.29f+pumpRotation*new Vector3(-.13f+fin*.065f,0,0),pumpBase+Vector3.up*.29f+pumpRotation*new Vector3(-.117f+fin*.065f,0,0),.147f,Steel,0,.7f,.7f,8);
            pg.Box(pumpTerminal-Vector3.up*.05f,new Vector3(.13f,.11f,.10f),pumpRotation,Steel,0,.75f);
            var gauge=pumpBase+Vector3.up*.76f;var gn=pumpRotation*Vector3.forward;
            pg.Tube(gauge-gn*.035f,gauge+gn*.035f,.085f,new Color(.51f,.36f,.15f),0,.75f,.75f,8);pg.Box(gauge+gn*.037f,new Vector3(.115f,.115f,.006f),pumpRotation,Ceramic,0,.75f);
            pg.Box(gauge+gn*.043f,new Vector3(.006f,.07f,.004f),pumpRotation*Quaternion.Euler(0,0,-35),Cloth,0,.75f);
            pg.Tube(pumpTerminal,gauge-Vector3.up*.08f,.018f,Steel,0,.75f,.75f,6);
            int count=groups.Sum(g=>g.tris.Count/3);if(count>15000)throw new InvalidOperationException("Wiring budget exceeded: "+count);
            data.floors=new Mesh[5];for(int i=0;i<5;i++){var mesh=groups[i].Mesh("Conduit floor "+i);string path=Folder+"/Floor"+i+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){old.Clear();old.vertices=mesh.vertices;old.normals=mesh.normals;old.uv=mesh.uv;var uv2=new List<Vector3>();mesh.GetUVs(1,uv2);old.SetUVs(1,uv2);old.colors=mesh.colors;old.triangles=mesh.triangles;old.RecalculateBounds();EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);data.floors[i]=mesh;}
            data.triangles=count;EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();return "Wiring: "+data.runs.Length+" runs, "+data.endpoints.Length+" endpoints, "+count+" triangles, 5 meshes, 1 shared material; GPU device pending";
        }
        static List<Vector3> Round(Vector3[] source)
        {
            var p=source.Where((v,i)=>i==0||(v-source[i-1]).sqrMagnitude>.000001f).ToArray();var output=new List<Vector3>();if(p.Length<2)return output;output.Add(p[0]);
            for(int i=1;i<p.Length-1;i++){if(Vector3.Angle(p[i]-p[i-1],p[i+1]-p[i])<22){output.Add(p[i]);continue;}float r=Mathf.Min(.06f,Mathf.Min(Vector3.Distance(p[i-1],p[i]),Vector3.Distance(p[i],p[i+1]))*.25f);var a=p[i]+(p[i-1]-p[i]).normalized*r;var b=p[i]+(p[i+1]-p[i]).normalized*r;output.Add(a);for(int j=1;j<=2;j++){float t=j/2f;output.Add((1-t)*(1-t)*a+2*(1-t)*t*p[i]+t*t*b);}}output.Add(p[p.Length-1]);return output;
        }
        static Vector3 Along(List<Vector3> p,float s,out Vector3 direction)
        {direction=Vector3.up;for(int i=1;i<p.Count;i++){float d=Vector3.Distance(p[i-1],p[i]);direction=(p[i]-p[i-1]).normalized;if(s<=d)return p[i-1]+direction*s;s-=d;}return p[p.Count-1];}
        sealed class Geometry
        {
            readonly List<Vector3> vertices=new(),normal=new(),route=new();readonly List<Vector2> uv=new();readonly List<Color> colours=new();public readonly List<int> tris=new();
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color colour,int circuit,float start,float end,Vector2? atlas=null)
            {int n=vertices.Count;vertices.AddRange(new[]{a,b,c,d});var norm=Vector3.Cross(b-a,c-a).normalized;for(int i=0;i<4;i++){normal.Add(norm);colours.Add(colour);route.Add(new Vector3(circuit,i<2?start:end,atlas.HasValue?0:1));}if(atlas.HasValue){float y=atlas.Value.y;uv.AddRange(new[]{new Vector2(.98f,y),new Vector2(.02f,y),new Vector2(.02f,y+.055f),new Vector2(.98f,y+.055f)});}else uv.AddRange(new[]{new Vector2(.02f,.02f),new Vector2(.2f,.02f),new Vector2(.2f,.06f),new Vector2(.02f,.06f)});tris.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
            public void Tube(Vector3 a,Vector3 b,float radius,Color colour,int circuit,float start,float end,int sides)
            {var axis=(b-a).normalized;var u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized*radius;var v=Vector3.Cross(axis,u);for(int i=0;i<sides;i++){float x=i*Mathf.PI*2/sides,y=(i+1)*Mathf.PI*2/sides;var p=u*Mathf.Cos(x)+v*Mathf.Sin(x);var q=u*Mathf.Cos(y)+v*Mathf.Sin(y);Quad(a+p,a+q,b+q,b+p,colour,circuit,start,end);if(colour==Cloth){int n=uv.Count;uv[n-4]=new Vector2(.27f,.01f);uv[n-3]=new Vector2(.49f,.01f);uv[n-2]=new Vector2(.49f,.095f);uv[n-1]=new Vector2(.27f,.095f);}}}
            public void Disk(Vector3 p,Vector3 axis,float radius,Color c,int circuit)
            {var u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized*radius;var v=Vector3.Cross(axis,u);int n=vertices.Count;for(int i=0;i<8;i++){float a=i*Mathf.PI/4;vertices.Add(p+u*Mathf.Cos(a)+v*Mathf.Sin(a));normal.Add(axis);route.Add(new Vector3(circuit,.75f,0));colours.Add(c);uv.Add(new Vector2(.1f,.02f));}for(int i=1;i<7;i++)tris.AddRange(new[]{n,n+i,n+i+1});}
            public void Braid(Vector3 a,Vector3 b,float radius,Color colour,int circuit,float age,int strand)
            {var axis=(b-a).normalized;var u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized;var v=Vector3.Cross(axis,u);Vector3 Point(float t){float angle=t*Vector3.Distance(a,b)*70+strand*Mathf.PI;return Vector3.Lerp(a,b,t)+(u*Mathf.Cos(angle)+v*Mathf.Sin(angle))*radius;}int n=Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(a,b)*10),2,4);for(int i=0;i<n;i++)Tube(Point(i/(float)n),Point((i+1)/(float)n),radius*.6f,colour,circuit,age,age,4);}
            public void Saddle(Vector3 p,Vector3 size,Quaternion q,Color c,int circuit,float age)
            {Vector3 V(float x,float y,float z)=>p+q*Vector3.Scale(new Vector3(x,y,z),size*.5f);Quad(V(-1,-1,1),V(1,-1,1),V(1,1,1),V(-1,1,1),c,circuit,age,age);Quad(V(1,-1,1),V(1,-1,-1),V(1,1,-1),V(1,1,1),c,circuit,age,age);Quad(V(-1,-1,-1),V(-1,-1,1),V(-1,1,1),V(-1,1,-1),c,circuit,age,age);}
            public void Box(Vector3 p,Vector3 size,Quaternion q,Color c,int circuit,float age)
            {Vector3 V(float x,float y,float z)=>p+q*Vector3.Scale(new Vector3(x,y,z),size*.5f);Quad(V(-1,-1,1),V(1,-1,1),V(1,1,1),V(-1,1,1),c,circuit,age,age);Quad(V(1,-1,-1),V(-1,-1,-1),V(-1,1,-1),V(1,1,-1),c,circuit,age,age);Quad(V(1,-1,1),V(1,-1,-1),V(1,1,-1),V(1,1,1),c,circuit,age,age);Quad(V(-1,-1,-1),V(-1,-1,1),V(-1,1,1),V(-1,1,-1),c,circuit,age,age);Quad(V(-1,1,1),V(1,1,1),V(1,1,-1),V(-1,1,-1),c,circuit,age,age);Quad(V(-1,-1,-1),V(1,-1,-1),V(1,-1,1),V(-1,-1,1),c,circuit,age,age);}
            public void Label(Vector3 p,Quaternion q,Vector2 size,int row,int circuit){Vector3 V(float x,float y)=>p+q*new Vector3(x*size.x*.5f,y*size.y*.5f,0);Quad(V(-1,-1),V(1,-1),V(1,1),V(-1,1),Color.white,circuit,0,0,new Vector2(0,.12f+row*.08f));}
            public Mesh Mesh(string name){var m=new Mesh{name=name};m.SetVertices(vertices);m.SetNormals(normal);m.SetUVs(0,uv);m.SetUVs(1,route);m.SetColors(colours);m.SetTriangles(tris,0);m.RecalculateBounds();return m;}
        }
    }
}
