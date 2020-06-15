using System.Collections;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Linq;
using System.Reflection;

namespace VeryRealHelp.HelpClubCommon.Editor
{

    public class SceneStatsWindow : EditorWindow
    {
        public abstract class Unit
        {
            public abstract IEnumerator ScanCoroutine();
            public abstract void OnGUI();

            public static readonly GUIStyle whiteBackground = new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = Texture2D.whiteTexture
                }
            };
        }

        [MenuItem("VRH/Analysis/Scene Stats")]
        static void Init()
        {
            SceneStatsWindow window = GetWindow<SceneStatsWindow>(false, "Scene Stats", true);
            window.Show();
        }

        private static Unit[] GetDefaultUnits() => new Unit[]
        {
            new MaterialsUnit(),
            new MeshesUnit(),
        };

        public static void SetSearchFilter(string filter, int filterMode)
        {
            var windows = Resources.FindObjectsOfTypeAll<SearchableEditorWindow>();
            SearchableEditorWindow hierarchy = null;
            foreach (var window in windows)
            {
                if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
                {
                    hierarchy = window;
                    break;
                }
            }
            if (hierarchy != null)
            {
                MethodInfo setSearchType = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
                object[] parameters = new object[] { filter, filterMode, false, false };
                setSearchType.Invoke(hierarchy, parameters);
            }
        }

        private string statusMessage = "";
        private EditorCoroutine scanCoroutine;
        private Unit[] units;
        private Vector2 scrollPosition;

        private bool IsScanning => scanCoroutine != null;

        private void OnEnable()
        {
            if (units == null)
                units = GetDefaultUnits();
        }

        public void OnSelectionChange()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (IsScanning)
            {
                GUILayout.Label("Stats");
                EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), 1f, statusMessage);
            }
            else
            {
                if (GUILayout.Button("Scan"))
                {
                    Scan();
                }
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                foreach (var unit in units)
                {
                    unit.OnGUI();
                    EditorGUILayout.Separator();
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void Scan()
        {
            scanCoroutine = EditorCoroutineUtility.StartCoroutine(ScanCoroutine(), this);
        }

        private IEnumerator ScanCoroutine()
        {
            foreach (var unit in units)
            {
                yield return EditorCoroutineUtility.StartCoroutine(unit.ScanCoroutine(), this);
            }
            scanCoroutine = null;
        }
    }

    public class MaterialsUnit : SceneStatsWindow.Unit
    {
        private class MaterialUsage
        {
            public int count;
            public Material material;

            public MaterialUsage (IGrouping<int?, Material> group)
            {
                count = group.Count();
                material = group.First();
            }
        }

        private bool hasRun = false;
        private bool enabled = true;
        private int rendererCount;
        private int materialCount;
        private int passCount;
        private MaterialUsage[] usedMaterials;

        public override void OnGUI()
        {
            enabled = EditorGUILayout.BeginToggleGroup("Materials", enabled);
            EditorGUI.indentLevel += 1;
            if (hasRun)
            {
                EditorGUILayout.LabelField(string.Format("{0:n0} Renderers", rendererCount));
                EditorGUILayout.LabelField(string.Format("{0:n0} Materials", materialCount));
                EditorGUILayout.LabelField(string.Format("{0:n0} Passes (minimum)", passCount));
                if (enabled)
                {
                    EditorGUI.indentLevel += 1;
                    foreach (var usage in usedMaterials)
                    {
                        if (Selection.activeObject == usage.material)
                        {
                            GUI.backgroundColor = Color.HSVToRGB(0, 0, 0.6f);
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(usage.material.name, EditorStyles.boldLabel);
                        if (GUILayout.Button("Highlight", EditorStyles.miniButton))
                        {
                            EditorGUIUtility.PingObject(usage.material);
                        }
                        if (GUILayout.Button("Find in Scene", EditorStyles.miniButton))
                        {
                            var searchPath = AssetDatabase.GetAssetPath(usage.material).Substring("Assets/".Length);
                            SceneStatsWindow.SetSearchFilter(string.Format("ref:\"{0}\"", searchPath), 0);
                        }
                        if (GUILayout.Button("Select", EditorStyles.miniButton))
                        {
                            EditorGUIUtility.PingObject(usage.material);
                            Selection.activeObject = usage.material;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.LabelField(string.Format("{0} ({1})", AssetDatabase.GetAssetPath(usage.material), usage.material.name), EditorStyles.miniLabel);
                        EditorGUILayout.LabelField(usage.material.shader.name, EditorStyles.miniBoldLabel);
                        EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(usage.material.shader), EditorStyles.miniLabel);
                        EditorGUILayout.LabelField(string.Format("{0:n0} times, {1:n0} passes", usage.count, usage.material?.passCount ?? 0), EditorStyles.miniLabel);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUI.indentLevel -= 1;
                }
            }
            else
            {
                EditorGUILayout.LabelField("Not scanned yet.");
            }
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndToggleGroup();
        }

        public override IEnumerator ScanCoroutine()
        {
            if (enabled)
            {
                var renderers = Object.FindObjectsOfType<Renderer>();
                var materials = renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).GroupBy(m => m?.GetInstanceID());
                var materialUsages = materials.Select(g => new MaterialUsage(g));
                rendererCount = renderers.Count();
                materialCount = materials.Count();
                passCount = materialUsages.Aggregate(0, (a,u) => a + u.material.passCount);
                usedMaterials = materialUsages.OrderBy(u => -u.count).ToArray();
                hasRun = true;
            }
            else
            {
                hasRun = false;
            }
            yield break;
        }
    }

    public class MeshesUnit : SceneStatsWindow.Unit
    {
        private class MeshUsage
        {
            public int count;
            public Mesh mesh;
            public int triangleCount;
            public int sceneTriangles;

            public MeshUsage(IGrouping<int?, Mesh> group)
            {
                count = group.Count();
                mesh = group.First();
                triangleCount = mesh.triangles.Length / 3;
                sceneTriangles = group.Count() * mesh.triangles.Length / 3;
            }
        }

        private bool hasRun = false;
        private bool enabled = true;
        private int filterCount;
        private int meshCount;
        private int triangleCount;
        private MeshUsage[] usedMeshes;

        public override void OnGUI()
        {
            enabled = EditorGUILayout.BeginToggleGroup("Meshes", enabled);
            EditorGUI.indentLevel += 1;
            if (hasRun)
            {
                EditorGUILayout.LabelField(string.Format("{0:n0} MeshFilters", filterCount));
                EditorGUILayout.LabelField(string.Format("{0:n0} Meshes", meshCount));
                EditorGUILayout.LabelField(string.Format("{0:n0} Triangles in Scene", triangleCount));
                if (enabled)
                {
                    EditorGUI.indentLevel += 1;
                    foreach (var usage in usedMeshes)
                    {
                        if (Selection.activeObject == usage.mesh)
                        {
                            GUI.backgroundColor = Color.HSVToRGB(0, 0, 0.6f);
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(usage.mesh.name, EditorStyles.boldLabel);
                        if (GUILayout.Button("Highlight", EditorStyles.miniButton))
                            EditorGUIUtility.PingObject(usage.mesh);
                        if (GUILayout.Button("Find in Scene", EditorStyles.miniButton))
                        {
                            var searchPath = AssetDatabase.GetAssetPath(usage.mesh).Substring("Assets/".Length);
                            SceneStatsWindow.SetSearchFilter(string.Format("ref:{0}:\"{1}\"", usage.mesh.GetInstanceID(), searchPath), 0);
                        }
                        if (GUILayout.Button("Select", EditorStyles.miniButton))
                        {
                            EditorGUIUtility.PingObject(usage.mesh);
                            Selection.activeObject = usage.mesh;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(usage.mesh), EditorStyles.miniLabel);
                        EditorGUILayout.LabelField(string.Format("{0:n0} total triangles ({1:n0} triangles * {2:n0} usages)", usage.sceneTriangles, usage.triangleCount, usage.count), EditorStyles.miniLabel);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUI.indentLevel -= 1;
                }
            }
            else
            {
                EditorGUILayout.LabelField("Not scanned yet.");
            }
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndToggleGroup();
        }

        public override IEnumerator ScanCoroutine()
        {
            if (enabled)
            {
                var meshFilters = Object.FindObjectsOfType<MeshRenderer>();
                var meshes = meshFilters.Select(f => f.GetComponent<MeshFilter>()?.sharedMesh).GroupBy(m => m?.GetInstanceID());
                var meshUsages = meshes.Select(g => new MeshUsage(g));
                filterCount = meshFilters.Count();
                meshCount = meshes.Count();
                usedMeshes = meshUsages.OrderBy(u => -u.sceneTriangles).ToArray();
                triangleCount = meshUsages.Aggregate(0, (a,u) => a + u.sceneTriangles);
                hasRun = true;
            }
            else
            {
                hasRun = false;
            }
            yield break;
        }
    }
} 
