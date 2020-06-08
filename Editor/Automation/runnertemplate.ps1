echo "Hello from runner_job"
echo "PATH is ${env:PATH}"
echo DIR is
DIR

echo "Starting Unity build"
$unity="C:/Program Files/Unity/Hub/Editor/2018.4.20f1/Editor/Unity.exe"
&$unity -projectPath . -buildTarget Win -executeMethod VeryRealHelp.HelpClubCommon.Editor.Automation.BatchBuild.Build -batchmode -nographics -quit -logFile U3D.log | out-null
echo "Build completed"
