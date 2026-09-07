using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Navigation;
using CDG.EditorTools.Validation.Rules;
using CDG.EditorTools.Validation.Scopes;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationStabilityTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";
        private const string PrefabFolderPath = TestFolderPath + "/Prefabs";
        private const string SceneFolderPath = TestFolderPath + "/Scenes";

        private sealed class RecordingValidationRule : IValidationRule
        {
            public ValidationIssueType Type => ValidationIssueType.MissingScript;

            internal Dictionary<string, int> AssetVisitCounts { get; } = new Dictionary<string, int>();
            internal int ObjectVisitCount { get; private set; }

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                ObjectVisitCount++;

                if (string.IsNullOrEmpty(assetPath))
                {
                    return;
                }

                if (!AssetVisitCounts.ContainsKey(assetPath))
                {
                    AssetVisitCounts.Add(assetPath, 0);
                }

                AssetVisitCounts[assetPath]++;
            }
        }

        private sealed class CancelOnAssetProgress : IValidationProgress
        {
            private readonly string _targetAssetPath;

            internal CancelOnAssetProgress(string targetAssetPath)
            {
                _targetAssetPath = targetAssetPath;
            }

            public bool Report(string title, string detail, float progress)
            {
                return detail != null && detail.IndexOf(_targetAssetPath, StringComparison.Ordinal) >= 0;
            }
        }

        private UnityEngine.Object[] _previousSelection;
        private Scene _previousActiveScene;

        [SetUp]
        public void SetUp()
        {
            StageUtility.GoToMainStage();

            _previousSelection = Selection.objects;
            _previousActiveScene = SceneManager.GetActiveScene();

            Selection.objects = Array.Empty<UnityEngine.Object>();

            EnsureFolder(TestFolderPath, "Assets", "CDGEditorToolsTests");
            EnsureFolder(PrefabFolderPath, TestFolderPath, "Prefabs");
            EnsureFolder(SceneFolderPath, TestFolderPath, "Scenes");
        }

        [TearDown]
        public void TearDown()
        {
            StageUtility.GoToMainStage();
            CloseLoadedTestScenes();

            if (_previousActiveScene.IsValid() && _previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(_previousActiveScene);
            }

            Selection.objects = _previousSelection;

            if (AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.DeleteAsset(TestFolderPath);
            }
        }

        [Test]
        public void Selection_WithSameGameObjectAndComponent_DoesNotValidateObjectTwice()
        {
            GameObject gameObject = new GameObject("Target");

            try
            {
                BoxCollider component = gameObject.AddComponent<BoxCollider>();
                Selection.objects = new UnityEngine.Object[] { gameObject, component };

                RecordingValidationRule rule = new RecordingValidationRule();
                SelectionValidationScope scope = new SelectionValidationScope();

                scope.Validate(new IValidationRule[] { rule }, new List<ValidationIssue>());

                Assert.AreEqual(1, rule.ObjectVisitCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ProjectScenes_WhenValidatedTwice_VisitsEachSceneTwiceAndLeavesScenesClosed()
        {
            const int sceneCount = 5;

            List<string> scenePaths = CreateScenes(sceneCount);
            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();

            scope.ValidateSceneAssets(new IValidationRule[] { rule }, new List<ValidationIssue>());
            scope.ValidateSceneAssets(new IValidationRule[] { rule }, new List<ValidationIssue>());

            for (int i = 0; i < scenePaths.Count; i++)
            {
                Assert.IsTrue(rule.AssetVisitCounts.ContainsKey(scenePaths[i]));
                Assert.AreEqual(2, rule.AssetVisitCounts[scenePaths[i]]);
                Assert.IsFalse(IsSceneLoaded(scenePaths[i]));
            }
        }

        [Test]
        public void ProjectScope_AfterCancellation_CanRunAgainSuccessfully()
        {
            List<string> prefabPaths = CreatePrefabs(3);
            List<string> scenePaths = CreateScenes(3);
            string cancelScenePath = scenePaths[1];

            CancelOnAssetProgress progress = new CancelOnAssetProgress(cancelScenePath);
            ProjectValidationScope canceledScope = new ProjectValidationScope(progress);

            Assert.Throws<OperationCanceledException>(() => canceledScope.ValidateProjectAssets(new IValidationRule[] { new RecordingValidationRule() }, new List<ValidationIssue>()));

            for (int i = 0; i < scenePaths.Count; i++)
            {
                Assert.IsFalse(IsSceneLoaded(scenePaths[i]));
            }

            RecordingValidationRule secondRule = new RecordingValidationRule();
            ProjectValidationScope secondScope = new ProjectValidationScope();

            secondScope.ValidateProjectAssets(new IValidationRule[] { secondRule }, new List<ValidationIssue>());

            for (int i = 0; i < prefabPaths.Count; i++)
            {
                Assert.IsTrue(secondRule.AssetVisitCounts.ContainsKey(prefabPaths[i]));
            }

            for (int i = 0; i < scenePaths.Count; i++)
            {
                Assert.IsTrue(secondRule.AssetVisitCounts.ContainsKey(scenePaths[i]));
                Assert.IsFalse(IsSceneLoaded(scenePaths[i]));
            }
        }

        [Test]
        public void ResultNavigator_WithDeletedPrefabIssue_ReturnsFalseWithoutOpeningPrefabStage()
        {
            const string prefabPath = PrefabFolderPath + "/DeletedTarget.prefab";

            CreatePrefab(prefabPath);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab);

            ValidationLocation location = ValidationLocationFactory.Create(prefab, null, prefabPath, string.Empty);
            ValidationIssue issue = new ValidationIssue(ValidationIssueType.MissingScript, "테스트 문제입니다.", location);

            Assert.IsTrue(AssetDatabase.DeleteAsset(prefabPath));

            bool result = ValidationResultNavigator.TryNavigate(issue);

            Assert.IsFalse(result);
            Assert.IsNull(PrefabStageUtility.GetCurrentPrefabStage());
        }

        private static List<string> CreatePrefabs(int count)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string path = $"{PrefabFolderPath}/Prefab_{i:D3}.prefab";
                CreatePrefab(path);
                paths.Add(path);
            }

            return paths;
        }

        private static void CreatePrefab(string assetPath)
        {
            GameObject root = new GameObject("PrefabRoot");

            try
            {
                Assert.IsNotNull(PrefabUtility.SaveAsPrefabAsset(root, assetPath));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
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
                SceneManager.MoveGameObjectToScene(root, scene);

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