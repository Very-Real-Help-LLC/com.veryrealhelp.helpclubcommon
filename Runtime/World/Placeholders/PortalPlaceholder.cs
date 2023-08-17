using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{
    public class PortalPlaceholder : Placeholder
    {
        private static readonly Vector3 portalSize = new Vector3(1.9f, 2.5f, 0.1f);

        [Header("Destination (World Id integer)")]
        public string destination;
        [Header("[Obsolete use `destination` instead]")]
        [Obsolete("Use the string `destination` field instead")]
        public WorldId worldId;

        public void OnDrawGizmos()
        {
            if (isActiveAndEnabled)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(Vector3.zero + portalSize.y * 0.5f * Vector3.up, portalSize);
                #if UNITY_EDITOR
                Handles.Label(transform.position + portalSize.y * 1.2f * Vector3.up, "Portal: " + worldId.ToString());
                #endif
            }
        }
    }
}
