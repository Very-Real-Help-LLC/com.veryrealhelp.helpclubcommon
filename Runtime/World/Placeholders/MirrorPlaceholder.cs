using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{
    public class MirrorPlaceholder : Placeholder
    {
        private static readonly Vector3 mirrorSize = new Vector3(1.9f, 2.5f, 0.1f);


        public void OnDrawGizmos()
        {
            if (isActiveAndEnabled)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(Vector3.zero + mirrorSize.y * 0.5f * Vector3.up, mirrorSize);
            }
        }
    }
}