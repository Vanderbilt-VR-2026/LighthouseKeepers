using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

// Editor-only contract visualization; no gameplay or production-loading changes.
public static class StationBoxExporter
{
    [Serializable] public class Contract { public string version; public Floor[] floors; public House house; public Station[] stations; public Work[] operatorPositions; public Route[] routes; }
    [Serializable] public class Floor { public string id; public float floorY, interiorRadius, clearHeight; }
    [Serializable] public class House { public float[] centre, interiorSize; }
    [Serializable] public class Pose { public float[] position, rotationEuler; }
    [Serializable] public class Footprint { public float[] size; }
    [Serializable] public class Anchor { public string name; public float[] localPosition; }
    [Serializable] public class Legacy { public string socketName, anchorName; public float[] localPosition; }
    [Serializable] public class Station { public string id, floor; public Pose anchor; public Footprint footprint; public Anchor[] interactiveAnchors; public Legacy[] legacySockets; }
    [Serializable] public class Work { public string id, floor; public float[] position; public float facingAzimuth, radius; }
    [Serializable] public class Route { public string id, floor; public float widthM; public Vector3[] points; }
    [Serializable] public class RouteExport { public Route[] routes; }
    static Vector3 V(float[] a) => new Vector3(a[0], a[1], a[2]);
    static Material box, floor, cyan, orange, label;
    static GameObject Root(string name, Transform parent = null) { var g = new GameObject(name); if (parent != null) g.transform.SetParent(parent, false); return g; }
    static Material Mat(string name, Color color, bool transparent = false)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Unlit")) {name=name};
        m.SetColor("_BaseColor", color); m.SetFloat("_Cull", 0);
        if (transparent) { m.SetFloat("_Surface",1); m.SetFloat("_SrcBlend",(int)BlendMode.SrcAlpha); m.SetFloat("_DstBlend",(int)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite",0); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.SetOverrideTag("RenderType","Transparent"); m.renderQueue=3000; }
        return m;
    }
    static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        var g=GameObject.CreatePrimitive(type); g.name=name; g.transform.SetParent(parent,false); g.transform.localPosition=position; g.transform.localScale=scale;
        UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
        var r=g.GetComponent<Renderer>(); r.sharedMaterial=material; r.shadowCastingMode=ShadowCastingMode.Off; r.receiveShadows=false;
        return g;
    }
    static void Text(string text, Transform parent, Vector3 pos, float size=.12f)
    {
        var g=Root("Label "+text,parent);g.transform.localPosition=pos;g.transform.localRotation=Quaternion.Euler(90,0,0);
        var t=g.AddComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=40;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=Color.white;
        t.GetComponent<Renderer>().sharedMaterial=t.font.material;
    }
    static void Line(string name, Transform parent, Vector3[] pts, Material material, bool loop=false, float width=.025f)
    {
        var l=Root(name,parent).AddComponent<LineRenderer>();l.useWorldSpace=false;l.positionCount=pts.Length;l.SetPositions(pts);l.loop=loop;l.widthMultiplier=width;l.sharedMaterial=material;l.shadowCastingMode=ShadowCastingMode.Off;l.receiveShadows=false;
    }
    public static string ConvertRouteArrays(string json)
    {
        return System.Text.RegularExpressions.Regex.Replace(json, @"""polyline""\s*:\s*\[((?:\s*\[[^\[\]]*\]\s*,?)+)\]", match => {
            var points=System.Text.RegularExpressions.Regex.Matches(match.Groups[1].Value,@"\[([^\]]+)\]").Cast<System.Text.RegularExpressions.Match>().Select(m=>{
                var n=m.Groups[1].Value.Split(',');return "{\"x\":"+n[0]+",\"y\":"+n[1]+",\"z\":"+n[2]+"}";
            });
            return "\"points\":["+string.Join(",",points)+"]";
        });
    }
    public static void Build()
    {
        var source=File.ReadAllText("Assets/Data/Layout/lighthouse-layout.json");File.WriteAllText("Docs/StationContract.json",source);var d=JsonUtility.FromJson<Contract>(source);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        box=Mat("Draft box translucent",new Color(.15f,.65f,.9f,.12f),true);floor=Mat("Draft floor",new Color(.12f,.15f,.18f));
        cyan=Mat("Draft route",new Color(.1f,.8f,1));orange=Mat("Draft work disc",new Color(1,.5f,.1f,.25f),true);
        var root=Root("LK_StationBoxes — 1.0 — NOT A PLAYABLE SCENE");
        var parents=new System.Collections.Generic.Dictionary<string,Transform>();
        parents["House"]=Root("House",root.transform).transform;
        Primitive("House floor marker",PrimitiveType.Cube,parents["House"],V(d.house.centre)+Vector3.down*.025f,new Vector3(7,.05f,5),floor);
        foreach(var f in d.floors)
        {
            var p=Root(f.id,root.transform).transform;parents[f.id]=p;
            Primitive(f.id+" floor marker",PrimitiveType.Cylinder,p,new Vector3(0,f.floorY-.025f,0),new Vector3(f.interiorRadius*2,.025f,f.interiorRadius*2),floor);
            foreach(float h in new[]{0f,f.clearHeight})
            {
                var pts=Enumerable.Range(0,64).Select(i=>new Vector3(Mathf.Sin(i*Mathf.PI/32)*f.interiorRadius,f.floorY+h,Mathf.Cos(i*Mathf.PI/32)*f.interiorRadius)).ToArray();
                Line(f.id+" cylindrical room boundary "+h,p,pts,cyan,true,.012f);
            }
            Text(f.id+" — Y "+f.floorY.ToString("F2",System.Globalization.CultureInfo.InvariantCulture),p,new Vector3(0,f.floorY+.05f,f.interiorRadius-.3f),.18f);
        }
        foreach(var s in d.stations.OrderBy(x=>x.id,StringComparer.Ordinal))
        {
            var p=Root(s.id,parents[s.floor]).transform;p.position=V(s.anchor.position);p.rotation=Quaternion.Euler(V(s.anchor.rotationEuler));
            Vector3 size=V(s.footprint.size);
            Primitive("Envelope — dimensions frozen through v1.0",PrimitiveType.Cube,p,new Vector3(0,size.y/2,size.z/2),size,box);
            Vector3[] corners={new Vector3(-size.x/2,0,0),new Vector3(size.x/2,0,0),new Vector3(size.x/2,0,size.z),new Vector3(-size.x/2,0,size.z)};
            Line("Envelope base",p,corners,cyan,true,.012f);Line("Envelope top",p,corners.Select(v=>v+Vector3.up*size.y).ToArray(),cyan,true,.012f);
            for(int i=0;i<4;i++)Line("Envelope edge "+i,p,new[]{corners[i],corners[i]+Vector3.up*size.y},cyan,false,.012f);
            Text(s.id,p,new Vector3(0,size.y+.025f,size.z/2),.075f);
            foreach(var a in s.interactiveAnchors)Root(a.name,p).transform.localPosition=V(a.localPosition);
            foreach(var a in s.legacySockets) { var alias=Root(a.socketName,p); alias.transform.localPosition=V(a.localPosition);Root(a.anchorName,alias.transform); }
        }
        foreach(var o in d.operatorPositions)
        {
            var g=Root(o.id,parents[o.floor]);g.transform.position=V(o.position);g.transform.rotation=Quaternion.Euler(0,o.facingAzimuth,0);
            Primitive("Standing clearance — radius 0.70 m",PrimitiveType.Cylinder,g.transform,Vector3.up*.01f,new Vector3(o.radius*2,.005f,o.radius*2),orange);
            Text(o.id,g.transform,new Vector3(0,.025f,0),.10f);
            Line("Facing",g.transform,new[]{new Vector3(0,.04f,0),new Vector3(0,.04f,.5f),new Vector3(-.1f,.04f,.4f),new Vector3(0,.04f,.5f),new Vector3(.1f,.04f,.4f)},cyan);
        }
        var routeData=JsonUtility.FromJson<RouteExport>(ConvertRouteArrays(source));
        foreach(var route in routeData.routes)
        {
            Line(route.id,parents[route.floor],route.points.Select(v=>v+Vector3.up*.04f).ToArray(),cyan,false,.035f);
        }
        Primitive("Scale reference — exactly 1 m",PrimitiveType.Cube,root.transform,new Vector3(-6,.5f,0),Vector3.one,floor);
        Primitive("Height marker — 1.70 m",PrimitiveType.Cube,root.transform,new Vector3(-7,.85f,0),new Vector3(.15f,1.7f,.15f),cyan);
        Text("1 m cube / 1.70 m height",root.transform,new Vector3(-6.5f,.02f,-1),.12f);
        const string path="Assets/Scenes/Lab/LK_StationBoxes.unity";
        if(!EditorSceneManager.SaveScene(scene,path))throw new Exception("Scene save failed");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
        var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        if(all.Count(t=>t.name.StartsWith("STN_",StringComparison.Ordinal))!=15)throw new Exception("Station count");
        if(all.Count(t=>t.name=="Interaction Anchor")!=16)throw new Exception("Legacy count");
        foreach(var s in d.stations)
        {
            var t=all.Single(x=>x.name==s.id);
            if(Vector3.Distance(t.position,V(s.anchor.position))>.00001f||Vector3.Distance(t.localScale,Vector3.one)>.00001f)throw new Exception("Transform mismatch "+s.id);
            foreach(var a in s.interactiveAnchors)if(t.Find(a.name)==null)throw new Exception("Missing anchor "+a.name);
            foreach(var a in s.legacySockets)if(t.Find(a.socketName+"/"+a.anchorName)==null)throw new Exception("Missing legacy "+a.socketName);
        }
        if(all.Any(t=>t.GetComponent<Camera>()||t.GetComponent<AudioListener>()||t.GetComponent<Collider>()))throw new Exception("Lab has forbidden active component");
        foreach(var r in all.SelectMany(t=>t.GetComponents<Renderer>()))foreach(var m in r.sharedMaterials)if(!m||!m.shader||m.shader.name=="Hidden/InternalErrorShader")throw new Exception("Missing material: "+r.name);
        if(new FileInfo(path).Length>=2*1024*1024)throw new Exception("Lab exceeds 2 MiB");
        File.WriteAllText("artifacts/contract/scene-validation.txt","PASS saved/reopened scene; 15 STN roots; 16 legacy aliases; every named anchor; matching world positions; unit root scales; no cameras/listeners/colliders; all renderer materials resolve; scene below 2 MiB.\n");
        Debug.Log("STATION_CONTRACT_DRAFT_PASS");
    }
}
