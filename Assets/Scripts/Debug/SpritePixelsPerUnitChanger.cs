using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class SpritePixelsPerUnitChanger : AssetPostprocessor
{
    const int PostProcessOrder = 0;

    public override int GetPostprocessOrder () {
        return PostProcessOrder;
    }
	void OnPreprocessTexture ()
	{
		TextureImporter textureImporter  = (TextureImporter) assetImporter;
		textureImporter.spritePixelsPerUnit = 10;
        textureImporter.crunchedCompression = false;
        textureImporter.compressionQuality = 100;
        textureImporter.filterMode = FilterMode.Point;

        // TextureImporterPlatformSettings pcSettings = new TextureImporterPlatformSettings(crunchedCompression=false, compressionQuality=100);
        // // TextureImporterPlatformSettings dsSettings = new TextureImporterPlatformSettings(); pcSettings.CopyTo(dsSettings);
        // // TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings();pcSettings.CopyTo(androidSettings);
        // // TextureImporterPlatformSettings html5Settings = new TextureImporterPlatformSettings();pcSettings.CopyTo(html5Settings);
        // textureImporter.SetPlatformTextureSettings("standalone", pcSettings);
        // textureImporter.SetPlatformTextureSettings("iPhone", pcSettings);
        // textureImporter.SetPlatformTextureSettings("Android", pcSettings);
        // textureImporter.SetPlatformTextureSettings("WebGL", pcSettings);
        // textureImporter.SetPlatformTextureSettings("Windows Store Apps", pcSettings);
        // textureImporter.SetPlatformTextureSettings("PS4", pcSettings);
        // textureImporter.SetPlatformTextureSettings("XboxOne", pcSettings);
        // textureImporter.SetPlatformTextureSettings("Nintendo Switch", pcSettings);
        // textureImporter.SetPlatformTextureSettings("tvOS", pcSettings);

	}
}
#endif