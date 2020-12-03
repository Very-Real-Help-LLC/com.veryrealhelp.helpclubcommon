using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using VeryRealHelp.HelpClubCommon.Schema;
using VeryRealHelp.HelpClubCommon.World;

namespace VeryRealHelp.HelpClubCommon.Editor.Automation
{
    static class BatchBuild
    {
        private class BuildConfig
        {
            public string buildRoot;
            public string buildName;
            public string version;
            public string uriRoot;
            public HashSet<BuildTarget> targets;

            public BuildConfig(Args args)
            {
                buildRoot = args.Get("buildRoot", "Build");
                buildName = args.Get("buildName", Application.productName);
                version = args.Get("buildVersion", Application.version);
                uriRoot = args.Get("uriRoot", "");
                targets = new HashSet<BuildTarget>();
                if (args.ContainsKey("android"))
                    targets.Add(BuildTarget.Android);
                if (args.ContainsKey("osx"))
                    targets.Add(BuildTarget.StandaloneOSX);
                if (args.ContainsKey("win"))
                    targets.Add(BuildTarget.StandaloneWindows);
                if (targets.Count == 0)  // if no targets specified, target all
                    targets = new HashSet<BuildTarget> { BuildTarget.Android, BuildTarget.StandaloneOSX, BuildTarget.StandaloneWindows };
            }

            public string BuildPath => $"{buildRoot}/{buildName}";

            public string GetBuildPath(BuildTarget target) => $"{BuildPath}/{target}";
            public string GetManifestPath(BuildTarget target) => $"{buildName}/{target}/{target}";
        }

        private class Args : Dictionary<string, string> {
            public string Get(string key, string defaultValue = null)
            {
                TryGetValue(key, out string value);
                if (value == null)
                    return defaultValue;
                else
                    return value;
            }
        }

        private static readonly string EOL = Environment.NewLine;

        private static void ParseCommandLineArguments(out Args providedArguments)
        {
            providedArguments = new Args();
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine(
              $"{EOL}" +
              $"###########################{EOL}" +
              $"#    Parsing settings     #{EOL}" +
              $"###########################{EOL}" +
              $"{EOL}"
            );

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                bool isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                string flag = args[current].TrimStart('-');

                // Parse optional value
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";

                // Assign
                Console.WriteLine($"Found flag \"{flag}\" with value \"{value}\".");
                providedArguments.Add(flag, value);
            }
        }

        private static void Error(string message)
        {
            if (Application.isBatchMode)
            {
                Console.WriteLine(message);
                ExitWithResult(BuildResult.Failed);
            }
            else
                throw new Exception(message);
        }

        [MenuItem("VRH/Automation/Run Build Automation")]
        public static void Build()
        {
            if (Application.isBatchMode)
            {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            }

            ParseCommandLineArguments(out var args);
            BuildConfig config = new BuildConfig(args);

            if (Application.isEditor)
            {
                bool confirmed = EditorUtility.DisplayDialog($"Create build {config.buildName}?", $"This will replace the contents of\n{Path.GetFullPath(config.buildRoot)}", "Continue");
                if (!confirmed)
                {
                    return;
                }
            }

            AssetBundle.UnloadAllAssetBundles(true);

            Debug.Log("Validating Project...");
            WorldDefinition worldDefinition = null;
            var worldInfos = WorldInfoEditor.GetAllWorldInfos().ToList();
            if (worldInfos.Count == 0)
            {
                Debug.Log("No WorldInfo assets found");
            }
            else if (worldInfos.Count > 1)
            {
                Error("More than one WorldInfo assets exists in project. This is a limitation of our current bundle distribution system.");
                return;
            }
            else
            {
                Debug.Log("WORLD INFOS: " + string.Join(" ", worldInfos));
                var worldInfo = worldInfos.First();
                var worldInfoPath = AssetDatabase.GetAssetPath(worldInfo);
                var worldInfoBundle = AssetDatabase.GetImplicitAssetBundleName(worldInfoPath);
                worldDefinition = new WorldDefinition
                {
                    name = Application.productName,
                    version = Application.version,
                    infoAsset = worldInfoPath,
                    infoBundle = worldInfoBundle
                };
                Debug.Log($"SELECTED: {worldInfo} at {worldInfoPath} from {worldInfoBundle}");

                WorldInfoEditor.PrepareAllForBuildSynchronously();
                if (!WorldValidator.ValidateAll())
                {
                    Error("Failed to Validate WorldInfo");
                }
            }

            Debug.Log("Building Project...");
            if (Directory.Exists(config.buildRoot))
            {
                Directory.Delete(config.buildRoot, recursive: true);
            }
            Directory.CreateDirectory(config.buildRoot);
            foreach (var target in config.targets)
            {
                DoBuild(config, target);
            }

            Debug.Log("Building Manifest...");
            var manifestBundlePath = config.buildRoot + "/" + config.GetManifestPath(config.targets.First());
            var manifestBundle = AssetBundle.LoadFromFile(manifestBundlePath);
            var manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            var manifestJson = $"{config.BuildPath}/manifest.json";
            using (StreamWriter file = new StreamWriter(manifestJson))
            {
                var bundleSet = new BundleSetDefinition
                {
                    uriRoot = config.uriRoot,
                    androidPath = config.GetManifestPath(BuildTarget.Android),
                    osxPath = config.GetManifestPath(BuildTarget.StandaloneOSX),
                    windowsPath = config.GetManifestPath(BuildTarget.StandaloneWindows),
                    bundlePaths = manifest.GetAllAssetBundles()
                };
                BundleSetDefinition bundlesForManifest = null;
                if (worldDefinition == null)
                {
                    bundlesForManifest = bundleSet;
                }
                else
                {
                    worldDefinition.bundles = bundleSet;
                }
                file.Write(JsonUtility.ToJson(new ProjectManifest {
                    isAWorldProject = worldDefinition != null,
                    world = worldDefinition,
                    bundles = bundlesForManifest
                }));
            }

            Debug.Log($"Build Completed for {config.buildName} version {config.version} at {config.BuildPath}");
            if (Application.isBatchMode)
                ExitWithResult(BuildResult.Succeeded);
        }

        private static void DoBuild(BuildConfig config, BuildTarget buildTarget)
        {
            string path = config.GetBuildPath(buildTarget);
            Directory.CreateDirectory(path);
            BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle & BuildAssetBundleOptions.StrictMode & BuildAssetBundleOptions.ChunkBasedCompression;
            BuildPipeline.BuildAssetBundles(path, bundleOptions, buildTarget);
        }

        private static void ExitWithResult(BuildResult result)
        {
            if (result == BuildResult.Succeeded)
            {
                Console.WriteLine("Build succeeded!");
                EditorApplication.Exit(0);
            }

            if (result == BuildResult.Failed)
            {
                Console.WriteLine("Build failed!");
                EditorApplication.Exit(101);
            }

            if (result == BuildResult.Cancelled)
            {
                Console.WriteLine("Build cancelled!");
                EditorApplication.Exit(102);
            }

            if (result == BuildResult.Unknown)
            {
                Console.WriteLine("Build result is unknown!");
                EditorApplication.Exit(103);
            }
        }
    }
}