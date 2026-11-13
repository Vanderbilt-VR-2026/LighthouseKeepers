#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using LighthouseKeepers.Presentation.Environment;
namespace LighthouseKeepers.WatchDemo.Editor
{
    public static class SteamerAuthoring
    {
        const string Folder="Assets/WatchDemo/Resources/Sprint5Ships";
        [MenuItem("Lighthouse Keepers/Ships/Bake coastal steamer")]
        public static void Bake()
        {
            EditorUtility.audioMasterMute=true;Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            var hull=Save(SteamerGeometry.BuildHull(),"Hull");var far=Save(SteamerGeometry.BuildDistant(),"Distant");var beam=Save(SteamerGeometry.BuildBeam(),"Beam");var particles=Save(SteamerGeometry.BuildParticles(),"Particles");
            var bodyMat=Material("HullMaterial","Lighthouse/Steamer hull and lamps");var fxMat=Material("EffectsMaterial","Lighthouse/Steamer GPU effects");
            var go=new GameObject("CoastalSteamer");try{var view=go.AddComponent<SteamerView>();view.hull=hull;view.distant=far;view.beam=beam;view.particles=particles;view.hullMaterial=bodyMat;view.effectsMaterial=fxMat;PrefabUtility.SaveAsPrefabAsset(go,Folder+"/CoastalSteamer.prefab");}finally{Object.DestroyImmediate(go);}
            AssetDatabase.SaveAssets();Debug.Log($"STEAMER_BAKED hull={hull.triangles.Length/3} distant={far.triangles.Length/3} beam={beam.triangles.Length/3} particles={particles.triangles.Length/3}; two materials, zero realtime lights; runtime flag OFF until outcomes pass.");
        }
        static Mesh Save(Mesh generated,string name){string path=Folder+"/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(generated,existing);Object.DestroyImmediate(generated);EditorUtility.SetDirty(existing);return existing;}AssetDatabase.CreateAsset(generated,path);return generated;}
        static Material Material(string name,string shader){string path=Folder+"/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find(shader)){name=name,enableInstancing=true};AssetDatabase.CreateAsset(mat,path);}return mat;}
    }
}
#endif
