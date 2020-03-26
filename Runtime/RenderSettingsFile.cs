using UnityEngine;
using UnityEngine.Rendering;

namespace VeryRealHelp.HelpClubCommon
{

    [CreateAssetMenu(fileName = "Render Settings", menuName = "Very Real Help/Worlds/Render Settings File", order = 1)]
    public class RenderSettingsFile : ScriptableObject
    {
        public Material skybox;

        public AmbientMode ambientMode;
        [ColorUsage(true, true)]
        public Color ambientLight;
        [ColorUsage(true, true)]
        public Color ambientSkyColor;
        [ColorUsage(true, true)]
        public Color ambientEquatorColor;
        [ColorUsage(true, true)]
        public Color ambientGroundColor;
        [Range(0f, 8f)]
        public float ambientIntensity;

        public bool fog;
        [ColorUsage(true, true)]
        public Color fogColor;
        public FogMode fogMode;
        public float fogDensity;
        public float fogStartDistance;
        public float fogEndDistance;
    }

}
