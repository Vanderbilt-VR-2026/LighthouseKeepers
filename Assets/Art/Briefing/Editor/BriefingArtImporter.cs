using UnityEditor;
namespace LighthouseKeepers.Editor
{
    public sealed class BriefingArtImporter:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/Art/Briefing/Resources/"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;
            t.sRGBTexture=true;t.mipmapEnabled=true;t.alphaSource=TextureImporterAlphaSource.None;
            t.wrapMode=UnityEngine.TextureWrapMode.Clamp;t.filterMode=UnityEngine.FilterMode.Trilinear;
            t.maxTextureSize=1024;t.isReadable=false;
            t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=1024,format=TextureImporterFormat.ASTC_6x6});
        }
    }
}
