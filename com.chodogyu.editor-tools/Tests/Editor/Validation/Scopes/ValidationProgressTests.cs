using System;
using System.Collections.Generic;
using System.Linq;
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
    internal sealed class ValidationProgressTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";

        private sealed class RecordingProgress : IValidationProgress
        {
            internal List<string> Titles { get; } = new List<string>();
            internal List<string> Details { get; } = new List<string>();
            internal int CancelOnReportNumber { get; set; } = -1;
            internal int ReportCount { get; private set; }

            public bool Report(string title, string detail, float progress)
            {
                ReportCount++;
                Titles.Add(title);
                Details.Add(detail);

                return ReportCount == CancelOnReportNumber;
            }
        }

        [SetUp]
        public void SetUp()
        {
            EnsureTestFolderExists();
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.DeleteAsset(TestFolderPath);
            }
        }

        [Test]
        public void ProjectPrefabs_WithProgress_ReportsCreatedPrefab()
        {
            const string prefabPath = TestFolderPath + "/ProgressPrefab.prefab";

            CreatePrefab(prefabPath);

            RecordingProgress progress = new RecordingProgress();
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope(progress);
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(Array.Empty<IValidationRule>(), issues);

            Assert.IsTrue(progress.Details.Any(detail => detail.Contains(prefabPath)));
        }

        [Test]
        public void ProjectPrefabs_WhenProgressRequestsCancel_ThrowsOperationCanceledException()
        {
            CreatePrefab(TestFolderPath + "/CancelPrefab.prefab");

            RecordingProgress progress = new RecordingProgress
            {
                CancelOnReportNumber = 1
            };

            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope(progress);

            Assert.Throws<OperationCanceledException>(() => scope.Validate(Array.Empty<IValidationRule>(), new List<ValidationIssue>()));
        }

        [Test]
        public void ProjectScenes_WithProgress_ReportsCreatedScene()
        {
            const string scenePath = TestFolderPath + "/ProgressScene.unity";

            CreateScene(scenePath);

            RecordingProgress progress = new RecordingProgress();
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope(progress);

            scope.ValidateSceneAssets(Array.Empty<IValidationRule>(), new List<ValidationIssue>());

            Assert.IsTrue(progress.Details.Any(detail => detail.Contains(scenePath)));
        }

        [Test]
        public void ProjectScenes_WhenProgressRequestsCancel_ThrowsOperationCanceledException()
        {
            CreateScene(TestFolderPath + "/CancelScene.unity");

            RecordingProgress progress = new RecordingProgress
            {
                CancelOnReportNumber = 1
            };

            ProjectScenesValidationScope scope = new ProjectScenesValidationScope(progress);

            Assert.Throws<OperationCanceledException>(() => scope.ValidateSceneAssets(Array.Empty<IValidationRule>(), new List<ValidationIssue>()));
        }

        [Test]
        public void ProjectScope_WithProgress_ReportsPrefabAndScenePhases()
        {
            CreatePrefab(TestFolderPath + "/ProjectProgress.prefab");
            CreateScene(TestFolderPath + "/ProjectProgress.unity");

            RecordingProgress progress = new RecordingProgress();
            ProjectValidationScope scope = new ProjectValidationScope(progress);

            scope.ValidateProjectAssets(Array.Empty<IValidationRule>(), new List<ValidationIssue>());

            Assert.IsTrue(progress.Titles.Contains("CDG Validation - Project Prefabs"));
            Assert.IsTrue(progress.Titles.Contains("CDG Validation - Project Scenes"));
        }

        [Test]
        public void ProjectScope_WhenProgressRequestsCancel_PropagatesCancellation()
        {
            CreatePrefab(TestFolderPath + "/ProjectCancel.prefab");

            RecordingProgress progress = new RecordingProgress
            {
                CancelOnReportNumber = 1
            };

            ProjectValidationScope scope = new ProjectValidationScope(progress);

            Assert.Throws<OperationCanceledException>(() => scope.ValidateProjectAssets(Array.Empty<IValidationRule>(), new List<ValidationIssue>()));
        }

        private static void CreatePrefab(string assetPath)
        {
            GameObject root = new GameObject("ProgressPrefab");

            try
            {
                Assert.IsNotNull(PrefabUtility.SaveAsPrefabAsset(root, assetPath));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void CreateScene(string scenePath)
        {
            Scene sourceScene = SceneManager.GetActiveScene();

            if (!EditorSceneManager.SaveScene(sourceScene, scenePath, true))
            {
                throw new InvalidOperationException($"테스트 Scene 복사본을 저장하지 못했습니다: {scenePath}");
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                GameObject[] roots = scene.GetRootGameObjects();

                for (int i = 0; i < roots.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(roots[i]);
                }

                GameObject root = new GameObject("ProgressScene");
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

        private static void EnsureTestFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "CDGEditorToolsTests");
            }
        }
    }
}