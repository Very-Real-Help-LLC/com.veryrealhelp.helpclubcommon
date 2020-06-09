using UnityEngine;
using UnityEditor;
using System.IO;

public static class AutomationSetup
{
    private const string githubWorkflowTemplatePath = "Packages/com.veryrealhelp.helpclubcommon/Editor/Automation/githubworkflow.yml";
    private const string githubWorkflowActualPath = ".github/workflows/main.yml";
    private const string runnerTemplatePath = "Packages/com.veryrealhelp.helpclubcommon/Editor/Automation/runnertemplate.ps1";
    private const string runnerActualPath = "runner_job.ps1";

    [MenuItem("VRH/Automation/Update Project Automation Settings")]
    public static void SetupMenuAction()
    {
        if (GetIsSetupCorrectly())
            Debug.Log("Automation Is Setup Correctly");
        else
        {
            Debug.Log("Setting Up Automation...");
            Setup();
            if (GetIsSetupCorrectly())
                Debug.Log("Automation Setup Complete");
            else
                Debug.Log("Automation Setup Failed");
        }
    }

    private static bool GetIsSetupCorrectly()
    {
        return GetGithubWorkflowIsSetupCorrectly() && GetRunnerIsSetupCorrectly();
    }

    private static void Setup()
    {
        SetupGithubWorkflow();
        SetupRunner();
    }


    private static bool GetGithubWorkflowIsSetupCorrectly()
    {
        return GetFilesAreEqual(githubWorkflowTemplatePath, githubWorkflowActualPath);
    }

    private static void SetupGithubWorkflow()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(githubWorkflowActualPath));
        File.Copy(githubWorkflowTemplatePath, githubWorkflowActualPath, true);
    }

    private static bool GetRunnerIsSetupCorrectly()
    {
        return GetFilesAreEqual(runnerTemplatePath, runnerActualPath);
    }

    private static void SetupRunner()
    {
        File.Copy(runnerTemplatePath, runnerActualPath, true);
    }


    private static bool GetFilesAreEqual(string templatePath, string actualPath)
    {
        if (!File.Exists(actualPath))
            return false;
        Debug.LogFormat("comparing template: {0} with actual: {1}", templatePath, actualPath);
        using (var actual = new FileStream(actualPath, FileMode.Open))
        using (var template = new FileStream(templatePath, FileMode.Open))
        {
            if (actual.Length != template.Length)
            {
                Debug.LogFormat("Lengths do not match: {0} : {1} & {2} : {3}", template.Length, templatePath, actual.Length, actualPath);
                return false;
            }
            else
            {
                int actualByte, templateByte;
                do
                {
                    actualByte = actual.ReadByte();
                    templateByte = template.ReadByte();
                }
                while ((actualByte == templateByte) && (actualByte != -1));
                if (actualByte != templateByte)
                    Debug.LogFormat("Discrepancy {0} : {1}", actualByte, templateByte);
                return actualByte == templateByte;
            }
        }
    }
}
