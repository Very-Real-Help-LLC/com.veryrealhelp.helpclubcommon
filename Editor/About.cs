using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class About
    {
        public const string documentationUrl = "https://github.com/Very-Real-Help-LLC/com.veryrealhelp.helpclubcommon#readme";

        [MenuItem("VRH/About", priority = 1)]
        public static void AboutMenuItem()
        {
            void Callback(string currentVersion) {
                var action = EditorUtility.DisplayDialogComplex(
                    "Help Club Common",
                    string.Format("Version {0}", currentVersion),
                    "Check for Updates",
                    "Close",
                    "Documentation"
                );
                if (action == 0)
                {
                    SelfUpdater.CheckForUpdatesMenuAction();
                }
                else if (action == 2)
                {
                    Application.OpenURL(documentationUrl);
                }
            }
            EditorCoroutineUtility.StartCoroutineOwnerless(SelfUpdater.GetInstalledVersionCoroutine(Callback));
        }
    }
}
