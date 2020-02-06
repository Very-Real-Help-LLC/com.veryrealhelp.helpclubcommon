using UnityEditor;
using UnityEngine;

namespace VRH
{

    public static class BuildScript
    {
        [MenuItem("VRH/Worlds/Build Bundles")]
        public static void BuildWorldBundles()
        {
            var options = BuildAssetBundleOptions.StrictMode & BuildAssetBundleOptions.ChunkBasedCompression;
            Debug.Log("Building Standalone Windows Bundles...");
            BuildPipeline.BuildAssetBundles("AssetBundles/StandaloneWindows", options, BuildTarget.StandaloneWindows);
            Debug.Log("Building Android Bundles...");
            BuildPipeline.BuildAssetBundles("AssetBundles/Android", options, BuildTarget.Android);
            Debug.Log("Finished Building Bundles");
        }

    }

}
