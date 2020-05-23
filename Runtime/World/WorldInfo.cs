using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.World
{
    [CreateAssetMenu(fileName = "WorldInfo", menuName = "Very Real Help/Worlds/WorldInfo", order = 1)]
    public class WorldInfo : ScriptableObject
    {
        public enum BuildProcess : uint
        {
            Unknown = 0,
            Editor = 1,
            BatchMode = 2,
        }

        [Header("General")]
        public string portalLabel;
        public Texture portalTexture;
        public string sceneBundle;
        public string sceneAssetName;
        public string[] bundleDependencies;

        [Header("Voice")]
        public float voiceProximityRange;

        [HideInInspector]
        public int buildTimestamp;
        [HideInInspector]
        public string worldVersion;
        [HideInInspector]
        public string unityVersion;
        [HideInInspector]
        public string helpClubCommonVersion;
        [HideInInspector]
        public BuildProcess buildProcess;
    }
}
