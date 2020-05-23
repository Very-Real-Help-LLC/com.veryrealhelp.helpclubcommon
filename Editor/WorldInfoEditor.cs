using System.Collections;
using UnityEngine;
using UnityEditor;
using VeryRealHelp.HelpClubCommon.World;
using Unity.EditorCoroutines.Editor;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class WorldInfoEditor
    {
        public static void PrepareAllForBuildSynchronously()
        {
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

        [MenuItem("Test/PrepareAllForBuild")]
        public static EditorCoroutine PrepareAllForBuild()
        {
            return EditorCoroutineUtility.StartCoroutineOwnerless(PrepareAllForBuildCoroutine());
        }

        private static IEnumerator PrepareAllForBuildCoroutine()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:WorldInfo"))
                yield return PrepareForBuildCoroutine(AssetDatabase.LoadAssetAtPath<WorldInfo>(AssetDatabase.GUIDToAssetPath(guid)));
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
        }

        private static WorldInfo.BuildProcess GetCurrentBuildProcess()
        {
            if (Application.isBatchMode)
                return WorldInfo.BuildProcess.BatchMode;
            if (Application.isEditor)
                return WorldInfo.BuildProcess.Editor;
            return WorldInfo.BuildProcess.Unknown;
        }

        [MenuItem("Test/Describe WorldInfos")]
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
