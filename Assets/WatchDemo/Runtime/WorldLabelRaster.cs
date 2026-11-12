using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;using LighthouseKeepers.Core;
namespace LighthouseKeepers.WatchDemo {
 // World writing is a mipmapped physical surface. Keep the original TextMesh as the data sink
 // for existing station scripts; rebuild only changed captions, at most two labels per frame.
 [DefaultExecutionOrder(1200)]public sealed class WorldLabelRaster:MonoBehaviour {
  sealed class Label {public TextMesh source;public Renderer old;public MeshRenderer face;public Mesh quad;public RenderTexture texture;public string shown;public Color colour;public bool visible;}
  readonly List<Label> labels=new();Material surface,glyph;Mesh glyphMesh;int cursor;float nextScan;readonly HashSet<TextMesh> known=new();
  public static void Create(WatchDemoController w){if(VisualPassSettings.Current&&VisualPassSettings.Current.renderQuality)w.gameObject.AddComponent<WorldLabelRaster>();}
  void Start(){surface=new Material(Resources.Load<Shader>("WorldWriting"));glyph=new Material(Resources.Load<Shader>("WorldWriting"));glyph.SetFloat("_Raster",1);glyphMesh=new Mesh();Scan();}
  void Scan(){nextScan=Time.time+3;foreach(var source in FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
   if(known.Contains(source)||source.GetComponentInParent<Canvas>()||source.name.Contains("name",System.StringComparison.OrdinalIgnoreCase)||source.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>())continue;
   var old=source.GetComponent<Renderer>();if(!old)continue;known.Add(source);var go=new GameObject("Mipmapped writing surface");go.transform.SetParent(source.transform,false);var mesh=new Mesh();go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=new Material(surface);r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;labels.Add(new Label{source=source,old=old,face=r,quad=mesh,visible=old.enabled});old.enabled=false;
  }}
  void LateUpdate(){if(!surface)return;if(Time.time>=nextScan)Scan();int count=labels.Count,rebuilt=0;for(int i=0;i<count;i++){cursor=(cursor+1)%count;var l=labels[cursor];if(!l.source||!l.face)continue;if(l.old.enabled){l.visible=true;l.old.enabled=false;}l.face.enabled=l.visible&&l.source.gameObject.activeInHierarchy&&!string.IsNullOrEmpty(l.source.text);if(!l.face.enabled)continue;
   if((l.shown!=l.source.text||l.colour!=l.source.color)&&rebuilt<2){Raster(l);rebuilt++;}
  }}
  void Raster(Label l){var source=l.source;var font=source.font?source.font:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");int size=Mathf.Max(64,source.fontSize);font.RequestCharactersInTexture(source.text,size,source.fontStyle);
   var verts=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();float y=0;var lines=source.text.Split('\n');float height=size*source.lineSpacing;float maxWidth=1;
   foreach(var line in lines){float width=0;foreach(char c in line)if(font.GetCharacterInfo(c,out var info,size,source.fontStyle))width+=info.advance;maxWidth=Mathf.Max(maxWidth,width);}
   foreach(var line in lines){float width=0;foreach(char c in line)if(font.GetCharacterInfo(c,out var inf,size,source.fontStyle))width+=inf.advance;
    float x=source.alignment==TextAlignment.Center?(maxWidth-width)*.5f:source.alignment==TextAlignment.Right?maxWidth-width:0;
    foreach(char c in line){if(!font.GetCharacterInfo(c,out var g,size,source.fontStyle))continue;int n=verts.Count;verts.Add(new Vector3(x+g.minX,y+g.minY));verts.Add(new Vector3(x+g.maxX,y+g.minY));verts.Add(new Vector3(x+g.maxX,y+g.maxY));verts.Add(new Vector3(x+g.minX,y+g.maxY));uv.Add(g.uvBottomLeft);uv.Add(g.uvBottomRight);uv.Add(g.uvTopRight);uv.Add(g.uvTopLeft);triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});x+=g.advance;}y-=height;
   }
   if(verts.Count==0)return;glyphMesh.Clear();glyphMesh.SetVertices(verts);glyphMesh.SetUVs(0,uv);glyphMesh.SetTriangles(triangles,0);glyphMesh.RecalculateBounds();var b=glyphMesh.bounds;float padding=4;float widthPixels=b.size.x+padding*2,heightPixels=b.size.y+padding*2;
   int textureWidth=Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.CeilToInt(widthPixels)),128,1024),h=Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.CeilToInt(heightPixels)),64,1024);
   if(l.texture&&(l.texture.width!=textureWidth||l.texture.height!=h)){l.texture.Release();Destroy(l.texture);l.texture=null;}
   if(!l.texture){l.texture=new RenderTexture(textureWidth,h,0,RenderTextureFormat.ARGB32){name="Mipmapped ink "+source.name,useMipMap=true,autoGenerateMips=false,filterMode=FilterMode.Trilinear,anisoLevel=4};l.texture.Create();l.face.sharedMaterial.mainTexture=l.texture;}
   var cmd=new CommandBuffer{name="Raster physical writing on caption change"};cmd.SetRenderTarget(l.texture);cmd.ClearRenderTarget(false,true,Color.clear);cmd.SetViewProjectionMatrices(Matrix4x4.identity,GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(b.min.x-padding,b.max.x+padding,b.min.y-padding,b.max.y+padding,-1,1),true));glyph.mainTexture=font.material.mainTexture;glyph.SetColor("_Color",Color.white);cmd.DrawMesh(glyphMesh,Matrix4x4.identity,glyph);cmd.GenerateMips(l.texture);Graphics.ExecuteCommandBuffer(cmd);cmd.Release();
   // TextMesh's renderer bounds preserve authored character scale, anchoring and placement.
   var bounds=l.old.localBounds;if(bounds.size.x<.00001f||bounds.size.y<.00001f)bounds=new Bounds(Vector3.zero,new Vector3(widthPixels,heightPixels,0)*source.characterSize*.1f);
   var mn=bounds.min;var mx=bounds.max;l.quad.vertices=new[]{new Vector3(mn.x,mn.y,-.001f),new Vector3(mx.x,mn.y,-.001f),new Vector3(mx.x,mx.y,-.001f),new Vector3(mn.x,mx.y,-.001f)};l.quad.uv=SystemInfo.graphicsUVStartsAtTop?new[]{Vector2.up,Vector2.one,Vector2.right,Vector2.zero}:new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};l.quad.triangles=new[]{0,2,1,0,3,2};l.quad.RecalculateBounds();l.face.sharedMaterial.SetColor("_Color",source.color);l.shown=source.text;l.colour=source.color;
  }
  void OnDestroy(){foreach(var l in labels){if(l.texture){l.texture.Release();Destroy(l.texture);}if(l.quad)Destroy(l.quad);if(l.face)Destroy(l.face.sharedMaterial);if(l.old)l.old.enabled=l.visible;}if(glyphMesh)Destroy(glyphMesh);if(surface)Destroy(surface);if(glyph)Destroy(glyph);}
 }
}
