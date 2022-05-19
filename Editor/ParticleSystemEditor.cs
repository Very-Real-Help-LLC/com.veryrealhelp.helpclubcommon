using UnityEditor;
using UnityEngine;
using VeryRealHelp.HelpClubCommon.Particles;
using VeryRealHelp.HelpClubCommon.World;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    [CustomEditor(typeof(ParticleSystemPlaceholder))]
    public class ParticleSystemEditor : UnityEditor.Editor
    {
        SerializedProperty particleType;
        SerializedProperty particlePrefab;
        SerializedProperty textMessages;

        private void OnEnable()
        {
            particleType = serializedObject.FindProperty("particleType");
            particlePrefab = serializedObject.FindProperty("particlePrefab");
            textMessages = serializedObject.FindProperty("textMessages");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            ParticleSystemPlaceholder particleSystemPlaceholder = (ParticleSystemPlaceholder)target;

            EditorGUILayout.LabelField("Generic Particle Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(particleType);
            if (particleSystemPlaceholder.particleType != ParticleSystemDefinition.ParticleType.None)
            {
                if (particleSystemPlaceholder.particleType == ParticleSystemDefinition.ParticleType.Prefab)
                {
                    EditorGUILayout.PropertyField(particlePrefab);
                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                particleSystemPlaceholder.particleLifetime = EditorGUILayout.FloatField("Lifetime", particleSystemPlaceholder.particleLifetime);
                particleSystemPlaceholder.spawnAreaSize = EditorGUILayout.Vector3Field("Particle Spawn Area", particleSystemPlaceholder.spawnAreaSize);
                EditorGUILayout.LabelField("Particle Scale", EditorStyles.boldLabel);
                EditorGUILayout.MinMaxSlider(ref particleSystemPlaceholder.minScale, ref particleSystemPlaceholder.maxScale, 0f, 5f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"Min Scale: {particleSystemPlaceholder.minScale.ToString("0.00")}");
                GUILayout.Label($"Max Scale: {particleSystemPlaceholder.maxScale.ToString("0.00")}");
                EditorGUILayout.EndHorizontal();

                switch (particleSystemPlaceholder.particleType)
                {
                    case Particles.ParticleSystemDefinition.ParticleType.Text:
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                        EditorGUILayout.LabelField("Text Settings", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(textMessages);
                        particleSystemPlaceholder.textFadeDuration = EditorGUILayout.FloatField("Text Fade Duration", particleSystemPlaceholder.textFadeDuration);
                        particleSystemPlaceholder.textScrollingOffset = EditorGUILayout.Vector3Field("Text Scroll Offset", particleSystemPlaceholder.textScrollingOffset);
                        break;
                    case Particles.ParticleSystemDefinition.ParticleType.Prefab:
                        //Extend by future prefab spawn settings
                        break;
                    case Particles.ParticleSystemDefinition.ParticleType.None:
                    default:
                        break;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
