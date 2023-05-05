using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public static class NavMeshCreator
    {
        public const string NavMeshGeometry = "NavMeshGeometry";
        public const string NavMeshColliderPrefab = "NavMeshColliderPrefab";
        public const string TeleportLayer = "Teleport";

        [MenuItem("VRH/Create NavMesh")]
        public static void CreateNavMesh()
        {
            //Update nav mesh
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            var triangulation = NavMesh.CalculateTriangulation();

            //Get paths
            var activeScene = SceneManager.GetActiveScene().name;
            var navMeshGeometryPath = $"Assets/Scenes/{activeScene}/{NavMeshGeometry}.asset";
            var navMeshColliderPrefabPath = $"Assets/Scenes/{activeScene}/{NavMeshColliderPrefab}.prefab";

            //Cleanup existing
            AssetDatabase.DeleteAsset(navMeshGeometryPath);
            AssetDatabase.DeleteAsset(navMeshColliderPrefabPath);
            var colliderInScene = GameObject.Find(NavMeshColliderPrefab);
            if (colliderInScene != null) {
                GameObject.DestroyImmediate(colliderInScene);
            }

            //Create mesh
            var mesh = new Mesh
            {
                vertices = triangulation.vertices,
                triangles = triangulation.indices
            };
            MeshUtility.Optimize(mesh);
            AssetDatabase.CreateAsset(mesh, navMeshGeometryPath);

            //Create prefab
            GameObject navMeshColliderGameobject = new GameObject();
            var meshFilter = navMeshColliderGameobject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            navMeshColliderGameobject.name = NavMeshColliderPrefab;
            navMeshColliderGameobject.AddComponent<MeshCollider>();
            navMeshColliderGameobject.layer = LayerMask.NameToLayer(TeleportLayer);
            PrefabUtility.SaveAsPrefabAssetAndConnect(navMeshColliderGameobject, navMeshColliderPrefabPath, InteractionMode.AutomatedAction);

            AssetDatabase.Refresh();

            Debug.Log($"Created NavMesh: {navMeshGeometryPath}");
        }
    }
}
