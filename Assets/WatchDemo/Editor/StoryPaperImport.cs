using UnityEditor;
namespace LighthouseKeepers.WatchDemo.Editor
{
    public sealed class StoryPaperImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(assetPath!="Assets/Art/Story/Resources/KettleStory/papers.png")return;
            var texture=(TextureImporter)assetImporter;texture.textureType=TextureImporterType.Default;texture.mipmapEnabled=true;texture.sRGBTexture=true;texture.maxTextureSize=2048;texture.anisoLevel=4;texture.filterMode=UnityEngine.FilterMode.Trilinear;
            texture.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_4x4});
        }
    }
}
