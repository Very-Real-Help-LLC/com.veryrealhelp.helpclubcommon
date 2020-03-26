using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public class WorldEditorWindow : EditorWindow
    {
        private bool scanQueued = false;
        private bool buildQueued = false;
        private bool scanned = false;

        private bool isScanning = false;
        private string scanningStatus;
        private bool isBuilding = false;
        private string buildingStatus;

        private const string goodIconName = "sv_icon_dot3_sml";
        private const string warnIconName = "sv_icon_dot12_sml";
        private const string errorIconName = "sv_icon_dot14_sml";
        private Texture goodIcon;
        private Texture warnIcon;
        private Texture errorIcon;

        /* NOTE: these CheckCollection instances should be moved to a shared location when they can be used during automated testing */

        private readonly CheckCollection settingsChecks = new CheckCollection(
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


        public CheckCollection bundleChecks = new CheckCollection(
            new CheckCollection.Check(
                "Portal Bundle", "should exist with name '..._portal'",
                () => {
                    foreach (var item in AssetDatabase.GetAllAssetBundleNames())
                        if (item.EndsWith("_portal"))
                            return true;
                        else if (item.EndsWith("_portalx"))
                            return true;
                    return false;
                }
            ),
            new CheckCollection.Check(
                "Portal Bundle", "Should contain only 'ReflectionProbe-0'",
                () => {
                    string bundleName = null;
                    foreach (var item in AssetDatabase.GetAllAssetBundleNames())
                        if (item.EndsWith("_portal"))
                        {
                            bundleName = item;
                            break;
                        }
                        else if (item.EndsWith("_portalx"))
                        {
                            bundleName = item;
                            break;
                        }
                    if (bundleName == null)
                        return false;
                    var paths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                    return paths.Length == 1 && Path.GetFileNameWithoutExtension(paths[0]) == "ReflectionProbe-0";
                }
            ),
            new CheckCollection.Check(
                "Scene Bundle", "should exist with name '..._scene'",
                () => {
                    foreach (var item in AssetDatabase.GetAllAssetBundleNames())
                        if (item.EndsWith("_scene"))
                            return true;
                    return false;
                }
            ),
            new CheckCollection.Check(
                "Sky Bundle", "should exist with name '..._sky'",
                () => {
                    foreach (var item in AssetDatabase.GetAllAssetBundleNames())
                        if (item.EndsWith("_sky"))
                            return true;
                    return false;
                }
            ),
            new CheckCollection.Check(
                "Sky Bundle", "Should contain 'Render Settings'",
                () => {
                    string bundleName = null;
                    foreach (var item in AssetDatabase.GetAllAssetBundleNames())
                        if (item.EndsWith("_sky"))
                        {
                            bundleName = item;
                            break;
                        }
                    if (bundleName == null)
                        return false;
                    foreach (var path in AssetDatabase.GetAssetPathsFromAssetBundle(bundleName))
                        if (Path.GetFileNameWithoutExtension(path).ToLower() == "render settings")
                            return true;
                    return false;
                }
            ),
            new CheckCollection.Check(
                "Stuff Bundle", "should exist with name '..._stuff'",
                () => {
                    foreach (var item in AssetDatabase.GetAllAssetBundleNames())
                        if (item.EndsWith("_stuff"))
                            return true;
                    return false;
                }
            )
        );


        public CheckCollection sceneRequirementChecks = new CheckCollection(
            new CheckCollection.Check(
                "Cameras", "No cameras should exist in scene",
                () => FindObjectsOfType<Camera>().Length == 0,
                () => {
                    foreach (var item in FindObjectsOfType<Camera>())
                        DestroyImmediate(item.gameObject);
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
                () => FindObjectsOfType<ReflectionProbe>().Length == 1
            )
        );

        public CheckCollection sceneSuggestionChecks = new CheckCollection(
            new CheckCollection.Check(
                "Light Probes", "Should be present",
                () => LightmapSettings.lightProbes != null && LightmapSettings.lightProbes.count > 0
            ),
            new CheckCollection.Check(
                "Realtime Lights", "Should not be used",
                () => FindObjectsOfType<Light>().Where(x => x.lightmapBakeType != LightmapBakeType.Baked).Count() == 0,
                () => {
                    foreach (Light light in FindObjectsOfType<Light>().Where(x => x.lightmapBakeType != LightmapBakeType.Baked))
                        light.lightmapBakeType = LightmapBakeType.Baked;
                }
            ),
            new CheckCollection.Check(
                "Triangle Count", "Should be under 100k",
                () => FindObjectsOfType<MeshFilter>().Aggregate(0, (acc, x) => acc + (x.sharedMesh != null ? x.sharedMesh.triangles.Length / 3 : 0)) < 100000
            ),
            new CheckCollection.Check(
                "Occlusion Culling", "Should be baked",
                () => StaticOcclusionCulling.umbraDataSize > 0
            ),
            new CheckCollection.Check(
                "Baked GI", "Should be enabled",
                () => Lightmapping.bakedGI,
                () => Lightmapping.bakedGI = true
            ),
            new CheckCollection.Check(
                "Realtime GI", "Should be disabled",
                () => !Lightmapping.realtimeGI,
                () => Lightmapping.realtimeGI = false
            ),
            new CheckCollection.Check(
                "Lightmap Max Size", "Should be 4096",
                () => LightmapEditorSettings.maxAtlasSize == 4096,
                () => LightmapEditorSettings.maxAtlasSize = 4096
            ),
            new CheckCollection.Check(
                "Lightmap Count", "Should be 2 or less",
                () => LightmapSettings.lightmaps.Length <= 2
            ),
            new CheckCollection.Check(
                "Shaders", "Should use 3 or fewer passes",
                () => {
                    bool valid = true;
                    HashSet<Material> materials = new HashSet<Material>();
                    foreach (var renderer in FindObjectsOfType<Renderer>())
                        foreach (var material in renderer.sharedMaterials)
                            if (material != null && material.passCount > 3)
                            {
                                valid = false;
                                materials.Add(material);
                            }
                    foreach (Material material in materials)
                        Debug.Log("Material " + material.name + " uses shader " + material.shader.name + " with " + material.passCount + " passes", material);
                    return valid;
                }
            )
        );
       

        [MenuItem("VRH/World Editor Window")]
        static void Init()
        {
            WorldEditorWindow window = GetWindow<WorldEditorWindow>(false, "VRH World Editing", true);
            window.goodIcon = EditorGUIUtility.IconContent(goodIconName).image;
            window.warnIcon = EditorGUIUtility.IconContent(warnIconName).image;
            window.errorIcon = EditorGUIUtility.IconContent(errorIconName).image;
            window.Show();
        }

        private void Awake()
        {
            Scan();
        }

        private void Update()
        {
            if (!scanned)
                scanQueued = true;
            if (scanQueued)
            {
                scanQueued = false;
                Scan();
            }
            if (buildQueued)
            {
                buildQueued = false;
                BuildBundles();
            }
        }

        private bool RenderCheckCollectionIsValid(CheckCollection collection, Texture invalidIcon)
        {
            if (collection.invalidCache == null)
                return false;
            if (collection.invalidCache.Length != 0)
            {
                foreach (var check in collection.invalidCache)
                {
                    GUILayout.Label(new GUIContent(check.label + ": " + check.invalidMessage, invalidIcon));
                    if (check.fix != null && GUILayout.Button("fix"))
                    {
                        check.fix();
                        scanQueued = true;
                    }
                }
                return false;
            }
            return true;
        }

        private void OnGUI()
        {
            if (isScanning)
            {
                GUILayout.Label(GUIContent.none);
                EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), 1f, scanningStatus);
            }
            else if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh")))
                scanQueued = true;

            GUILayout.Label("Project Settings");
            bool settingsPassed = RenderCheckCollectionIsValid(settingsChecks, errorIcon);
            if (settingsPassed)
                GUILayout.Label(new GUIContent("All Checks Passed", goodIcon));

            GUILayout.Space(12);

            GUILayout.Label("Bundles");
            bool bundlesPassed = RenderCheckCollectionIsValid(bundleChecks, errorIcon);
            if (bundlesPassed)
                GUILayout.Label(new GUIContent("All Checks Passed", goodIcon));

            GUILayout.Space(12);

            GUILayout.Label("Scene");
            bool scenePassed = RenderCheckCollectionIsValid(sceneRequirementChecks, errorIcon);
            if (scenePassed && RenderCheckCollectionIsValid(sceneSuggestionChecks, warnIcon))
                GUILayout.Label(new GUIContent("All Checks Passed", goodIcon));

            GUILayout.Space(12);

            bool canBuild = settingsPassed && bundlesPassed && scenePassed;
            GUI.enabled = canBuild;
            if (isBuilding)
            {
                GUILayout.Label(GUIContent.none);
                EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), 1f, buildingStatus);
            }
            else if (GUILayout.Button("Build Bundles"))
                buildQueued = true;
            GUI.enabled = true;
        }

        private void Scan() => EditorCoroutineUtility.StartCoroutine(ScanCoroutine(), this);

        private IEnumerator ScanCoroutine()
        {
            isScanning = true;
            scanningStatus = "Scanning Settings...";
            yield return null;
            settingsChecks.UpdateInvalidCache();
            yield return null;
            scanningStatus = "Scanning Bundles...";
            yield return null;
            bundleChecks.UpdateInvalidCache();
            yield return null;
            scanningStatus = "Scanning Scene...";
            yield return null;
            sceneRequirementChecks.UpdateInvalidCache();
            yield return null;
            sceneSuggestionChecks.UpdateInvalidCache();
            yield return null;
            scanningStatus = "Done";
            isScanning = false;
            scanned = true;
        }

        private void BuildBundles() => EditorCoroutineUtility.StartCoroutine(BuildBundlesCoroutine(), this);

        private IEnumerator BuildBundlesCoroutine()
        {
            isBuilding = true;

            buildingStatus = "Building Android Bundles...";
            yield return null;
            BuildScript.BuildBundlesAndroid();
            yield return null;

            buildingStatus = "Building Standalone Windows Bundles...";
            yield return null;
            BuildScript.BuildBundlesStandaloneWindows();
            yield return null;

            buildingStatus = "Building Standalone OSX Bundles...";
            yield return null;
            BuildScript.BuildBundlesStandaloneOSX();
            yield return null;

            buildingStatus = "Done";
            isBuilding = false;
        }
    }
}