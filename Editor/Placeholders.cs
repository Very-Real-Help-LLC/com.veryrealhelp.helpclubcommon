using UnityEngine;
using UnityEditor;
using VeryRealHelp.HelpClubCommon.World;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class Placeholders
    {
        public static void CreatePlaceholder<T>(MenuCommand menuCommand, string name)
        {
            GameObject obj = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
            obj.AddComponent(typeof(T));
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeObject = obj;
        }

        [MenuItem("VRH/Placeholders/Portal Placeholder")]
        public static void CreatePortalPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<PortalPlaceholder>(menuCommand, "Portal Placeholder");

        [MenuItem("VRH/Placeholders/Generic Portal Placeholder")]
        public static void CreateGenericPortalPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<GenericPortalPlaceHolder>(menuCommand, "Generic Portal Placeholder");

        [MenuItem("VRH/Placeholders/Mirror Placeholder")]
        public static void CreateMirrorPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<MirrorPlaceholder>(menuCommand, "Mirror Placeholder");

        [MenuItem("VRH/Placeholders/Prop Placeholder")]
        public static void CreatePropPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<PropPlaceHolder>(menuCommand, "Prop Placeholder");

        [MenuItem("VRH/Placeholders/Particle System Placeholder")]
        public static void CreateParticleSystemPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<ParticleSystemPlaceholder>(menuCommand, "Particle System Placeholder");

        [MenuItem("VRH/Placeholders/PlaybackStation Placeholder")]
        public static void CreatePlaybackStationPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<PlaybackStationPlaceholder>(menuCommand, "PlaybackStation Placeholder");
        
        [MenuItem("VRH/Placeholders/SpawnPoint Placeholder")]
        public static void CreateSpawnPointPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<SpawnPlaceholder>(menuCommand, "SpawnPoint Placeholder");

        [MenuItem("VRH/Placeholders/Bulletin Board Placeholder")]
        public static void CreateBulletinboardPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<BulletinboardPlaceholder>(menuCommand, "Bulletin Board Placeholder");

    }
}
