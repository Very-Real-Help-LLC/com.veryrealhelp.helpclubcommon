using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VeryRealHelp.HelpClubCommon.World
{
    public class SpawnPlaceholder : Placeholder
    {
        [Serializable]
        public struct SpawnPoint
        {
            [Tooltip("Examples of roles: visitor, member, greeter, guide, guardian, admin. These are subject to change.")]
            public string minimumUserRole;
            public Transform spawnLocation;
        }
        
        [Header("Users will spawn where these transforms are located. Spawns can be controlled based on user type.")]
        public SpawnPoint[] spawnPoints;

        public void OnDrawGizmos()
        {
            if (!isActiveAndEnabled) return;

            Gizmos.color = Color.yellow;
            if(spawnPoints == null) return;
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint.spawnLocation == null)
                {
                    continue;
                }
                Gizmos.DrawSphere(spawnPoint.spawnLocation.position, .1f);
                #if UNITY_EDITOR
                var labelPosition = spawnPoint.spawnLocation.position + Vector3.up;
                Handles.Label(labelPosition, $"{spawnPoint.minimumUserRole} Spawn Point");
                #endif
            }
        }
    }
}
