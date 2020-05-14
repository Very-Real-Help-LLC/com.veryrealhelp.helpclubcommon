using UnityEngine;

namespace VeryRealHelp.HelpClubCommon
{

    public static class RenderSettingsFileApplier {

        public const string RenderSettingsAssetName = "assets/render settings.asset";

        public static RenderSettingsFile GetRenderSettingsFromAssetBundle(AssetBundle bundle)
        {
            if (bundle.Contains(RenderSettingsAssetName))
                return bundle.LoadAsset<RenderSettingsFile>(RenderSettingsAssetName);
            else
                return null;
        }
        
        public static void ApplyToActiveScene(RenderSettingsFile file)
        {
            RenderSettings.skybox = file.skybox;
            if (Camera.main != null)
                Camera.main.farClipPlane = file.farClipPlane;

            RenderSettings.ambientMode = file.ambientMode;
            RenderSettings.ambientLight = file.ambientLight;
            RenderSettings.ambientSkyColor = file.ambientSkyColor;
            RenderSettings.ambientEquatorColor = file.ambientEquatorColor;
            RenderSettings.ambientGroundColor = file.ambientGroundColor;
            RenderSettings.ambientIntensity = file.ambientIntensity;

            RenderSettings.fog = file.fog;
            RenderSettings.fogColor = file.fogColor;
            RenderSettings.fogMode = file.fogMode;
            RenderSettings.fogDensity = file.fogDensity;
            RenderSettings.fogStartDistance = file.fogStartDistance;
            RenderSettings.fogEndDistance = file.fogEndDistance;
        }

        public static void UpdateFromActiveScene(RenderSettingsFile file)
        {
            file.skybox = RenderSettings.skybox;
            file.farClipPlane = (Camera.main?.farClipPlane) ?? 1000;

            file.ambientMode = RenderSettings.ambientMode;
            file.ambientLight = RenderSettings.ambientLight;
            file.ambientSkyColor = RenderSettings.ambientSkyColor;
            file.ambientEquatorColor = RenderSettings.ambientEquatorColor;
            file.ambientGroundColor = RenderSettings.ambientGroundColor;
            file.ambientIntensity = RenderSettings.ambientIntensity;

            file.fog = RenderSettings.fog;
            file.fogColor = RenderSettings.fogColor;
            file.fogMode = RenderSettings.fogMode;
            file.fogDensity = RenderSettings.fogDensity;
            file.fogStartDistance = RenderSettings.fogStartDistance;
            file.fogEndDistance = RenderSettings.fogEndDistance;
        }

    }

}
