using UnityEngine;

namespace VeryRealHelp.HelpClubCommon
{

    public static class RenderSettingsFileApplier {

        public static void ApplyToActiveScene(RenderSettingsFile file)
        {
            var settings = file != null ? file : RenderSettingsFile.Defaults;
            RenderSettings.skybox = settings.skybox;

            RenderSettings.ambientMode = settings.ambientMode;
            RenderSettings.ambientLight = settings.ambientLight;
            RenderSettings.ambientSkyColor = settings.ambientSkyColor;
            RenderSettings.ambientEquatorColor = settings.ambientEquatorColor;
            RenderSettings.ambientGroundColor = settings.ambientGroundColor;
            RenderSettings.ambientIntensity = settings.ambientIntensity;

            RenderSettings.fog = settings.fog;
            RenderSettings.fogColor = settings.fogColor;
            RenderSettings.fogMode = settings.fogMode;
            RenderSettings.fogDensity = settings.fogDensity;
            RenderSettings.fogStartDistance = settings.fogStartDistance;
            RenderSettings.fogEndDistance = settings.fogEndDistance;
        }

        public static void UpdateFromActiveScene(RenderSettingsFile file)
        {
            file.skybox = RenderSettings.skybox;

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

        public static void ApplyToCamera(RenderSettingsFile file, Camera camera)
        {
            var settings = file != null ? file : RenderSettingsFile.Defaults;
            camera.farClipPlane = settings.farClipPlane;
        }

        public static void UpdateFromCamera(RenderSettingsFile file, Camera camera)
        {
            file.farClipPlane = camera.farClipPlane;
        }

    }

}
