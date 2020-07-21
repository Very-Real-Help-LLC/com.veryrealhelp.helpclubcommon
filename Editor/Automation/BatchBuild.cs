using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Editor.Automation
{
    static class BatchBuild
    {
        private static string EOL = Environment.NewLine;

        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
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

        private static Dictionary<string, string> GetValidatedOptions()
        {
            ParseCommandLineArguments(out var validatedOptions);

            if (!validatedOptions.TryGetValue("projectPath", out var projectPath))
            {
                Console.WriteLine("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }

            if (!validatedOptions.TryGetValue("buildTarget", out var buildTarget))
            {
                Console.WriteLine("Missing argument -buildTarget");
                EditorApplication.Exit(120);
            }

            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget))
            {
                EditorApplication.Exit(121);
            }

            if (!validatedOptions.TryGetValue("customBuildPath", out var customBuildPath))
            {
                Console.WriteLine("Missing argument -customBuildPath");
                EditorApplication.Exit(130);
            }

            string defaultCustomBuildName = "TestBuild";
            if (!validatedOptions.TryGetValue("customBuildName", out var customBuildName))
            {
                Console.WriteLine($"Missing argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }
            else if (customBuildName == "")
            {
                Console.WriteLine($"Invalid argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }

            return validatedOptions;
        }


        [MenuItem("Test/BuildProject")]
        public static void Build()
        {
            if (Application.isBatchMode)
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log("Building Project...");
            WorldInfoEditor.PrepareAllForBuildSynchronously();
            if (!WorldValidator.ValidateAll())
                if (Application.isBatchMode)
                    ExitWithResult(BuildResult.Failed);
                else
                    throw new Exception("Validation Failed");


            ParseCommandLineArguments(out var validatedOptions);

            //Build for all if no specific tags provided
            if(!validatedOptions.ContainsKey("android") && !validatedOptions.ContainsKey("win") && !validatedOptions.ContainsKey("osx"))
            {
                DoBuild(BuildTarget.Android);
                DoBuild(BuildTarget.StandaloneWindows);
                DoBuild(BuildTarget.StandaloneOSX);
            }
            else
            {
                if(validatedOptions.ContainsKey("android"))
                {
                    DoBuild(BuildTarget.Android);
                }
                if (validatedOptions.ContainsKey("win"))
                {
                    DoBuild(BuildTarget.StandaloneWindows);
                }
                if (validatedOptions.ContainsKey("osx"))
                {
                    DoBuild(BuildTarget.StandaloneOSX);
                }
            }

            if (Application.isBatchMode)
                ExitWithResult(BuildResult.Succeeded);
        }


        public static void DoBuild(BuildTarget buildTarg)
        {

            string directory = "BuildTest/" + buildTarg.ToString();
            //string directory = "AssetBundles/" + buildOptions.target.ToString();
            Directory.CreateDirectory(directory);
            BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle & BuildAssetBundleOptions.StrictMode & BuildAssetBundleOptions.ChunkBasedCompression;
            BuildPipeline.BuildAssetBundles(directory, bundleOptions, buildTarg);

        }

        [MenuItem("Test/BuildProjectOld")]
        public static void BuildProjectOld()
        {
            // Gather values from args
            var options = GetValidatedOptions();

            /*
            var options = new Dictionary<string, string>();
            //options.Add("projectPath", "C:/Users/Callum/Desktop/unity-builder/action/default-build-script");
            options.Add("buildTarget", "StandaloneWindows");
            //options.Add("customBuildPath", "C:/Users/Callum/Desktop/unity-builder/action/default-build-script/StandaloneWindows/StandaloneWindows.exe");
            //options.Add("customBuildName", "TestBuild");
            */

            // Gather values from project
            //var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();

            // Define BuildPlayer Options
            var buildOptions = new BuildPlayerOptions
            {
                //scenes = scenes,
                //locationPathName = options["customBuildPath"],
                target = (BuildTarget)Enum.Parse(typeof(BuildTarget), options["buildTarget"]),
            };

            //string directory = "AssetBundles/StandaloneWindows";

            string directory = "AssetBundles/" + buildOptions.target.ToString();
            Directory.CreateDirectory(directory);

            BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle & BuildAssetBundleOptions.StrictMode & BuildAssetBundleOptions.ChunkBasedCompression;

            //Perform bundle build
            BuildPipeline.BuildAssetBundles(directory, bundleOptions, BuildTarget.StandaloneWindows);

            /*
            // Perform build
            BuildReport buildReport = BuildPipeline.BuildPlayer(buildOptions);

            // Summary
            BuildSummary summary = buildReport.summary;
            ReportSummary(summary);

            // Result
            BuildResult result = summary.result;
            */

            ExitWithResult(BuildResult.Succeeded);
            //ExitWithResult(result);
        }

        private static void ReportSummary(BuildSummary summary)
        {
            Console.WriteLine(
              $"{EOL}" +
              $"###########################{EOL}" +
              $"#      Build results!      #{EOL}" +
              $"###########################{EOL}" +
              $"{EOL}" +
              $"Duration: {summary.totalTime.ToString()}{EOL}" +
              $"Warnings: {summary.totalWarnings.ToString()}{EOL}" +
              $"Errors: {summary.totalErrors.ToString()}{EOL}" +
              $"Size: {summary.totalSize.ToString()} bytes{EOL}" +
              $"{EOL}"
            );
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