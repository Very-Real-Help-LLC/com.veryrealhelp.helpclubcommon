using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Editor
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
            string directory = "AssetBundles/Android";
            Directory.CreateDirectory(directory);
            BuildPipeline.BuildAssetBundles(directory, bundleOptions, BuildTarget.Android);
            Debug.Log("Finished Building Android Bundles");
        }

        [MenuItem("VRH/Bundles/Build Standalone Windows Bundles")]
        public static void BuildBundlesStandaloneWindows()
        {
            Debug.Log("Building Standalone Windows Bundles...");
            string directory = "AssetBundles/StandaloneWindows";
            Directory.CreateDirectory(directory);
            BuildPipeline.BuildAssetBundles(directory, bundleOptions, BuildTarget.StandaloneWindows);
            Debug.Log("Finished Building Standalone Windows Bundles");
        }

        [MenuItem("VRH/Bundles/Build Standalone OSX Bundles")]
        public static void BuildBundlesStandaloneOSX()
        {
            Debug.Log("Building Standalone OSX Bundles...");
            string directory = "AssetBundles/StandaloneOSX";
            Directory.CreateDirectory(directory);
            BuildPipeline.BuildAssetBundles(directory, bundleOptions, BuildTarget.StandaloneOSX);
            Debug.Log("Finished Building Standalone OSX Bundles");
        }

    }

}
