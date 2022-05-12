using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{
    public class MirrorPlaceholder : Placeholder
    {
        private static readonly Vector3 mirrorSize = new Vector3(1.28f, 2.56f, 0.1f);
        private static readonly Vector3 mirrorArea = new Vector3(2.56f, 2.56f, 3f);

        public bool visualizeAffirmations;

        public void OnDrawGizmos()
        {
            if (isActiveAndEnabled)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(Vector3.zero + mirrorSize.y * 0.5f * Vector3.up, mirrorSize);
                Color transparentAreaColor = Color.magenta;
                transparentAreaColor.a = 0.2f;
                Gizmos.color = transparentAreaColor;
                Gizmos.DrawCube(new Vector3(0f, mirrorArea.y * 0.5f, mirrorArea.z * 0.5f), mirrorArea);

#if UNITY_EDITOR
                Handles.Label(transform.position + mirrorSize.y * 1.1f * Vector3.up, "Mirror");
#endif
            }
        }
    }
}