using UnityEditor;
using UnityEngine;

namespace VeryRealHelp.HelpClubCommon.Editor
{

    [CustomEditor(typeof(RenderSettingsFile))]
    public class RenderSettingsFileEditor : UnityEditor.Editor
    {
        public RenderSettingsFile Target => (RenderSettingsFile)serializedObject.targetObject;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(12f);
            if (GUILayout.Button("Apply to Current Scene"))
                RenderSettingsFileApplier.ApplyToActiveScene(Target);

            GUILayout.Space(12f);
            if (GUILayout.Button("Update from Current Scene"))
                RenderSettingsFileApplier.UpdateFromActiveScene(Target);
        }
    }

}
