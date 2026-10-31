using UnityEditor;
using UnityEngine;

// Mixamo: bake root en pose para que el loop no desplace ni haga rollback.
public class MixamoRootLock : AssetPostprocessor
{
    private void OnPreprocessAnimation()
    {
        if (assetPath.IndexOf("Resources/Mixamo") < 0)
            return;
        if (assetPath.IndexOf("Megan") >= 0)
            return;

        var importer = assetImporter as ModelImporter;
        if (importer == null)
            return;

        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;

        var clips = importer.defaultClipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
        {
            clips = new[] { new ModelImporterClipAnimation() };
            clips[0].name = "mixamo.com";
            clips[0].takeName = "mixamo.com";
            clips[0].firstFrame = 0;
            clips[0].lastFrame = 1000;
        }

        bool loop = assetPath.IndexOf("Standing") < 0;
        for (int i = 0; i < clips.Length; i++)
        {
            var c = clips[i];
            c.lockRootRotation = true;
            c.lockRootHeightY = true;
            c.lockRootPositionXZ = true;
            c.keepOriginalOrientation = true;
            c.keepOriginalPositionY = false;
            c.keepOriginalPositionXZ = false;
            c.heightFromFeet = true;
            c.loopTime = loop;
            clips[i] = c;
        }

        importer.clipAnimations = clips;
    }
}

[InitializeOnLoad]
public static class MixamoRootLockOnce
{
    static MixamoRootLockOnce()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        if (EditorPrefs.GetInt("EnigmaMixamoRootLock", 0) == 7)
            return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Resources/Mixamo" });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (path.IndexOf("Megan") >= 0)
                continue;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        EditorPrefs.SetInt("EnigmaMixamoRootLock", 7);
    }
}
