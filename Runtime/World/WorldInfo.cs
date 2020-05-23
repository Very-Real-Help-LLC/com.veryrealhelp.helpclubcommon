#if UNITY_EDITOR
using UnityEditor;
#endif
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
        #if UNITY_EDITOR
        public SceneAsset sceneAsset;
        #endif
        public string portalLabel;
        public Texture portalTexture;

        [Header("Voice")]
        public float voiceProximityRange;

        [HideInInspector]
        public string sceneBundle;
        [HideInInspector]
        public string sceneAssetName;
        [HideInInspector]
        public string[] bundleDependencies;
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
