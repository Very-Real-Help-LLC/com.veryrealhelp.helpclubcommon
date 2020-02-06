using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VRH
{
    public class WorldEditorWindow : EditorWindow
    {
        enum BundleType { Portal, Scene, Sky, Stuff };

        private bool scanned = false;
        private Dictionary<BundleType, string> bundleNames;
        private List<string> invalidBundleNames;
        private bool hasLightProbes = false;

        private bool isBuilding = false;

        private const string goodIconName = "sv_icon_dot3_sml";
        private const string warnIconName = "sv_icon_dot12_sml";
        private const string errorIconName = "sv_icon_dot14_sml";
        private Texture goodIcon;
        private Texture warnIcon;
        private Texture errorIcon;

        [MenuItem("VRH/World Editor Window")]
        static void Init()
        {
            WorldEditorWindow window = GetWindow<WorldEditorWindow>(false, "VRH World Editing", true);
            window.goodIcon = EditorGUIUtility.IconContent(goodIconName).image;
            window.warnIcon = EditorGUIUtility.IconContent(warnIconName).image;
            window.errorIcon = EditorGUIUtility.IconContent(errorIconName).image;
            window.Show();
        }

        private void OnGUI()
        {
            if (!scanned || GUILayout.Button(EditorGUIUtility.IconContent("Refresh")))
                Scan();

            GUILayout.Label("Bundles Status");
            foreach (BundleType bundleType in (BundleType[])Enum.GetValues(typeof(BundleType)))
            {
                bool present = bundleNames != null && bundleNames.ContainsKey(bundleType);
                GUILayout.Label(new GUIContent(present ? bundleNames[bundleType] : bundleType.ToString() + " (Missing)", present ? goodIcon : errorIcon));
            }

            GUILayout.Space(12);

            if (invalidBundleNames != null && invalidBundleNames.Count > 0)
            {
                GUILayout.Label("Invalid Bundles");
                foreach (string bundleName in invalidBundleNames)
                    GUILayout.Label(new GUIContent(bundleName, warnIcon));

                GUILayout.Space(12);
            }

            GUILayout.Label(new GUIContent("Light Probes", hasLightProbes ? goodIcon : errorIcon));

            GUILayout.Space(12);

            GUI.enabled = !isBuilding;
            if (GUILayout.Button("Build Bundles"))
            {
                isBuilding = true;
                BuildScript.BuildWorldBundles();
                isBuilding = false;
            }
            GUI.enabled = true;
        }

        private void Scan()
        {
            bundleNames = new Dictionary<BundleType, string>();
            invalidBundleNames = new List<string>();
            foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
            {
                if (bundleName.EndsWith("_portal") || bundleName.EndsWith("_portalx"))
                    bundleNames[BundleType.Portal] = bundleName;
                else if (bundleName.EndsWith("_scene"))
                    bundleNames[BundleType.Scene] = bundleName;
                else if (bundleName.EndsWith("_sky"))
                    bundleNames[BundleType.Sky] = bundleName;
                else if (bundleName.EndsWith("_stuff"))
                    bundleNames[BundleType.Stuff] = bundleName;
                else
                    invalidBundleNames.Add(bundleName);
            }
            scanned = true;

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            hasLightProbes = LightmapSettings.lightProbes.count > 0;
        }
    }
}