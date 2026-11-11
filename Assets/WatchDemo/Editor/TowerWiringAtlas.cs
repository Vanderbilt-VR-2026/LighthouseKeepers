using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using LighthouseKeepers.Core;
using LighthouseKeepers.WatchDemo;
namespace LighthouseKeepers.Editor
{
    public static partial class TowerWiringAuthoring
    {
        static void BuildAtlas()
        {
            const int w=512,h=1024,size=38;
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");var all="";for(int row=0;row<5;row++)all+=KeeperStrings.Get("wiring.box"+row);font.RequestCharactersInTexture(all,size,FontStyle.Bold);
            var v=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            for(int row=0;row<5;row++)
            {
                string text=KeeperStrings.Get("wiring.box"+row);float width=0;foreach(char c in text)if(font.GetCharacterInfo(c,out var g,size,FontStyle.Bold))width+=g.advance;
                float scale=Mathf.Min(1,480/width),x=(w-width*scale)/2,y=(.12f+row*.08f)*h+14;
                foreach(char c in text){if(!font.GetCharacterInfo(c,out var g,size,FontStyle.Bold))continue;int n=v.Count;v.Add(new Vector3(x+g.minX*scale,y+g.minY*scale));v.Add(new Vector3(x+g.maxX*scale,y+g.minY*scale));v.Add(new Vector3(x+g.maxX*scale,y+g.maxY*scale));v.Add(new Vector3(x+g.minX*scale,y+g.maxY*scale));uv.AddRange(new[]{g.uvBottomLeft,g.uvBottomRight,g.uvTopRight,g.uvTopLeft});tris.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});x+=g.advance*scale;}
            }
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);
            var mat=new Material(Shader.Find("LighthouseKeepers/WorldWriting"));mat.mainTexture=font.material.mainTexture;mat.SetColor("_Color",new Color(.07f,.08f,.065f));
            var rt=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32);var cmd=new CommandBuffer();cmd.SetRenderTarget(rt);cmd.ClearRenderTarget(false,true,new Color(.71f,.70f,.64f));cmd.SetViewProjectionMatrices(Matrix4x4.identity,GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(0,w,0,h,-1,1),true));cmd.DrawMesh(mesh,Matrix4x4.identity,mat);Graphics.ExecuteCommandBuffer(cmd);cmd.Release();
            var prior=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(w,h,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,w,h),0,0);texture.Apply();if(SystemInfo.graphicsUVStartsAtTop){var pixels=texture.GetPixels();var flipped=new Color[pixels.Length];for(int y=0;y<h;y++)System.Array.Copy(pixels,y*w,flipped,(h-1-y)*w,w);texture.SetPixels(flipped);texture.Apply();}File.WriteAllBytes(Folder+"/Atlas.png",texture.EncodeToPNG());RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(texture);Object.DestroyImmediate(mat);Object.DestroyImmediate(mesh);
            AssetDatabase.ImportAsset(Folder+"/Atlas.png",ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+"/Atlas.png");importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;importer.wrapMode=TextureWrapMode.Clamp;var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.format=TextureImporterFormat.ASTC_6x6;android.maxTextureSize=1024;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            var spark=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Sparks.mat");if(!spark){spark=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));spark.SetColor("_BaseColor",new Color(1,.45f,.08f));AssetDatabase.CreateAsset(spark,Folder+"/Sparks.mat");}
        }
    }
}
