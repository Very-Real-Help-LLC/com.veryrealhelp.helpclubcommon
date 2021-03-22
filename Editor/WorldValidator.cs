using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using VeryRealHelp.HelpClubCommon.World;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class WorldValidator
    {
        public static readonly string[] VALID_MODEL_EXTENSIONS = { "fbx", "dae", "3ds", "dxf", "obj" };

        [MenuItem("VRH/Worlds/Validate All Worlds")]
        public static bool ValidateAll()
        {
            if (Application.isBatchMode)
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log("Validating all worlds");
            var guids = AssetDatabase.FindAssets("t:WorldInfo");
            bool[] results = Array.ConvertAll(guids, guid => ValidateWorld(AssetDatabase.GUIDToAssetPath(guid)));
            return Array.TrueForAll(results, value => value);
        }

        public static bool ValidateWorld(string assetPath) => ValidateWorld(AssetDatabase.LoadAssetAtPath<WorldInfo>(assetPath));

        public static bool ValidateWorld(WorldInfo worldInfo)
        {
            if (Application.isBatchMode)
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.LogFormat("Validating world {0} (scene: {1})", worldInfo.portalLabel, worldInfo.sceneAssetName);
            
            EditorSceneManager.OpenScene(worldInfo.sceneAssetName);
            var collections = new CheckCollection[]
            {
                settingsChecks,
                sceneRequirementChecks,
                worldInfoChecks,
            };
            uint count = 0;
            uint failures = 0;
            foreach (var collection in collections)
                foreach (var check in collection)
                {
                    bool passed = false;
                    ++count;
                    if (check.Test()) {
                        Debug.LogFormat("[PASS] {0}", check.label);
                        passed = true;
                    }
                    else if (check.fix != null)
                    {
                        check.Fix();
                        if (check.Test())
                        {
                            Debug.LogFormat("[PASS] (FIXED AUTOMATICALLY) {0} : {1}", check.label, check.invalidMessage);
                            passed = true;
                        }
                    }
                    if (!passed) {
                        Debug.LogFormat("[FAIL] {0}: {1}", check.label, check.invalidMessage);
                        ++failures;
                    }
                }
            System.Console.WriteLine();
            if (failures > 0)
                Debug.LogFormat("Failed {0}/{1} checks.", failures, count);
            else
                Debug.LogFormat("Passed all {0} checks.", count);
            System.Console.WriteLine();
            return failures == 0;
        }

        #region Check Collections

        public static readonly CheckCollection settingsChecks = new CheckCollection(
            new CheckCollection.Check(
                "Android Texture Compression", "Should use ASTC",
                () => EditorUserBuildSettings.androidBuildSubtarget == MobileTextureSubtarget.ASTC,
                () => EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC
            ),
            new CheckCollection.Check(
                "Android VR", "Should be enabled",
                () => PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android),
                () => PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.Android, true)
            ),
            new CheckCollection.Check(
                "Standalone VR", "Should be enabled",
                () => PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Standalone),
                () => PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.Standalone, true)
            ),
            new CheckCollection.Check(
                "Standalone VR", "Oculus SDK should be selected",
                () => {
                    string[] activeSdks = PlayerSettings.GetVirtualRealitySDKs(BuildTargetGroup.Standalone);
                    return Array.IndexOf(activeSdks, "Oculus") != -1;
                },
                () => PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Standalone, new string[] { "Oculus" })
            ),
            new CheckCollection.Check(
                "Android VR", "Oculus SDK should be selected",
                () => {
                    string[] activeSdks = PlayerSettings.GetVirtualRealitySDKs(BuildTargetGroup.Android);
                    return Array.IndexOf(activeSdks, "Oculus") != -1;
                },
                () => PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Android, new string[] { "Oculus" })
            ),
            new CheckCollection.Check(
                "Oculus SDK", "Should use V2 Signing",
                () => PlayerSettings.VROculus.v2Signing,
                () => PlayerSettings.VROculus.v2Signing = true
            ),
            new CheckCollection.Check(
                "Stereo Rendering Mode", "Should be set to Single Pass",
                () => PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass,
                () => PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass
            ),
            new CheckCollection.Check(
                "Render Pipeline", "Should use Unity Standard Rendering",
                () => GraphicsSettings.renderPipelineAsset == null
            )
        );

        public static readonly CheckCollection sceneRequirementChecks = new CheckCollection(
            new CheckCollection.Check(
                "Cameras", "No cameras should exist in scene",
                () => UnityEngine.Object.FindObjectsOfType<Camera>().Length == 0,
                () => {
                    foreach (var item in UnityEngine.Object.FindObjectsOfType<Camera>())
                        UnityEngine.Object.DestroyImmediate(item.gameObject);
                }
            ),
            new CheckCollection.Check(
                "SpawnPoints", "Should have a SpawnPoints Object",
                () => GameObject.Find("SpawnPoints") != null,
                () => new GameObject("SpawnPoints")
            ),
            new CheckCollection.Check(
                "SpawnPoints", "Should have at least 5 SpawnPoints",
                () => GameObject.Find("SpawnPoints")?.transform.childCount >= 5
            ),
            new CheckCollection.Check(
                "Lightmap Mode", "Should be Non-Directional",
                () => LightmapSettings.lightmapsMode == LightmapsMode.NonDirectional,
                () => LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional
            ),
            new CheckCollection.Check(
                "Reflection Probes", "Should have one",
                () => UnityEngine.Object.FindObjectsOfType<ReflectionProbe>().Length == 1
            ),
            new CheckCollection.Check(
                "Audio Source Settings", "All Audio Sources have Settings",
                () => UnityEngine.Object.FindObjectsOfType<AudioSource>()
                    .Select(x => x.GetComponent<Audio.AudioSourceSettings>())
                    .Where(x => x == null)
                    .Count() == 0,
                () => UnityEngine.Object.FindObjectsOfType<AudioSource>()
                    .Where(x => x.GetComponent<Audio.AudioSourceSettings>() == null)
                    .ToList()
                    .ForEach(x => x.gameObject.AddComponent<Audio.AudioSourceSettings>())
            ),
            new CheckCollection.Check(
                "Model Formats", "All models must be natively supported by Unity: .fbx, .dae (Collada), .3ds, .dxf, or .obj",
                () => UnityEngine.Object.FindObjectsOfType<MeshCollider>().Select(x => x.sharedMesh)
                    .Union(
                        UnityEngine.Object.FindObjectsOfType<MeshFilter>().Select(x => x.sharedMesh)
                    )
                    .Where(x => x != null)
                    .All(x =>
                    {
                        var path = AssetDatabase.GetAssetPath(x);
                        bool valid = true;
                        if (path.StartsWith("Library/unity default resources"))
                        {
                            return true;
                        }
                        var parts = path.Split('.');
                        valid = parts.Count() > 1;
                        if (valid)
                        {
                            var extension = parts[parts.Length - 1].ToLower();
                            valid = VALID_MODEL_EXTENSIONS.Contains(extension);
                        }
                        if (!valid)
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            Debug.LogError($"Invalid Model Format used: {x} ({path})", asset);
                        }
                        return valid;
                    })
            )
        );

        public static readonly CheckCollection worldInfoChecks = new CheckCollection(
            () => AssetDatabase.FindAssets("t:WorldInfo")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<WorldInfo>)
                .SelectMany(worldInfo => new CheckCollection.Check[]
                {
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Should have a Scene",
                        () => worldInfo.sceneAsset != null
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Should have a portal label",
                        () => !string.IsNullOrWhiteSpace(worldInfo.portalLabel)
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Should have a portal texture",
                        () => worldInfo.portalTexture != null
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Should have a preview texture",
                        () => {
                            var passing = worldInfo.previewTexture != null;
                            if (!passing)
                            {
                                Debug.LogError("WorldInfo Should have a preview texture", worldInfo);
                            }
                            return passing;
                        }
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Preview Texture should be at least 512x512",
                        () => {
                            var exists = worldInfo.previewTexture != null;
                            var passing = exists && worldInfo.previewTexture.width >= 512 && worldInfo.previewTexture.height >= 512;
                            if (exists && !passing)
                            {
                                Debug.LogError("Preview Texture needs to be at least 512x512", worldInfo.previewTexture);
                            }
                            return passing;
                        }
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Preview Texture should have mipmaps enabled",
                        () => worldInfo.previewTexture != null && worldInfo.previewTexture.mipmapCount > 1,
                        () => {
                            var path = AssetDatabase.GetAssetPath(worldInfo.previewTexture);
                            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
                            importer.mipmapEnabled = true;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        }
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Bundles should have a Render Settings File",
                        () => {
                            var sceneAssetName = AssetDatabase.GetAssetPath(worldInfo.sceneAsset);
                            var sceneBundleName = AssetDatabase.GetImplicitAssetBundleName(sceneAssetName);
                            var bundleNames = AssetDatabase.GetAssetBundleDependencies(sceneBundleName, true);
                            return AssetDatabase.FindAssets("t:RenderSettingsFile")
                                .Select(AssetDatabase.GUIDToAssetPath)
                                .Select(AssetDatabase.GetImplicitAssetBundleName)
                                .Any(renderSettingsBundleName => bundleNames.Contains(renderSettingsBundleName));
                        }
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Portal Texture should be in the same bundle as WorldInfo",
                        () => {
                            var exists = worldInfo.portalTexture != null;
                            if (!exists) return false;
                            var worldInfoBundleName = AssetDatabase.GetImplicitAssetBundleName(AssetDatabase.GetAssetPath(worldInfo));
                            var portalTextureBundleName = AssetDatabase.GetImplicitAssetBundleName(AssetDatabase.GetAssetPath(worldInfo.portalTexture));
                            return worldInfoBundleName == portalTextureBundleName;
                        },
                        () =>
                        {
                            var exists = worldInfo.portalTexture != null;
                            if (!exists) return;
                            var worldInfoBundleName = AssetDatabase.GetImplicitAssetBundleName(AssetDatabase.GetAssetPath(worldInfo));
                            AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(worldInfo.portalTexture)).SetAssetBundleNameAndVariant(worldInfoBundleName, "");
                        }
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Preview Texture should be in the same bundle as WorldInfo",
                        () => {
                            var exists = worldInfo.previewTexture != null;
                            if (!exists) return false;
                            var worldInfoBundleName = AssetDatabase.GetImplicitAssetBundleName(AssetDatabase.GetAssetPath(worldInfo));
                            var previewTextureBundleName = AssetDatabase.GetImplicitAssetBundleName(AssetDatabase.GetAssetPath(worldInfo.previewTexture));
                            return worldInfoBundleName == previewTextureBundleName;
                        },
                        () =>
                        {
                            var exists = worldInfo.previewTexture != null;
                            if (!exists) return;
                            var worldInfoBundleName = AssetDatabase.GetImplicitAssetBundleName(AssetDatabase.GetAssetPath(worldInfo));
                            AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(worldInfo.previewTexture)).SetAssetBundleNameAndVariant(worldInfoBundleName, "");
                        }
                    ),
                })
        );

        #endregion
    }
}
