using System;using System.Collections.Generic;using System.Linq;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class RenderAudit {
 public static void Batch(){string folder=System.Environment.GetEnvironmentVariable("LK_AUDIT_OUT")??"artifacts/s4-master/part-c/coplanar";bool fix=System.Environment.GetEnvironmentVariable("LK_AUDIT_FIX")=="1";if(!fix){Run(folder,false);return;}for(int pass=0;pass<4;pass++){Run(folder+"/pass"+pass,true);string result=Run(folder+"/after"+pass,false);if(result.Contains("issues=0 "))return;}throw new Exception("Coplanar overlaps remain; inspect the audit before proceeding");}

 struct Tri {public MeshRenderer r;public Vector3 a,b,c,n;public float d;public int axis;}
 public static string Run(string folder,bool fix){Directory.CreateDirectory(folder);var report=new List<string>();var lights=new List<string>();int count=0,issues=0,repairs=0;
 var previous=EditorSceneManager.GetSceneManagerSetup();try{foreach(var path in AssetDatabase.FindAssets("t:Scene",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(x=>x)){
  var scene=EditorSceneManager.OpenScene(path);count++;var groups=new Dictionary<string,List<Tri>>();var normals=new Dictionary<MeshRenderer,Vector3>();var pairs=new HashSet<string>();var neighbours=new Dictionary<MeshRenderer,HashSet<MeshRenderer>>();var constraints=new List<(MeshRenderer a,MeshRenderer b,Vector3 n)>();
  var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
  foreach(var r in renderers){var name=PathOf(r.transform);if(name.Contains("Player")||name.Contains("Hand")||name.Contains("Avatar")||name.Contains("Sky"))continue;
   var lod=r.GetComponentInParent<LODGroup>();if(lod&&lod.GetLODs().Length>0&&!lod.GetLODs()[0].renderers.Contains(r))continue;
   var filter=r.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)continue;var mesh=filter.sharedMesh;Vector3[] v;int[] indices;try{v=mesh.vertices;indices=mesh.triangles;}catch{continue;}
   var matrix=r.localToWorldMatrix;
   for(int i=0;i<indices.Length;i+=3){Vector3 a=matrix.MultiplyPoint3x4(v[indices[i]]),b=matrix.MultiplyPoint3x4(v[indices[i+1]]),c=matrix.MultiplyPoint3x4(v[indices[i+2]]);var cross=Vector3.Cross(b-a,c-a);if(cross.magnitude<.002f)continue;var n=cross.normalized;float d=Vector3.Dot(n,a);int axis=Mathf.Abs(n.x)>Mathf.Abs(n.y)?(Mathf.Abs(n.x)>Mathf.Abs(n.z)?0:2):(Mathf.Abs(n.y)>Mathf.Abs(n.z)?1:2);
    // Same-facing surfaces only; opposed faces may be an intentionally double-sided wall.
    string key=$"{Mathf.RoundToInt(n.x*1000)}/{Mathf.RoundToInt(n.y*1000)}/{Mathf.RoundToInt(n.z*1000)}/{Mathf.RoundToInt(d*1000)}";
    if(!groups.TryGetValue(key,out var list)){list=new();groups.Add(key,list);}list.Add(new Tri{r=r,a=a,b=b,c=c,n=n,d=d,axis=axis});
   }
  }
  foreach(var group in groups.Values){float cellSize=Mathf.Max(1,group.Max(t=>Mathf.Max(t.a.magnitude,t.b.magnitude,t.c.magnitude))/32f);var cells=new Dictionary<Vector2Int,List<int>>();var checkedPairs=new HashSet<long>();
   for(int i=0;i<group.Count;i++){var t=group[i];var a=P(t.a,t.axis);var b=P(t.b,t.axis);var c=P(t.c,t.axis);var min=Vector2.Min(a,Vector2.Min(b,c));var max=Vector2.Max(a,Vector2.Max(b,c));
    for(int x=Mathf.FloorToInt(min.x/cellSize);x<=Mathf.FloorToInt(max.x/cellSize);x++)for(int y=Mathf.FloorToInt(min.y/cellSize);y<=Mathf.FloorToInt(max.y/cellSize);y++){
     var cell=new Vector2Int(x,y);if(!cells.TryGetValue(cell,out var list)){list=new();cells.Add(cell,list);}foreach(int j in list){long pair=((long)i<<32)+(uint)j;if(!checkedPairs.Add(pair))continue;var u=group[j];if(t.r==u.r)continue;
      if(Mathf.Abs(Vector3.Dot(t.n,u.a)-t.d)>.0015f||Vector3.Dot(t.n,u.n)<.999f)continue;
      float area=Overlap(new[]{a,b,c},new[]{P(u.a,t.axis),P(u.b,t.axis),P(u.c,t.axis)});if(area<.002f)continue;
      string id=Math.Min(t.r.GetInstanceID(),u.r.GetInstanceID())+"/"+Math.Max(t.r.GetInstanceID(),u.r.GetInstanceID());if(!pairs.Add(id))continue;issues++;report.Add(path+" | "+PathOf(t.r.transform)+" <> "+PathOf(u.r.transform)+" | overlap="+area.ToString("F4"));
      // Choose the smaller cover; retain all collision and source meshes, offset a visual child.
      var target=t.r.bounds.size.sqrMagnitude<=u.r.bounds.size.sqrMagnitude?t.r:u.r;foreach(var r in new[]{t.r,u.r})if(!neighbours.ContainsKey(r))neighbours[r]=new();neighbours[t.r].Add(u.r);neighbours[u.r].Add(t.r);constraints.Add((t.r,u.r,t.n));
     }list.Add(i);
    }
   }
  }
  var colours=new Dictionary<MeshRenderer,int>();foreach(var r in neighbours.Keys.OrderByDescending(r=>neighbours[r].Count).ThenBy(r=>PathOf(r.transform))){var used=new HashSet<int>(neighbours[r].Where(colours.ContainsKey).Select(n=>colours[n]));int colour=0;while(used.Contains(colour))colour++;colours[r]=colour;if(colour>0)normals[r]=new Vector3(.0123f,.0107f,.0151f)*colour;}
  for(int pass=0;pass<12;pass++){bool changed=false;foreach(var c in constraints){normals.TryGetValue(c.a,out var a);normals.TryGetValue(c.b,out var b);if(Mathf.Abs(Vector3.Dot(a-b,c.n))>=.003f)continue;var target=c.a.bounds.size.sqrMagnitude<=c.b.bounds.size.sqrMagnitude?c.a:c.b;normals.TryGetValue(target,out var delta);normals[target]=delta+c.n*.012f;changed=true;}if(!changed)break;}
  foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None)){lights.Add(path+" | "+PathOf(light.transform)+" | "+light.type+" | "+light.lightmapBakeType+" | enabled="+light.enabled+" | shadows="+light.shadows);if(fix&&light.lightmapBakeType==LightmapBakeType.Realtime){light.lightmapBakeType=LightmapBakeType.Baked;EditorUtility.SetDirty(light);}}
  if(fix){foreach(var pair in normals){var r=pair.Key;if(!r)continue;var filter=r.GetComponent<MeshFilter>();var delta=r.transform.InverseTransformVector(pair.Value);var source=filter.sharedMesh;string identity=AssetDatabase.GetAssetPath(source)+"/"+source.name+"/"+delta.ToString("F5");string hash=UnityEngine.Hash128.Compute(identity).ToString();string meshPath="Assets/Art/RenderCorrections/Offset-"+hash+".asset";var shifted=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(!shifted){Directory.CreateDirectory("Assets/Art/RenderCorrections");AssetDatabase.Refresh();shifted=UnityEngine.Object.Instantiate(source);shifted.name=source.name+" / visual separation";shifted.vertices=source.vertices.Select(v=>v+delta).ToArray();shifted.RecalculateBounds();AssetDatabase.CreateAsset(shifted,meshPath);}filter.sharedMesh=shifted;var lod=r.GetComponentInParent<LODGroup>();if(lod)lod.RecalculateBounds();repairs++;}EditorSceneManager.SaveScene(scene);}
 }
 }finally{if(previous.Length>0&&previous.All(x=>!string.IsNullOrEmpty(x.path)))EditorSceneManager.RestoreSceneManagerSetup(previous);}File.WriteAllLines(Path.Combine(folder,"coplanar.txt"),report.Prepend($"scenes={count}, coplanarPairs={issues}, visualOffsets={repairs}"));File.WriteAllLines(Path.Combine(folder,"lights.txt"),lights);return $"scenes={count} issues={issues} repaired={repairs}";
 }
 static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
 static Vector2 P(Vector3 v,int axis)=>axis==0?new Vector2(v.y,v.z):axis==1?new Vector2(v.x,v.z):new Vector2(v.x,v.y);
 static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
 static float Overlap(Vector2[] subject,Vector2[] clip){var points=subject.ToList();float sign=Mathf.Sign(Cross(clip[1]-clip[0],clip[2]-clip[0]));for(int i=0;i<3&&points.Count>0;i++){var a=clip[i];var b=clip[(i+1)%3];var input=points;points=new();var p=input[input.Count-1];float dp=Cross(b-a,p-a)*sign;foreach(var q in input){float dq=Cross(b-a,q-a)*sign;if((dq>=0)!=(dp>=0))points.Add(p+(q-p)*(dp/(dp-dq)));if(dq>=0)points.Add(q);p=q;dp=dq;}}float area=0;for(int i=0;i<points.Count;i++)area+=Cross(points[i],points[(i+1)%points.Count]);return Mathf.Abs(area)*.5f;}
}
