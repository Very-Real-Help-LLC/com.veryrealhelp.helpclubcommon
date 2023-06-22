echo "Hello from runner_job"
echo "PATH is ${env:PATH}"
echo DIR is
DIR

echo "Starting Unity build"
if($IsMacOS)
{
    $unity="/Applications/Unity/Hub/Editor/2020.3.47f1/Unity.app/Contents/MacOS/Unity"
}
else 
{
    $unity="C:/Program Files/Unity/Hub/Editor/2020.3.47f1/Editor/Unity.exe"
}

&$unity -projectPath . -executeMethod VeryRealHelp.HelpClubCommon.Editor.Automation.BatchBuild.Build -batchmode -nographics -quit -logFile | Out-Host

echo "Build completed"
