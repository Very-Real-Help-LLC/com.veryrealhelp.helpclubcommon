using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using VeryRealHelp.HelpClubCommon.World;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;
using System.Linq;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class WorldInfoEditor
    {
        public static void PrepareAllForBuildSynchronously()
        {
            if (Application.isBatchMode)
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log("Preparing WorldInfo objects for build...");
            void RunToCompletion(IEnumerator coroutine) {
                while (coroutine.MoveNext())
                    if (typeof(IEnumerator).IsInstanceOfType(coroutine.Current))
                        RunToCompletion(coroutine.Current as IEnumerator);
            }
            RunToCompletion(PrepareAllForBuildCoroutine());

            Debug.Log("Finished Preparing WorldInfo objects for build.");
            DescribeWorldInfos();
        }

        [MenuItem("VRH/Worlds/Prepare All WorldInfos")]
        public static EditorCoroutine PrepareAllForBuild()
        {
            return EditorCoroutineUtility.StartCoroutineOwnerless(PrepareAllForBuildCoroutine());
        }

        public static IEnumerable<WorldInfo> GetAllWorldInfos()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:WorldInfo"))
                yield return AssetDatabase.LoadAssetAtPath<WorldInfo>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private static IEnumerator PrepareAllForBuildCoroutine()
        {
            foreach (var worldInfo in GetAllWorldInfos())
                yield return PrepareForBuildCoroutine(worldInfo);
        }

        private static IEnumerator PrepareForBuildCoroutine(WorldInfo worldInfo)
        {
            Debug.LogFormat("Preparing WorldInfo ({0}) for build...", worldInfo.name);
            worldInfo.buildTimestamp = Mathf.FloorToInt(System.DateTimeOffset.Now.ToUnixTimeSeconds());
            worldInfo.unityVersion = Application.unityVersion;
            var packageListRequest = UnityEditor.PackageManager.Client.List(true);
            while (!packageListRequest.IsCompleted)
                yield return null;
            foreach (var packageInfo in packageListRequest.Result)
                if (packageInfo.name == "com.veryrealhelp.helpclubcommon")
                    worldInfo.helpClubCommonVersion = packageInfo.version;
            worldInfo.buildProcess = GetCurrentBuildProcess();
            if (worldInfo.sceneAsset == null)
                Debug.LogError("WorldInfo is missing Scene Asset");
            worldInfo.sceneAssetName = AssetDatabase.GetAssetPath(worldInfo.sceneAsset);
            worldInfo.sceneBundle = AssetDatabase.GetImplicitAssetBundleName(worldInfo.sceneAssetName);
            worldInfo.bundleDependencies = AssetDatabase.GetAssetBundleDependencies(worldInfo.sceneBundle, true);

            string isActionTriggered = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
            if(isActionTriggered == "true")
            {
                worldInfo.buildNumber = Environment.GetEnvironmentVariable("GITHUB_REF");
            }
            
            EditorUtility.SetDirty(worldInfo);
            AssetDatabase.SaveAssets();
        }

        private static string GetSceneAssetBundleName(SceneAsset sceneAsset)
        {
            var sceneAssetPath = AssetDatabase.GetAssetPath(sceneAsset);
            foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames())
                foreach (var path in AssetDatabase.GetAssetPathsFromAssetBundle(bundleName))
                    if (path == sceneAssetPath)
                        return bundleName;
            Debug.LogError("WorldInfo.sceneAsset does not belong to any bundles.");
            return null;
        }

        private static WorldInfo.BuildProcess GetCurrentBuildProcess()
        {
            if (Application.isBatchMode)
                return WorldInfo.BuildProcess.BatchMode;
            if (Application.isEditor)
                return WorldInfo.BuildProcess.Editor;
            return WorldInfo.BuildProcess.Unknown;
        }

        [MenuItem("VRH/Worlds/Describe All WorldInfos")]
        public static void DescribeWorldInfos()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:WorldInfo"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var worldInfo = AssetDatabase.LoadAssetAtPath<WorldInfo>(path);
                Debug.LogFormat("WorldInfo {0} {1} {2}", guid, path, JsonUtility.ToJson(worldInfo));
            }
        }
}
}
