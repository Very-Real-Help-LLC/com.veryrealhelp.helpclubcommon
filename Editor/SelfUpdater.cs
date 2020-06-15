using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class SelfUpdater
    {
        public const string githubApiRoot = "https://api.github.com";
        public const string repositoryUser = "Very-Real-Help-LLC";
        public const string repositoryName = "com.veryrealhelp.helpclubcommon";
        public const string packageName = "com.veryrealhelp.helpclubcommon";

        [MenuItem("VRH/Check for Updates")]
        public static void CheckForUpdatesMenuAction()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(CheckForUpdatesMenuActionCoroutine());
        }

        public static IEnumerator CheckForUpdatesMenuActionCoroutine()
        {
            GithubReleaseInfo release = null;
            string currentVersion = null;
            yield return GetLatestReleaseInfo(info => release = info);
            yield return GetInstalledVersionCoroutine(version => currentVersion = version);
            if (release.tag_name == currentVersion)
            {
                EditorUtility.DisplayDialog("Help Club Common", string.Format("You are using the latest version of Help Club Common ({0})", currentVersion), "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Help Club Common", string.Format("There is an update available.\nLatest: {0}\nInstalled: {1}", currentVersion, release.tag_name), "OK");
            }
        }


        public static IEnumerator GetInstalledVersionCoroutine(Action<string> callback)
        {
            var request = Client.List(true);
            while (!request.IsCompleted)
                yield return null;
            foreach (var package in request.Result)
            {
                if (package.name == packageName)
                {
                    callback.Invoke(package.version);
                }
            }
        }


        public class GithubReleaseInfo
        {
            public string tag_name;
        }

        public static IEnumerator GetLatestReleaseInfo(Action<GithubReleaseInfo> callback = null)
        {
            string url = string.Format("{0}/repos/{1}/{2}/releases/latest", githubApiRoot, repositoryUser, repositoryName);
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("accept", "application/vnd.github.v3+json");
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                Debug.LogWarningFormat("Request failed: {0} {1}", request.url, request.error);
            else
            {
                var release = JsonUtility.FromJson<GithubReleaseInfo>(request.downloadHandler.text);
                callback?.Invoke(release);
            }
        }
       
    }
}
