using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{
    [ExecuteInEditMode]
    public class PlaybackStationPlaceholder : Placeholder
    {
        private static readonly Vector3 mirrorSize = new Vector3(2.5f, 0.15f, 2.5f);

        public string playlistId;
        public string[] recordingIds;
        public string category;

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (isActiveAndEnabled)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Color col = Color.cyan;
                col.a = 0.5f;
                Gizmos.color = col;
                Gizmos.DrawCube(Vector3.zero + mirrorSize.y * 0.5f * Vector3.up, mirrorSize);

                Handles.Label(transform.position + mirrorSize.y * 1.1f * Vector3.up, "PlaybackStation");
            }
        }
#endif
    }
}