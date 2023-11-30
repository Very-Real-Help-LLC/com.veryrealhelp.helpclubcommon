using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VeryRealHelp.HelpClubCommon.World
{
    public class SpawnPointPlaceholder : Placeholder
    {
        // This does not necessarily match the user roles in the client
        public enum UserRole
        {
            Visitor,
            Greeted,
            Member,
            Guide,
        }

        [Serializable]
        public struct SpawnPoint
        {
            public UserRole userRole;
            public Transform spawnLocation;
        }
        
        [Header("Users will spawn where these transforms are located. Spawns can be controlled based on user type.")]
        public SpawnPoint[] spawnPoints;

        public void OnDrawGizmos()
        {
            if (!isActiveAndEnabled) return;

            Gizmos.color = Color.yellow;
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint.spawnLocation == null)
                {
                    continue;
                }
                Gizmos.DrawSphere(spawnPoint.spawnLocation.position, .1f);
                #if UNITY_EDITOR
                var labelPosition = spawnPoint.spawnLocation.position + Vector3.up;
                Handles.Label(labelPosition, $"{spawnPoint.userRole} Spawn Point");
                #endif
            }
        }
    }
}
