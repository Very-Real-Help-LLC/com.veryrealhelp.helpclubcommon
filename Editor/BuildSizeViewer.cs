// This script was originally created by MunifiSense to analyze VRC asset bundles.
// View the original source here https://github.com/MunifiSense/VRChat-Build-Size-Viewer
// The original script is MIT licensed, so it's safe for us to use and distribute.
// I have heavily modified it to support multi-bundle loading, and tailored it around our specific asset bundle build automation
// Unity provides more information about asset bundles in the hidden editor log when building, than it exposes via apis. 
// This script scrapes those logs and presents the data in a relevant way.
// I've also added in support for diffing scene and stuff bundles, in order to find memory duplication

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class BuildSizeViewer : EditorWindow {
        
        private readonly string _buildLogPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Unity/Editor/Editor.log";
        private readonly char[] _delimiterChars = { ' ', '\t' };
        private int _buildTargetIndex;
        private readonly string[] _buildTargets = { "Android", "StandaloneOSX", "StandaloneWindows", "iOS" };
        
        private class BuildObject {
            public string Size;
            public string Percent;
            public string Path;
            public bool Duplicate;
        }

        private class BundleInfo
        {
            public bool BuildLogFound;
            public string BundleName;
            
            public List<BuildObject> BuildObjectList;
            public List<string> UncompressedList;
            
            public string TotalSize;
            public Vector2 ScrollPos;    
        }
        private readonly BundleInfo _sceneBundleInfo = new BundleInfo();
        private readonly BundleInfo _assetsBundleInfo = new BundleInfo();
        
        [MenuItem("VRH/Automation/Asset Bundle Build Size Viewer")]

        public static void ShowWindow() {
            GetWindow(typeof(BuildSizeViewer));
        }

        private void OnGUI()
        {
            // Define the bundle names using Application.productName as a prefix
            string assetBundlePrefix = Application.productName.ToLower().Replace(" ", "_");
            
            _sceneBundleInfo.BundleName = $"{assetBundlePrefix}_scene";
            _assetsBundleInfo.BundleName = $"{assetBundlePrefix}_stuff";
            
            EditorGUILayout.LabelField("Asset Bundle Build Size Viewer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Run the build automation to generate bundles, then click the button!", EditorStyles.label);
            _buildTargetIndex = EditorGUILayout.Popup("Build Target", _buildTargetIndex, _buildTargets);
            if (GUILayout.Button("Read Build Log")) {
                _sceneBundleInfo.BuildLogFound = GetBuildSize(_sceneBundleInfo);
                _assetsBundleInfo.BuildLogFound = GetBuildSize(_assetsBundleInfo);
                if (_sceneBundleInfo.BuildLogFound && _assetsBundleInfo.BuildLogFound)
                {
                    DiffBundles();
                }
            }
            if (_sceneBundleInfo.BuildLogFound && _assetsBundleInfo.BuildLogFound)
            {
                EditorGUILayout.BeginHorizontal();
                DrawBuildLog(_sceneBundleInfo);
                DrawBuildLog(_assetsBundleInfo);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Couldn't find bundle build logs");
            }
        }

        private void DiffBundles()
        {
            foreach (var buildObject1 in _sceneBundleInfo.BuildObjectList)
            {
                foreach (var buildObject2 in _assetsBundleInfo.BuildObjectList.Where(buildObject2 => buildObject1.Path == buildObject2.Path))
                {
                    buildObject1.Duplicate = true;
                    buildObject2.Duplicate = true;
                }
            }
        }

        private void DrawBuildLog(BundleInfo bundleInfo)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(bundleInfo.BundleName);
            if (bundleInfo.UncompressedList != null && bundleInfo.UncompressedList.Count != 0) {
                EditorGUILayout.LabelField("Total Compressed Build Size: " + bundleInfo.TotalSize);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.EndHorizontal();
                foreach (var entry in bundleInfo.UncompressedList) {
                    EditorGUILayout.LabelField(entry);
                }
            }

            if (bundleInfo.BuildObjectList == null || bundleInfo.BuildObjectList.Count == 0)
            {
                EditorGUILayout.LabelField("No Build Objects Found");
                return;
            }
        
            var windowWidth = (float)(position.width * 0.6);
            var percentLabelWidth = (float)(windowWidth * 0.15);
            var sizeLabelWidth = (float)(windowWidth * 0.15);
            var buttonLabelWidth = (float)(windowWidth * 0.15);
            var pathLabelWidth = (float)(windowWidth * 0.35);

            var warningStyle = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    textColor = Color.yellow
                }
            };

            bundleInfo.ScrollPos = EditorGUILayout.BeginScrollView(bundleInfo.ScrollPos);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Separator();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Size%", GUILayout.Width(percentLabelWidth));
            EditorGUILayout.LabelField("Size", GUILayout.Width(sizeLabelWidth));
            EditorGUILayout.LabelField("Path", GUILayout.Width(pathLabelWidth));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Separator();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Used Assets and files from the Resources folder, sorted by uncompressed size:");
            foreach (var buildObject in bundleInfo.BuildObjectList) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(buildObject.Percent, GUILayout.Width(percentLabelWidth));
                EditorGUILayout.LabelField(buildObject.Size, GUILayout.Width(sizeLabelWidth));
                if(buildObject.Duplicate)
                    EditorGUILayout.LabelField(buildObject.Path, warningStyle);
                else
                    EditorGUILayout.LabelField(buildObject.Path);
                
                if(buildObject.Path != "Resources/unity_builtin_extra") {
                    if (GUILayout.Button("Go", GUILayout.Width(buttonLabelWidth))) {
                        var obj = AssetDatabase.LoadAssetAtPath(buildObject.Path, typeof(Object));
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private bool GetBuildSize(BundleInfo bundleInfo)
        {
            var buildLogCopyPath = $"{_buildLogPath}copy";
            Debug.Log(buildLogCopyPath);
        
            // Make a copy of the editor log file
            FileUtil.ReplaceFile(_buildLogPath, buildLogCopyPath);
        
            //Read the text from log
            var reader = new StreamReader(buildLogCopyPath);
        
            var foundBuildTarget = TryReadUntilLineContains(ref reader, $"Building {_buildTargets[_buildTargetIndex]}...", out _);
            var foundBundleName = TryReadUntilLineContains(ref reader, $"Bundle Name: {bundleInfo.BundleName}", out _);
        
            // Read the build info 
            if (foundBuildTarget && foundBundleName)
            {
                bundleInfo.BuildObjectList = new List<BuildObject>();
                bundleInfo.UncompressedList = new List<string>();

                if (TryReadUntilLineContains(ref reader, "Compressed Size:", out var totalSizeLine))
                {
                    bundleInfo.TotalSize = totalSizeLine.Split(':')[1];
                }
                
                // Read through the list of assets
                ForeachLineUntilLineContains(ref reader,
                    "Used Assets and files from the Resources folder, sorted by uncompressed size:", 
                    line => { bundleInfo.UncompressedList.Add(line); }
                );

                // Read through the build objects
                ForeachLineUntilLineContains(ref reader,
                    "-------------------------------------------------------------------------------",
                    line =>
                    {
                        var splitLine = line.Split(_delimiterChars);
                        var temp = new BuildObject
                        {
                            Size = splitLine[1] + splitLine[2],
                            Percent = splitLine[4],
                            Path = splitLine[5]
                        };
                        for (var i = 6; i < splitLine.Length; i++)
                        {
                            temp.Path += $" {splitLine[i]}";
                        }

                        bundleInfo.BuildObjectList.Add(temp);
                    });
            }

            // Cleanup
            FileUtil.DeleteFileOrDirectory(buildLogCopyPath);
            reader.Close();
        
            return foundBuildTarget && foundBundleName;
        }
    
        private static bool TryReadUntilLineContains(ref StreamReader reader, string targetLine, out string line)
        {
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains(targetLine)) return true;
            }
            return false;
        }

        private static void ForeachLineUntilLineContains(ref StreamReader reader, string targetLine, Action<string> action)
        {
            while (reader.ReadLine() is { } line)
            {
                if(line.Contains(targetLine)) return;
                action.Invoke(line);
            }
        }
    }
}
#endif
