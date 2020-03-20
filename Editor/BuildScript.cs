using System.Collections;
using UnityEditor;
using UnityEngine;

namespace VRH
{

    public static class BuildScript
    {
        public static BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.StrictMode & BuildAssetBundleOptions.ChunkBasedCompression;

        [MenuItem("VRH/Bundles/Build All")]
        public static void BuildWorldBundles()
        {
            BuildBundlesAndroid();
            BuildBundlesStandaloneWindows();
            BuildBundlesStandaloneOSX();
        }

        public static IEnumerator BuildWorldBundlesCoroutine()
        {
            BuildBundlesAndroid();
            yield return null;
            BuildBundlesStandaloneWindows();
            yield return null;
            BuildBundlesStandaloneOSX();
            yield return null;
        }
        
        [MenuItem("VRH/Bundles/Build Android Bundles")]
        public static void BuildBundlesAndroid()
        {
            Debug.Log("Building Android Bundles...");
            BuildPipeline.BuildAssetBundles("AssetBundles/Android", bundleOptions, BuildTarget.Android);
            Debug.Log("Finished Building Android Bundles");
        }

        [MenuItem("VRH/Bundles/Build Standalone Windows Bundles")]
        public static void BuildBundlesStandaloneWindows()
        {
            Debug.Log("Building Standalone Windows Bundles...");
            BuildPipeline.BuildAssetBundles("AssetBundles/StandaloneWindows", bundleOptions, BuildTarget.StandaloneWindows);
            Debug.Log("Finished Building Standalone Windows Bundles");
        }

        [MenuItem("VRH/Bundles/Build Standalone OSX Bundles")]
        public static void BuildBundlesStandaloneOSX()
        {
            Debug.Log("Building Standalone OSX Bundles...");
            BuildPipeline.BuildAssetBundles("AssetBundles/StandaloneOSX", bundleOptions, BuildTarget.StandaloneOSX);
            Debug.Log("Finished Building Standalone OSX Bundles");
        }

    }

}
