﻿using UnityEngine;
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


        [MenuItem("VRH/Placeholders/Prop Placeholder")]
        public static void CreatePropPlaceholder(MenuCommand menuCommand) => CreatePlaceholder<PropPlaceHolder>(menuCommand, "Prop Placeholder");
    }
}
