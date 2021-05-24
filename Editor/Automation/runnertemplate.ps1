echo "Hello from runner_job"
echo "PATH is ${env:PATH}"
echo DIR is
DIR

echo "Starting Unity build"
if($IsMacOS)
{
    $unity="/Applications/Unity/Hub/Editor/2018.4.30f1/Unity.app/Contents/MacOS/Unity"
}
else 
{
    $unity="C:/Program Files/Unity/Hub/Editor/2018.4.20f1/Editor/Unity.exe"
}

&$unity -projectPath . -executeMethod VeryRealHelp.HelpClubCommon.Editor.Automation.BatchBuild.Build -batchmode -nographics -quit -logFile "${env:ACTION_LOG_FILE}" | out-null
echo "Logged to ${env:ACTION_LOG_FILE}"

Get-Content -Path ${env:ACTION_LOG_FILE}

echo "Build completed"
