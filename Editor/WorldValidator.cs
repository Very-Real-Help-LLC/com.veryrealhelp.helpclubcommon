using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using VeryRealHelp.HelpClubCommon.World;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class WorldValidator
    {
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
                sceneRequirementChecks
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
            )
        );

        #endregion
    }
}
