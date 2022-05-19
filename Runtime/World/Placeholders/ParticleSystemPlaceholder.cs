using UnityEngine;
using VeryRealHelp.HelpClubCommon.Particles;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{

    public class ParticleSystemPlaceholder : Placeholder
    {
        //generic
        public ParticleSystemDefinition.ParticleType particleType;
        public GameObject particlePrefab;

        //ps settings
        public float particleLifetime = 2f;
        public float minScale = 0.5f;
        public float maxScale = 1.5f;
        public Vector3 spawnAreaSize = new Vector3(3f, 1.55f, 2f);

        //text related
        public string[] textMessages;
        public float textFadeDuration = 2f;
        public Vector3 textScrollingOffset = new Vector3(0f, 0.1f, 0.15f);

        public void OnDrawGizmos()
        {
            if (isActiveAndEnabled)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Color transparentAreaColor = Color.blue;
                transparentAreaColor.a = 0.2f;
                Gizmos.color = transparentAreaColor;
                Gizmos.DrawCube(Vector3.zero, spawnAreaSize);
            }
        }
    }
}
