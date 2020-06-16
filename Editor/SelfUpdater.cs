using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
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
        public const string packageUrl = "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon.git";

        [MenuItem("VRH/Check for Updates", priority = 1)]
        public static void CheckForUpdatesMenuAction()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(CheckForUpdatesMenuActionCoroutine());
        }

        public static IEnumerator CheckForUpdatesMenuActionCoroutine()
        {
            string latestTag = null;
            string installedTag = null;
            yield return GetLatestReleaseInfo(info => latestTag = info.tag_name);
            yield return GetInstalledVersionCoroutine(version => installedTag = version);
            if (latestTag == installedTag)
            {
                EditorUtility.DisplayDialog("Help Club Common", string.Format("You are using the latest version of Help Club Common ({0})", installedTag), "OK");
            }
            else
            {
                if (EditorUtility.DisplayDialog(
                    "Help Club Common",
                    string.Format("There is an update available.\nLatest: {0}\nInstalled: {1}", latestTag, installedTag),
                    "Update",
                    "Not Now"
                ))
                {
                    UpdateToVersion(latestTag);
                }
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
        

        public static void UpdateToVersion(string newTag)
        {
            const string manifestPath = "Packages/manifest.json";
            Regex pattern = new Regex(string.Format(@"(\s*""{0}""\s*:\s*)(""[^""]*"",?\s*)", packageName));

            var lines = File.ReadAllLines(manifestPath);
            List<string> outLines = new List<string>();

            foreach (var line in lines)
            {
                var match = pattern.Match(line);
                var outLine = line;
                if (match.Success)
                    outLine = string.Format(@"{0}""{1}#{2}"",", match.Groups[1], packageUrl, newTag);
                outLines.Add(outLine);
            }

            var output = string.Join("\n", outLines);
            File.WriteAllText(manifestPath, output);

            AssetDatabase.Refresh();
        }
    }
}
