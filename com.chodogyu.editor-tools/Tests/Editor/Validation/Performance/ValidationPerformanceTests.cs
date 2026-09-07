using System;
using System.Collections.Generic;
using System.Diagnostics;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using CDG.EditorTools.Validation.Scopes;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationPerformanceTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";
        private const string PrefabFolderPath = TestFolderPath + "/PerformancePrefabs";
        private const string SceneFolderPath = TestFolderPath + "/PerformanceScenes";

        private sealed class NoOpValidationRule : IValidationRule
        {
            public ValidationIssueType Type => ValidationIssueType.MissingScript;

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
            }
        }

        private Scene _previousActiveScene;

        [SetUp]
        public void SetUp()
        {
            _previousActiveScene = SceneManager.GetActiveScene();

            EnsureFolder(TestFolderPath, "Assets", "CDGEditorToolsTests");
            EnsureFolder(PrefabFolderPath, TestFolderPath, "PerformancePrefabs");
            EnsureFolder(SceneFolderPath, TestFolderPath, "PerformanceScenes");
        }

        [TearDown]
        public void TearDown()
        {
            CloseLoadedTestScenes();

            if (_previousActiveScene.IsValid() && _previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(_previousActiveScene);
            }

            if (AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.DeleteAsset(TestFolderPath);
            }
        }

        [Test]
        public void HierarchyTraversal_WithOneThousandObjects_LogsElapsedTime()
        {
            const int childCount = 1000;

            GameObject root = CreateFlatHierarchy(childCount);

            try
            {
                int visitedCount = 0;
                Stopwatch stopwatch = Stopwatch.StartNew();

                GameObjectHierarchyTraversal.Traverse(root, gameObject => visitedCount++);

                stopwatch.Stop();

                TestContext.WriteLine($"[Performance] Hierarchy Traversal: {childCount + 1} GameObjects / {stopwatch.ElapsedMilliseconds} ms");

                Assert.AreEqual(childCount + 1, visitedCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void HierarchyValidation_WithActualRules_LogsElapsedTime()
        {
            const int childCount = 1000;

            GameObject root = CreateReferenceHierarchy(childCount);

            try
            {
                IReadOnlyList<IValidationRule> rules = new IValidationRule[]
                {
                    new MissingScriptRule(),
                    new BrokenSerializedReferenceRule()
                };

                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                Stopwatch stopwatch = Stopwatch.StartNew();

                runner.Validate(root, string.Empty, rules, issues);

                stopwatch.Stop();

                TestContext.WriteLine($"[Performance] Hierarchy + Actual Rules: {childCount + 1} GameObjects / {stopwatch.ElapsedMilliseconds} ms");

                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ProjectPrefabs_WithFiftyAssets_LogsElapsedTime()
        {
            const int prefabCount = 50;

            CreatePrefabs(prefabCount);

            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();
            IReadOnlyList<IValidationRule> rules = new IValidationRule[]
            {
                new NoOpValidationRule()
            };

            Stopwatch stopwatch = Stopwatch.StartNew();

            scope.Validate(rules, new List<ValidationIssue>());

            stopwatch.Stop();

            TestContext.WriteLine($"[Performance] Project Prefabs: {prefabCount} test Prefabs + existing project assets / {stopwatch.ElapsedMilliseconds} ms");

            for (int i = 0; i < prefabCount; i++)
            {
                string path = $"{PrefabFolderPath}/Prefab_{i:D3}.prefab";
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            }
        }

        [Test]
        public void ProjectScenes_WithTwentyAssets_LogsElapsedTime()
        {
            const int sceneCount = 20;

            List<string> scenePaths = CreateScenes(sceneCount);

            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();
            IReadOnlyList<IValidationRule> rules = new IValidationRule[]
            {
                new NoOpValidationRule()
            };

            Stopwatch stopwatch = Stopwatch.StartNew();

            scope.ValidateSceneAssets(rules, new List<ValidationIssue>());

            stopwatch.Stop();

            TestContext.WriteLine($"[Performance] Project Scenes: {sceneCount} test Scenes + existing project assets / {stopwatch.ElapsedMilliseconds} ms");

            for (int i = 0; i < scenePaths.Count; i++)
            {
                Assert.IsFalse(IsSceneLoaded(scenePaths[i]));
            }
        }

        private static GameObject CreateFlatHierarchy(int childCount)
        {
            GameObject root = new GameObject("PerformanceRoot");

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = new GameObject($"Child_{i:D4}");
                child.transform.SetParent(root.transform);
            }

            return root;
        }

        private static GameObject CreateReferenceHierarchy(int childCount)
        {
            GameObject root = new GameObject("PerformanceReferenceRoot");

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = new GameObject($"Reference_{i:D4}");
                child.transform.SetParent(root.transform);
                child.AddComponent<MeshFilter>();
            }

            return root;
        }

        private static void CreatePrefabs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                string path = $"{PrefabFolderPath}/Prefab_{i:D3}.prefab";
                GameObject root = new GameObject($"Prefab_{i:D3}");

                try
                {
                    GameObject child = new GameObject("Child");
                    child.transform.SetParent(root.transform);

                    Assert.IsNotNull(PrefabUtility.SaveAsPrefabAsset(root, path));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static List<string> CreateScenes(int count)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string path = $"{SceneFolderPath}/Scene_{i:D3}.unity";
                CreateScene(path, $"SceneRoot_{i:D3}");
                paths.Add(path);
            }

            return paths;
        }

        private static void CreateScene(string scenePath, string rootName)
        {
            Scene sourceScene = SceneManager.GetActiveScene();

            if (!EditorSceneManager.SaveScene(sourceScene, scenePath, true))
            {
                throw new InvalidOperationException($"테스트 Scene 복사본을 저장하지 못했습니다: {scenePath}");
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                GameObject[] existingRoots = scene.GetRootGameObjects();

                for (int i = 0; i < existingRoots.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(existingRoots[i]);
                }

                GameObject root = new GameObject(rootName);
                GameObject child = new GameObject("Child");

                SceneManager.MoveGameObjectToScene(root, scene);
                SceneManager.MoveGameObjectToScene(child, scene);
                child.transform.SetParent(root.transform);

                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException($"테스트 Scene을 저장하지 못했습니다: {scenePath}");
                }
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static bool IsSceneLoaded(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            return scene.IsValid() && scene.isLoaded;
        }

        private static void CloseLoadedTestScenes()
        {
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path))
                {
                    continue;
                }

                if (scene.path.StartsWith(TestFolderPath, StringComparison.Ordinal))
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void EnsureFolder(string folderPath, string parentPath, string folderName)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
    }
}