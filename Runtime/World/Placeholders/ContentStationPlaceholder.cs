using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VeryRealHelp.HelpClubCommon.World
{
public class ContentStationPlaceholder : Placeholder
{
        public string webUiPath;

        public bool allowPlayerSpawnPosition = false;

#region ContentStation Positions
        [Header("Content Station Positions")]
        public Transform recordingPosition;
        public Transform aiAgentPosition;
        public Transform[] aiAgentToolSpawnPoints;
        public Transform portalPosition;
        public Transform toolPosition;
        public Transform playerSpawnPosition;
#endregion
        public Canvas contentStationCanvas;
        public Button button;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Automatically create missing transforms as children
            if (recordingPosition == null)
            {
                recordingPosition = CreateChildTransform("RecordingPosition");
            }
            
            if (aiAgentPosition == null)
            {
                aiAgentPosition = CreateChildTransform("AIAgentPosition");
            }
            
            if (aiAgentToolSpawnPoints == null || aiAgentToolSpawnPoints.Length == 0)
            {
                aiAgentToolSpawnPoints = new Transform[3];
                for (int i = 0; i < aiAgentToolSpawnPoints.Length; i++)
                {
                    aiAgentToolSpawnPoints[i] = CreateChildTransform($"AIAgentToolSpawnPoint_{i}");
                }
            }
            
            if (portalPosition == null)
            {
                portalPosition = CreateChildTransform("PortalPosition");
            }
            
            if (toolPosition == null)
            {
                toolPosition = CreateChildTransform("ToolPosition");
            }

            if (playerSpawnPosition == null)
            {
                playerSpawnPosition = CreateChildTransform("PlayerSpawnPosition");
            }
        }

        private Transform CreateChildTransform(string name)
        {
            // Check if child already exists
            Transform existing = transform.Find(name);
            if (existing != null)
            {
                return existing;
            }
            
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child.transform;
        }

        private void DrawAllPositions()
        {
            // Draw recording position
            if (recordingPosition != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(recordingPosition.position, 0.05f);
                Handles.Label(recordingPosition.position + Vector3.up * 0.2f, "Recording");
            }
            
            // Draw AI agent position
            if (aiAgentPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(aiAgentPosition.position, 0.05f);
                Handles.Label(aiAgentPosition.position + Vector3.up * 0.2f, "AI Agent");
            }
            
            // Draw AI agent tool spawn points
            if (aiAgentToolSpawnPoints != null)
            {
                for (int i = 0; i < aiAgentToolSpawnPoints.Length; i++)
                {
                    if (aiAgentToolSpawnPoints[i] != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(aiAgentToolSpawnPoints[i].position, 0.04f);
                        Handles.Label(aiAgentToolSpawnPoints[i].position + Vector3.up * 0.2f, $"Tool Spawn {i}");
                    }
                }
            }
            
            // Draw portal position
            if (portalPosition != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(portalPosition.position, 0.05f);
                Handles.Label(portalPosition.position + Vector3.up * 0.2f, "Portal");
            }
            
            // Draw tool position
            if (toolPosition != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(toolPosition.position, 0.05f);
                Handles.Label(toolPosition.position + Vector3.up * 0.2f, "Tool");
            }

            // Draw player spawn position
            if (playerSpawnPosition != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(playerSpawnPosition.position, 0.05f);
                Handles.Label(playerSpawnPosition.position + Vector3.up * 0.2f, "Player Spawn");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (isActiveAndEnabled)
            {
                DrawAllPositions();
            }
        }

        private void OnDrawGizmos()
        {
            if (!isActiveAndEnabled)
                return;

            // Check if any of the child positions are selected
            var selected = Selection.activeTransform;
            if (selected != null)
            {
                if (selected == recordingPosition || 
                    selected == aiAgentPosition || 
                    selected == portalPosition || 
                    selected == toolPosition ||
                    (aiAgentToolSpawnPoints != null && System.Array.IndexOf(aiAgentToolSpawnPoints, selected) >= 0))
                {
                    DrawAllPositions();
                }
            }
        }
#endif
}
}