using System.Collections;
using UnityEditor;
using UnityEngine;

namespace VRH
{

    public static class BuildScript
    {
        public static BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.StrictMode & BuildAssetBundleOptions.ChunkBasedCompression;

        public static void BuildWorldBundles()
        {
            BuildBundlesAndroid();
            BuildBundlesStandaloneWindows();
        }

        public static IEnumerator BuildWorldBundlesCoroutine()
        {
            BuildBundlesAndroid();
            yield return null;
            BuildBundlesStandaloneWindows();
            yield return null;
        }

        public static void BuildBundlesAndroid()
        {
            Debug.Log("Building Android Bundles...");
            BuildPipeline.BuildAssetBundles("AssetBundles/Android", bundleOptions, BuildTarget.Android);
            Debug.Log("Finished Building Android Bundles");
        }

        public static void BuildBundlesStandaloneWindows()
        {
            Debug.Log("Building Standalone Windows Bundles...");
            BuildPipeline.BuildAssetBundles("AssetBundles/StandaloneWindows", bundleOptions, BuildTarget.StandaloneWindows);
            Debug.Log("Finished Building Standalone Windows Bundles");
        }

    }

}
