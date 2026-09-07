using System;
using System.Collections.Generic;
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
    internal sealed class LargeProjectValidationTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";
        private const string PrefabFolderPath = TestFolderPath + "/Prefabs";
        private const string SceneFolderPath = TestFolderPath + "/Scenes";

        private sealed class RecordingValidationRule : IValidationRule
        {
            public ValidationIssueType Type => ValidationIssueType.MissingScript;

            internal HashSet<string> VisitedAssetPaths { get; } = new HashSet<string>();

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                VisitedAssetPaths.Add(assetPath);
            }
        }

        private sealed class CancelOnAssetProgress : IValidationProgress
        {
            private readonly string _cancelAssetPath;

            internal CancelOnAssetProgress(string cancelAssetPath)
            {
                _cancelAssetPath = cancelAssetPath;
            }

            public bool Report(string title, string detail, float progress)
            {
                return detail != null && detail.IndexOf(_cancelAssetPath, StringComparison.Ordinal) >= 0;
            }
        }

        private Scene _previousActiveScene;

        [SetUp]
        public void SetUp()
        {
            _previousActiveScene = SceneManager.GetActiveScene();

            EnsureFolder(TestFolderPath, "Assets", "CDGEditorToolsTests");
            EnsureFolder(PrefabFolderPath, TestFolderPath, "Prefabs");
            EnsureFolder(SceneFolderPath, TestFolderPath, "Scenes");
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
        public void ProjectPrefabs_WithManyAssets_ValidatesEveryCreatedPrefab()
        {
            const int prefabCount = 20;

            List<string> prefabPaths = CreatePrefabs(prefabCount);
            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();

            scope.Validate(new IValidationRule[] { rule }, new List<ValidationIssue>());

            for (int i = 0; i < prefabPaths.Count; i++)
            {
                Assert.IsTrue(rule.VisitedAssetPaths.Contains(prefabPaths[i]), $"검사되지 않은 Prefab: {prefabPaths[i]}");
            }
        }

        [Test]
        public void ProjectPrefabs_WhenCanceledMidScan_DoesNotValidateLaterPrefab()
        {
            const int prefabCount = 20;

            List<string> prefabPaths = CreatePrefabs(prefabCount);
            string cancelPath = prefabPaths[10];

            RecordingValidationRule rule = new RecordingValidationRule();
            CancelOnAssetProgress progress = new CancelOnAssetProgress(cancelPath);
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope(progress);

            Assert.Throws<OperationCanceledException>(() => scope.Validate(new IValidationRule[] { rule }, new List<ValidationIssue>()));

            Assert.IsTrue(rule.VisitedAssetPaths.Contains(prefabPaths[0]));
            Assert.IsFalse(rule.VisitedAssetPaths.Contains(cancelPath));
            Assert.IsFalse(rule.VisitedAssetPaths.Contains(prefabPaths[19]));
        }

        [Test]
        public void ProjectScenes_WithManyAssets_ValidatesEverySceneAndLeavesScenesClosed()
        {
            const int sceneCount = 10;

            List<string> scenePaths = CreateScenes(sceneCount);
            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();

            scope.ValidateSceneAssets(new IValidationRule[] { rule }, new List<ValidationIssue>());

            for (int i = 0; i < scenePaths.Count; i++)
            {
                Assert.IsTrue(rule.VisitedAssetPaths.Contains(scenePaths[i]), $"검사되지 않은 Scene: {scenePaths[i]}");
                Assert.IsFalse(IsSceneLoaded(scenePaths[i]), $"검사 후에도 Scene이 열려 있습니다: {scenePaths[i]}");
            }
        }

        [Test]
        public void ProjectScenes_WhenCanceledMidScan_ClosesTemporaryScenesAndRestoresActiveScene()
        {
            const int sceneCount = 10;

            List<string> scenePaths = CreateScenes(sceneCount);
            string cancelPath = scenePaths[5];

            RecordingValidationRule rule = new RecordingValidationRule();
            CancelOnAssetProgress progress = new CancelOnAssetProgress(cancelPath);
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope(progress);

            Assert.Throws<OperationCanceledException>(() => scope.ValidateSceneAssets(new IValidationRule[] { rule }, new List<ValidationIssue>()));

            Assert.IsTrue(rule.VisitedAssetPaths.Contains(scenePaths[0]));
            Assert.IsFalse(rule.VisitedAssetPaths.Contains(cancelPath));
            Assert.IsFalse(rule.VisitedAssetPaths.Contains(scenePaths[9]));

            for (int i = 0; i < scenePaths.Count; i++)
            {
                Assert.IsFalse(IsSceneLoaded(scenePaths[i]), $"취소 후에도 Scene이 열려 있습니다: {scenePaths[i]}");
            }

            Assert.IsTrue(SceneManager.GetActiveScene() == _previousActiveScene);
        }

        [Test]
        public void ProjectScope_WhenCanceledDuringScenePhase_LeavesNoTestSceneLoaded()
        {
            const int prefabCount = 10;
            const int sceneCount = 6;

            List<string> prefabPaths = CreatePrefabs(prefabCount);
            List<string> scenePaths = CreateScenes(sceneCount);
            string cancelScenePath = scenePaths[3];

            RecordingValidationRule rule = new RecordingValidationRule();
            CancelOnAssetProgress progress = new CancelOnAssetProgress(cancelScenePath);
            ProjectValidationScope scope = new ProjectValidationScope(progress);

            Assert.Throws<OperationCanceledException>(() => scope.ValidateProjectAssets(new IValidationRule[] { rule }, new List<ValidationIssue>()));

            for (int i = 0; i < prefabPaths.Count; i++)
            {
                Assert.IsTrue(rule.VisitedAssetPaths.Contains(prefabPaths[i]), $"Project Scope에서 검사되지 않은 Prefab: {prefabPaths[i]}");
            }

            Assert.IsTrue(rule.VisitedAssetPaths.Contains(scenePaths[0]));
            Assert.IsFalse(rule.VisitedAssetPaths.Contains(cancelScenePath));
            Assert.IsFalse(rule.VisitedAssetPaths.Contains(scenePaths[5]));

            for (int i = 0; i < scenePaths.Count; i++)
            {
                Assert.IsFalse(IsSceneLoaded(scenePaths[i]), $"Project Scope 취소 후에도 Scene이 열려 있습니다: {scenePaths[i]}");
            }

            Assert.IsTrue(SceneManager.GetActiveScene() == _previousActiveScene);
        }

        private static List<string> CreatePrefabs(int count)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string path = $"{PrefabFolderPath}/Prefab_{i:D3}.prefab";
                GameObject root = new GameObject($"Prefab_{i:D3}");

                try
                {
                    GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                    Assert.IsNotNull(savedPrefab);
                    paths.Add(path);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }

            return paths;
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