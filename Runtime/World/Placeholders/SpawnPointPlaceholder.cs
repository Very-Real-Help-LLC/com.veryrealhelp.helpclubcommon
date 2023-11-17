using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VeryRealHelp.HelpClubCommon.World
{
    public class SpawnPointPlaceholder : Placeholder
    {
        [Header("List of Transforms where users can spawn.")]
        public Transform[] spawnPoints;

        public void OnDrawGizmos()
        {
            if (!isActiveAndEnabled) return;

            Gizmos.color = Color.yellow;
            foreach (var spawnPoint in spawnPoints)
            {
                Gizmos.DrawSphere(spawnPoint.position, .1f);
                #if UNITY_EDITOR
                var labelPosition = spawnPoint.position + Vector3.up;
                Handles.Label(labelPosition, "Spawn Point");
                #endif
            }
        }
    }
}
