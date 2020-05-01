using System;
using UnityEditor;
using UnityEngine;
using VeryRealHelp.HelpClubCommon.Actions;

namespace VeryRealHelp.HelpClubCommon.Editor {
    [CustomPropertyDrawer(typeof(ActionType))]
    public class ActionTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            EditorGUI.BeginProperty(position, label, property);
            Enum newEnum = EditorGUI.EnumFlagsField(position, label, (ActionType)property.intValue);
            property.intValue = (int)Convert.ChangeType(newEnum, typeof(ActionType));
            EditorGUI.EndProperty();
        }
    }
}
