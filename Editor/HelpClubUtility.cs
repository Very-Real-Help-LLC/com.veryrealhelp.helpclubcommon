using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VeryRealHelp.HelpClubCommon.World;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public class HelpClubUtility : EditorWindow
    {
        [MenuItem("VRH/Help Club Utility")]
        static void Open() => GetWindow<HelpClubUtility>(false, "Help Club Utility", true).Show();

        private static class Textures
        {
            private static Dictionary<string, Texture> cache = new Dictionary<string, Texture>();

            public static Texture Logo => Get("helpclublogo");
            public static Texture Error => Get("dot-red");
            public static Texture Warn => Get("dot-orange");
            public static Texture Okay => Get("dot-green");
            public static Texture Info => Get("dot-blue");

            public static Texture Get(string name)
            {
                cache.TryGetValue(name, out var texture);
                if (texture == null)
                {
                    texture = Resources.Load<Texture>($"com.veryrealhelp.helpclubcommon.editor/{name}");
                    cache[name] = texture;
                }
                return texture;
            }
        }


        private static GUIStyle _logoStyle;
        public static GUIStyle LogoStyle
        {
            get {
                if (_logoStyle == null)
                    _logoStyle = new GUIStyle()
                    {
                        padding = new RectOffset(8, 8, 8, 8),
                        normal = new GUIStyleState ()
                        {
                            background = Texture2D.whiteTexture
                        }
                    };
                return _logoStyle;
            }
        }

        private class CheckSet
        {
            public enum Severity { Suggestion, Requirement }

            public string label;
            public bool completed = false;
            public bool valid = false;
            public bool running = false;
            public bool autoFix = false;
            public Severity severity = Severity.Requirement;
            public CheckCollection checks;
            public List<CheckCollection.Result> results;

            public bool CanBeValidWithFixes => results.All(x => x.passed || x.check.HasFix);
            public Texture FailureTexture => severity == Severity.Requirement ? Textures.Error : Textures.Warn;

            public void OnGUI()
            {
                if (running)
                {
                    GUILayout.Space(30);
                    EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), 0, "Running");
                }
                else
                {
                    if (results != null)
                    {
                        GUILayout.Label(new GUIContent(label, valid ? Textures.Okay : CanBeValidWithFixes ? Textures.Warn : FailureTexture));
                        EditorGUI.indentLevel++;
                        results
                            .Where(x => !x.passed && !x.check.HasFix)
                            .ToList()
                            .ForEach(x =>
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(EditorGUI.indentLevel * 30);
                                GUILayout.Label(new GUIContent($"{x.check.label}: {x.check.invalidMessage}", FailureTexture), EditorStyles.miniLabel);
                                GUILayout.EndHorizontal();
                            });
                        results
                            .Where(x => x.passed && x.autoFixed || !x.passed && x.check.HasFix)
                            .ToList()
                            .ForEach(x =>
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(EditorGUI.indentLevel * 30);
                                GUILayout.Label(new GUIContent($"{x.check.label}: {x.check.invalidMessage}", Textures.Info), EditorStyles.miniLabel);
                                if (!x.passed && x.check.HasFix && GUILayout.Button("FIX", EditorStyles.miniButton))
                                {
                                    x.check.fix();
                                }
                                GUILayout.EndHorizontal();
                            });
                        EditorGUI.indentLevel--;
                    }
                }
            }

            public void Run()
            {
                valid = false;
                running = true;
                results = checks.GetResults(autoFix).ToList();
                valid = results.All(x => x.passed);
                running = false;
                completed = true;
            }
        }

        const int TRIANGLE_LIMIT = 200000;
        const int MATERIAL_LIMIT = 3;
        private static CheckCollection sceneSuggestions = new CheckCollection(
            /* new CheckCollection.Check(
                "Shaders", "Should only use Help Club standard shaders",
                () => UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                    .GetRootGameObjects()
                    .SelectMany(x => x.GetComponentsInChildren<Renderer>().SelectMany(y => y.sharedMaterials.Select(material => new
                    {
                        material,
                        y.gameObject
                    })))
                    .GroupBy(x => x.material, x => x.gameObject)
                    .Where(x => x.Key != null)
                    .Select(x => new
                    {
                        material = x.Key,
                        isValid = x.Key.shader.name.StartsWith("VeryRealHelp/"),
                        gameObject = x.FirstOrDefault()
                    })
                    .Select(x =>
                    {
                        if (!x.isValid)
                            Debug.LogError($"Material {x.material.name} in Scene not using Help Club shaders.", x.gameObject);
                        return x;
                    })
                    .ToList()
                    .All(x => x.isValid)
            ), */
            new CheckCollection.Check(
                "Materials", $"Should use no more than {MATERIAL_LIMIT} materials",
                () =>
                {
                    var count = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                        .GetRootGameObjects()
                        .SelectMany(x => x.GetComponentsInChildren<MeshRenderer>())
                        .SelectMany(x => x.sharedMaterials)
                        .Where(x => x != null)
                        .Distinct()
                        .Count();
                    var passing = count < MATERIAL_LIMIT;
                    if (!passing)
                        Debug.LogWarning($"Scene objects use {count} materials which is over the budget of {MATERIAL_LIMIT}");
                    return passing;
                }
            ),
            new CheckCollection.Check(
                "Geometry", $"Should have fewer than {TRIANGLE_LIMIT} triangles",
                () =>
                {
                    var triangleCount = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                        .GetRootGameObjects()
                        .SelectMany(x => x.GetComponentsInChildren<MeshFilter>())
                        .Select(x => x.sharedMesh)
                        .Where(x => x != null)
                        .Sum(x => x.triangles.Length / 3);
                    var passing = triangleCount < TRIANGLE_LIMIT;
                    if (!passing)
                        Debug.LogWarning($"Scene contains a total of {triangleCount} triangles which is over the budget of {TRIANGLE_LIMIT}");
                    return passing;
                }
            ),
            new CheckCollection.Check(
                "Mesh Colliders", "Should not use non-convex mesh colliders",
                () =>
                {
                    var offenders = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                        .GetRootGameObjects()
                        .SelectMany(x => x.GetComponentsInChildren<MeshCollider>())
                        .Where(x => !x.convex)
                        .ToList();
                    offenders.ForEach(x => Debug.LogWarning($"Non-Convex MeshCollider: {x.gameObject.name}", x.gameObject));
                    return offenders.Count == 0;
                }
            )
        );

        public static readonly CheckCollection assetChecks = new CheckCollection(
            new CheckCollection.Check(
                "Shaders", "Should only use Help Club standard shaders",
                () => AssetDatabase.FindAssets("t:Material")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<Material>)
                    .Select(material => new
                    {
                        material,
                        isValid = material.shader.name.StartsWith("VeryRealHelp/")
                    })
                    .Select(x =>
                    {
                        if (!x.isValid)
                            Debug.LogWarning($"Material {x.material.name} in Assets not using Help Club shaders.", x.material);
                        return x;
                    })
                    .ToList()
                    .All(x => x.isValid)
            )
        );

        private readonly List<CheckSet> checkSets = new List<CheckSet>()
        {
            new CheckSet() { label = "Project Settings", checks = WorldValidator.settingsChecks, autoFix = true },
            new CheckSet() { label = "Scene Requirements", checks = WorldValidator.sceneRequirementChecks, autoFix = false },
            new CheckSet() { label = "Scene Suggestions", checks = sceneSuggestions, autoFix = false, severity = CheckSet.Severity.Suggestion },
            new CheckSet() { label = "Asset Bundles", checks = WorldValidator.assetBundleChecks, autoFix = true, severity = CheckSet.Severity.Requirement},
            new CheckSet() { label = "World Info Files", checks = WorldValidator.worldInfoChecks, autoFix = true, severity = CheckSet.Severity.Requirement},
            //new CheckSet() { label = "Assets", checks = assetChecks, autoFix = false, severity = CheckSet.Severity.Suggestion },
        };

        private void RunChecks()
        {
            Debug.Log("### ------- Running Checks ------- ###");
            checkSets.ForEach(x => x.Run());
        }

        private void OnGUI()
        {
            GUI.backgroundColor = Color.gray;
            GUILayout.Box(Textures.Logo, LogoStyle);
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Run Checks", EditorStyles.miniButton))
                RunChecks();
            checkSets.ForEach(x => x.OnGUI());
            if (checkSets.Any(x => x.completed && !x.valid))
            {
                EditorGUILayout.HelpBox("Some checks did not pass validation. More information may be available in the Unity Console.", MessageType.Warning);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label("Legend", EditorStyles.miniBoldLabel);
                GUILayout.Label(new GUIContent("Okay", Textures.Okay), EditorStyles.miniLabel);
                GUILayout.Label(new GUIContent("Problem", Textures.Error), EditorStyles.miniLabel);
                GUILayout.Label(new GUIContent("Warning", Textures.Warn), EditorStyles.miniLabel);
                GUILayout.Label(new GUIContent("Info", Textures.Info), EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
            }
        }
    }
}
