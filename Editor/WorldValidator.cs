using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using VeryRealHelp.HelpClubCommon.World;
using Object = UnityEngine.Object;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class WorldValidator
    {
        public static readonly string[] VALID_MODEL_EXTENSIONS = { "fbx", "dae", "3ds", "dxf", "obj", "asset", "mesh" };

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
                "Render Pipeline", "Should use Unity Standard Rendering",
                () => GraphicsSettings.renderPipelineAsset == null
            ),
            new CheckCollection.Check(
                $"Quality Settings", "Auto applying Quality Settings from Preset",
                () => {
                    var cachedActiveObject = Selection.activeObject;
                    Preset qualitySettings = (Preset)AssetDatabase.LoadAssetAtPath("Packages/com.veryrealhelp.helpclubcommon/Presets/QualitySettings.preset", typeof(Preset));
                    Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("QualitySettings"); //Workaround: no metheds exposed in 2019 LTS to directly access editors quality settings (Project/Quality)
                    bool presetEquals = qualitySettings.DataEquals(Selection.activeObject);
                    Selection.activeObject = cachedActiveObject;
                    return presetEquals;
                },
                () =>
                {
                    var cachedActiveObject = Selection.activeObject;
                    Preset qualitySettings = (Preset)AssetDatabase.LoadAssetAtPath("Packages/com.veryrealhelp.helpclubcommon/Presets/QualitySettings.preset", typeof(Preset));
                    Selection.activeObject = SettingsService.OpenProjectSettings("Project/Quality");
                    Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("QualitySettings");
                    qualitySettings.ApplyTo(Selection.activeObject);
                    Selection.activeObject = cachedActiveObject;
                }
            ),
            new CheckCollection.Check(
                $"TagManager Settings", "Auto applying Tags and Layers from Preset",
                () => {
                    var cachedActiveObject = Selection.activeObject;
                    Preset tagManagerPreset = (Preset)AssetDatabase.LoadAssetAtPath("Packages/com.veryrealhelp.helpclubcommon/Presets/TagManager.preset", typeof(Preset));
                    Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("TagManager");
                    bool presetEquals = tagManagerPreset.DataEquals(Selection.activeObject);
                    Selection.activeObject = cachedActiveObject;
                    return presetEquals;
                },
                () =>
                {
                    var cachedActiveObject = Selection.activeObject;
                    Preset tagManagerPreset = (Preset)AssetDatabase.LoadAssetAtPath("Packages/com.veryrealhelp.helpclubcommon/Presets/TagManager.preset", typeof(Preset));
                    Selection.activeObject = SettingsService.OpenProjectSettings("Project/TagManager");
                    Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("TagManager");
                    tagManagerPreset.ApplyTo(Selection.activeObject);
                    Selection.activeObject = cachedActiveObject;
                }
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
                        // embedded meshes generated by ProDrawCallOptimizer
                        if (path == "")
                        {
                            return true;
                        }
                        // unity meshes
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
            ),
            new CheckCollection.Check(
                "NavMesh Collider", "NavMesh baked for scene & NavMesh are colliders setup in scene",
                () =>
                {
                    //NavMesh.asset exists
                    var activeScene = SceneManager.GetActiveScene().name;
                    var navMeshAssetPath = $"{Application.dataPath}/Scenes/{activeScene}/NavMesh.asset";
                    if(!System.IO.File.Exists(navMeshAssetPath))
                    {
                        return false;
                    }
                    //NavMesh Collider exists in scene
                    if (GameObject.Find(NavMeshCreator.NavMeshColliderPrefab) == null)
                    {
                        return false;
                    }
                    return true;
                },
                () => {
                    NavMeshCreator.CreateNavMesh();
                }
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
                        "Check Asset Bundle Assignments",
                        "Ensure all scene assets are assigned to the correct asset bundles",
                        test: () => {
                            // Check if assets are already assigned to the correct bundles
                            return AreAssetsAssignedToCorrectBundles();
                        },
                        fix: () => {
                            // Assign assets to the correct bundles if needed
                            AssignAssetsToBundles();
                        }
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "WorldInfo should be in the info bundle",
                        () =>
                        {
                            // Define the bundle names using Application.productName as a prefix
                            var assetBundlePrefix = Application.productName.ToLower().Replace(" ", "_");
                            var infoBundleName = $"{assetBundlePrefix}_info";
                            return CheckAssetBundleName(worldInfo, infoBundleName);
                        }, () =>
                        {
                            // Define the bundle names using Application.productName as a prefix
                            var assetBundlePrefix = Application.productName.ToLower().Replace(" ", "_");
                            var infoBundleName = $"{assetBundlePrefix}_info";
                            TrySetAssetBundleName(worldInfo, infoBundleName);
                        }
                    ),
                    new CheckCollection.Check(
                        $"WorldInfo: {worldInfo.name}", "Bundles should have a Render Settings File",
                        test: () => {
                            var sceneAssetName = AssetDatabase.GetAssetPath(worldInfo.sceneAsset);
                            var sceneBundleName = AssetDatabase.GetImplicitAssetBundleName(sceneAssetName);
                            var bundleNames = AssetDatabase.GetAssetBundleDependencies(sceneBundleName, true);
                            return AssetDatabase.FindAssets("t:RenderSettingsFile")
                                .Select(AssetDatabase.GUIDToAssetPath)
                                .Select(AssetDatabase.GetImplicitAssetBundleName)
                                .Any(renderSettingsBundleName => bundleNames.Contains(renderSettingsBundleName));
                        },
                        fix: () => {
                            var sceneAssetName = AssetDatabase.GetAssetPath(worldInfo.sceneAsset);
                            var sceneBundleName = AssetDatabase.GetImplicitAssetBundleName(sceneAssetName);
                            var bundleNames = AssetDatabase.GetAssetBundleDependencies(sceneBundleName, true);

                            // Assuming we use the first bundle name from the dependencies
                            string targetBundleName = bundleNames.FirstOrDefault();
                            if (string.IsNullOrEmpty(targetBundleName))
                            {
                                Debug.LogWarning("No bundle dependencies found. Cannot assign RenderSettingsFile to a bundle.");
                                return;
                            }

                            string[] renderSettingsFiles = AssetDatabase.FindAssets("t:RenderSettingsFile");
                            if (renderSettingsFiles.Length > 0)
                            {
                                string firstRenderSettingsFilePath = AssetDatabase.GUIDToAssetPath(renderSettingsFiles[0]);
                                AssetImporter importer = AssetImporter.GetAtPath(firstRenderSettingsFilePath);
                                importer.SetAssetBundleNameAndVariant(targetBundleName, "");

                                Debug.Log($"RenderSettingsFile '{firstRenderSettingsFilePath}' assigned to bundle '{targetBundleName}'.");
                            }
                            else
                            {
                                Debug.LogWarning("No RenderSettingsFile found to assign to bundle.");
                            }
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
        
        [MenuItem("VRH/Automation/Delete All Asset Bundles")]
        public static void DeleteAllAssetBundles()
        {
            // Get all asset bundle names currently in use
            var allBundleNames = AssetDatabase.GetAllAssetBundleNames();

            foreach (var bundleName in allBundleNames)
            {
                // Remove the asset bundle and its dependencies
                var result = AssetDatabase.RemoveAssetBundleName(bundleName, true);

                if (result)
                {
                    Debug.Log($"Removed asset bundle: {bundleName}");
                }
                else
                {
                    Debug.LogWarning($"Failed to remove asset bundle: {bundleName}");
                }
            }

            // Save and refresh the AssetDatabase to reflect the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Deleted all asset bundles. Total: {allBundleNames.Length}");
        }
        
        [MenuItem("VRH/Automation/Assign Assets To Bundles")]        
        public static void AssignAssetsToBundles()
        {
            // Define the bundle names using Application.productName as a prefix
            var assetBundlePrefix = Application.productName.ToLower().Replace(" ", "_");
            
            var sceneBundleName = $"{assetBundlePrefix}_scene";
            var assetsBundleName = $"{assetBundlePrefix}_stuff";

            // Assign the current scene to a bundle
            var scenePath = SceneManager.GetActiveScene().path;
            AssetImporter.GetAtPath(scenePath).SetAssetBundleNameAndVariant(sceneBundleName, "");

            // Iterate over all GameObjects in the scene
            foreach (var obj in Object.FindObjectsOfType<GameObject>())
            {
                SetAssetBundleNameForAllProperties(obj, assetsBundleName);
                Object prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (prefabAsset != null)
                {
                    SetAssetBundleNameForAllProperties(prefabAsset, assetsBundleName);    
                }
                
                // Get all components on the GameObject, including children and inactive components
                var components = obj.GetComponentsInChildren<Component>(true);
                foreach (var component in components)
                {
                    SetAssetBundleNameForAllProperties(component, assetsBundleName);
                }
            }

            var skyboxMaterial = RenderSettings.skybox;
            if (skyboxMaterial != null)
            {
                SetAssetBundleNameForAllProperties(skyboxMaterial, assetsBundleName);
            }

            Debug.Log("Assets have been assigned to their respective bundles.");
        }

        private static bool AreAssetsAssignedToCorrectBundles()
        {
            // Define the bundle names using Application.productName as a prefix
            var assetBundlePrefix = Application.productName.ToLower().Replace(" ", "_");
            
            var sceneBundleName = $"{assetBundlePrefix}_scene";
            var assetsBundleName = $"{assetBundlePrefix}_stuff";

            // Check the current scene's bundle assignment
            var scenePath = SceneManager.GetActiveScene().path;
            var sceneAssetBundleName = AssetImporter.GetAtPath(scenePath).assetBundleName;
            if (sceneAssetBundleName != sceneBundleName)
            {
                return false; // Scene is not assigned to the correct bundle
            }

            // Iterate over all GameObjects in the scene
            foreach (var obj in Object.FindObjectsOfType<GameObject>())
            {
                Object prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (prefabAsset != null)
                {
                    CheckAssetBundleNameForAllProperties(prefabAsset, assetsBundleName);    
                }
                
                CheckAssetBundleNameForAllProperties(obj, assetsBundleName);
                
                // Get all components on the GameObject, including children and inactive components
                var components = obj.GetComponentsInChildren<Component>(true);
                foreach (var component in components)
                {
                    var valid = CheckAssetBundleNameForAllProperties(component, assetsBundleName);
                    if (!valid) return false;
                }
            }

            var skyboxMaterial = RenderSettings.skybox;
            if (skyboxMaterial != null)
            {
                var valid = CheckAssetBundleNameForAllProperties(skyboxMaterial, assetsBundleName);
                if (!valid) return false;
            }

            // All assets are correctly assigned
            return true;
        }

        private static void SetAssetBundleNameForAllProperties(Object unityObject, string assetsBundleName)
        {
            // Set the asset bundle name for the root object
            TrySetAssetBundleName(unityObject, assetsBundleName);

            // Start the recursive process for the initial object
            SetAssetBundleNameRecursively(unityObject);
            return;

            // Recursive function to set asset bundle names for all dependencies
            void SetAssetBundleNameRecursively(Object obj)
            {
                var so = new SerializedObject(obj);
                var sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue != null)
                    {
                        // Set the asset bundle name for the dependency
                        TrySetAssetBundleName(sp.objectReferenceValue, assetsBundleName);

                        // Recursively set asset bundle names for the dependencies of the current object
                        if (sp.objectReferenceValue != unityObject) // Prevent infinite recursion
                        {
                            SetAssetBundleNameRecursively(sp.objectReferenceValue);
                        }
                    }
                }
            }
        }

        private static bool CheckAssetBundleNameForAllProperties(Object unityObject, string assetsBundleName)
        {
            // Start the recursive checking process
            return CheckRecursively(unityObject);

            // Local function for recursive checking
            bool CheckRecursively(Object obj)
            {
                var so = new SerializedObject(obj);
                var sp = so.GetIterator();

                if (!CheckAssetBundleName(obj, assetsBundleName))
                {
                    // Asset is not assigned to the correct bundle
                    return false;
                }

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue != null)
                    {
                        if (!CheckAssetBundleName(sp.objectReferenceValue, assetsBundleName))
                        {
                            // Asset is not assigned to the correct bundle
                            return false;
                        }

                        // Recurse into the current property's referenced object, avoiding infinite recursion
                        if (sp.objectReferenceValue != obj && !CheckRecursively(sp.objectReferenceValue))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }


        private static bool CanAssetBeImported(string assetPath)
        {
            // Don't try to touch default unity assets
            if(assetPath == "Library/unity default resources" || assetPath == "Resources/unity_builtin_extra") return false;

            // Exclude script assets and package assets
            if (string.IsNullOrEmpty(assetPath) || assetPath.EndsWith(".cs") ||
                assetPath.StartsWith("Packages/")) return false;
            
            return true;
        }

        private static bool TrySetAssetBundleName(Object unityObject, string assetBundleName)
        {
            var baseAssetPath = AssetDatabase.GetAssetPath(unityObject);
            if(!CanAssetBeImported(baseAssetPath)) return false;
            var baseImportedAsset = AssetImporter.GetAtPath(baseAssetPath);
            if (baseImportedAsset == null) return false;
            baseImportedAsset.SetAssetBundleNameAndVariant(assetBundleName, "");
            return true;
        }

        private static bool CheckAssetBundleName(Object unityObject, string assetBundleName)
        {
            var baseAssetPath = AssetDatabase.GetAssetPath(unityObject);
            
            // We only need to check importable assets
            if(!CanAssetBeImported(baseAssetPath)) return true;
            
            var baseImportedAsset = AssetImporter.GetAtPath(baseAssetPath);
            if (baseImportedAsset != null) return baseImportedAsset.assetBundleName == assetBundleName;
            
            Debug.LogWarning($"Failed to import asset at path {baseAssetPath}");
            return false;
        }

    }
}
