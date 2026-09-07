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
    internal sealed class LoadedScenesValidationScopeTests
    {
        private sealed class RecordingValidationRule : IValidationRule
        {
            public ValidationIssueType Type => ValidationIssueType.MissingScript;

            internal List<GameObject> VisitedObjects { get; } = new List<GameObject>();

            internal List<string> AssetPaths { get; } = new List<string>();

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                VisitedObjects.Add(gameObject);
                AssetPaths.Add(assetPath);
            }
        }

        private readonly List<Scene> _createdScenes = new List<Scene>();
        private readonly List<string> _createdSceneAssetPaths = new List<string>();

        private Scene _previousActiveScene;

        [SetUp]
        public void SetUp()
        {
            _previousActiveScene = SceneManager.GetActiveScene();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = _createdScenes.Count - 1; i >= 0; i--)
            {
                Scene scene = _createdScenes[i];

                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            _createdScenes.Clear();

            for (int i = _createdSceneAssetPaths.Count - 1; i >= 0; i--)
            {
                string scenePath = _createdSceneAssetPaths[i];

                if (!string.IsNullOrEmpty(scenePath))
                {
                    AssetDatabase.DeleteAsset(scenePath);
                }
            }

            _createdSceneAssetPaths.Clear();

            if (_previousActiveScene.IsValid() && _previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(_previousActiveScene);
            }

            DeleteTestFolderIfEmpty();
        }

        [Test]
        public void Validate_WithMultipleLoadedScenes_ValidatesObjectsFromEveryScene()
        {
            Scene firstScene = CreateAdditiveScene();
            Scene secondScene = CreateAdditiveScene();

            GameObject firstRoot = CreateGameObjectInScene("FirstRoot", firstScene);
            GameObject firstChild = CreateGameObjectInScene("FirstChild", firstScene);
            firstChild.transform.SetParent(firstRoot.transform);

            GameObject secondRoot = CreateGameObjectInScene("SecondRoot", secondScene);
            GameObject secondChild = CreateGameObjectInScene("SecondChild", secondScene);
            secondChild.transform.SetParent(secondRoot.transform);

            RecordingValidationRule rule = new RecordingValidationRule();
            LoadedScenesValidationScope scope = new LoadedScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(
                new IValidationRule[] { rule },
                issues);

            Assert.AreEqual(1, rule.VisitedObjects.FindAll(item => item == firstRoot).Count);
            Assert.AreEqual(1, rule.VisitedObjects.FindAll(item => item == firstChild).Count);
            Assert.AreEqual(1, rule.VisitedObjects.FindAll(item => item == secondRoot).Count);
            Assert.AreEqual(1, rule.VisitedObjects.FindAll(item => item == secondChild).Count);
        }

        [Test]
        public void Validate_WithMultipleRoots_ValidatesEveryRootHierarchy()
        {
            Scene scene = CreateAdditiveScene();

            GameObject firstRoot = CreateGameObjectInScene("FirstRoot", scene);
            GameObject firstChild = CreateGameObjectInScene("FirstChild", scene);
            firstChild.transform.SetParent(firstRoot.transform);

            GameObject secondRoot = CreateGameObjectInScene("SecondRoot", scene);

            RecordingValidationRule rule = new RecordingValidationRule();
            LoadedScenesValidationScope scope = new LoadedScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(
                new IValidationRule[] { rule },
                issues);

            Assert.AreEqual(1, rule.VisitedObjects.FindAll(item => item == firstRoot).Count);
            Assert.AreEqual(1, rule.VisitedObjects.FindAll(item => item == firstChild).Count);
            Assert.AreEqual(1, rule.VisitedObjects.FindAll(item => item == secondRoot).Count);
        }

        [Test]
        public void Validate_WithSavedScene_UsesSceneAssetPath()
        {
            Scene scene = CreateAdditiveScene();
            GameObject root = CreateGameObjectInScene("SavedSceneRoot", scene);

            RecordingValidationRule rule = new RecordingValidationRule();
            LoadedScenesValidationScope scope = new LoadedScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(
                new IValidationRule[] { rule },
                issues);

            int index = rule.VisitedObjects.IndexOf(root);

            Assert.AreNotEqual(-1, index);
            Assert.IsFalse(string.IsNullOrEmpty(scene.path));
            Assert.AreEqual(scene.path, rule.AssetPaths[index]);
        }

        [Test]
        public void Validate_WithUnsavedScene_UsesEmptyAssetPath()
        {
            Scene unsavedScene = FindLoadedUnsavedScene();

            if (!unsavedScene.IsValid())
            {
                Assert.Ignore("현재 로드된 저장되지 않은 Scene이 없어 이 테스트를 실행할 수 없습니다.");
            }

            GameObject root = CreateGameObjectInScene("UnsavedSceneRoot", unsavedScene);

            try
            {
                RecordingValidationRule rule = new RecordingValidationRule();
                LoadedScenesValidationScope scope = new LoadedScenesValidationScope();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                scope.Validate(
                    new IValidationRule[] { rule },
                    issues);

                int index = rule.VisitedObjects.IndexOf(root);

                Assert.AreNotEqual(-1, index);
                Assert.AreEqual(string.Empty, rule.AssetPaths[index]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Validate_WithEmptyLoadedScene_DoesNotFail()
        {
            CreateAdditiveScene();

            RecordingValidationRule rule = new RecordingValidationRule();
            LoadedScenesValidationScope scope = new LoadedScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.DoesNotThrow(() => scope.Validate(
                new IValidationRule[] { rule },
                issues));
        }

        [Test]
        public void Validate_WithNullRules_ThrowsArgumentNullException()
        {
            LoadedScenesValidationScope scope = new LoadedScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(
                null,
                issues));
        }

        [Test]
        public void Validate_WithNullIssues_ThrowsArgumentNullException()
        {
            LoadedScenesValidationScope scope = new LoadedScenesValidationScope();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(
                Array.Empty<IValidationRule>(),
                null));
        }

        private Scene CreateAdditiveScene()
        {
            EnsureTestFolderExists();

            Scene sourceScene = SceneManager.GetActiveScene();
            string scenePath = $"Assets/CDGEditorToolsTests/LoadedSceneScope_{Guid.NewGuid():N}.unity";

            if (!EditorSceneManager.SaveScene(sourceScene, scenePath, true))
            {
                throw new InvalidOperationException($"테스트 Scene 복사본을 저장하지 못했습니다: {scenePath}");
            }

            _createdSceneAssetPaths.Add(scenePath);

            Scene scene = EditorSceneManager.OpenScene(
                scenePath,
                OpenSceneMode.Additive);

            _createdScenes.Add(scene);

            GameObject[] existingRoots = scene.GetRootGameObjects();

            for (int i = 0; i < existingRoots.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(existingRoots[i]);
            }

            return scene;
        }

        private static Scene FindLoadedUnsavedScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.IsValid() && scene.isLoaded && string.IsNullOrEmpty(scene.path))
                {
                    return scene;
                }
            }

            return default;
        }

        private static GameObject CreateGameObjectInScene(string name, Scene scene)
        {
            GameObject gameObject = new GameObject(name);
            SceneManager.MoveGameObjectToScene(gameObject, scene);

            return gameObject;
        }

        private static void EnsureTestFolderExists()
        {
            const string folderPath = "Assets/CDGEditorToolsTests";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(
                    "Assets",
                    "CDGEditorToolsTests");
            }
        }

        private static void DeleteTestFolderIfEmpty()
        {
            const string folderPath = "Assets/CDGEditorToolsTests";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] remainingAssets = AssetDatabase.FindAssets(
                string.Empty,
                new[] { folderPath });

            if (remainingAssets.Length == 0)
            {
                AssetDatabase.DeleteAsset(folderPath);
            }
        }
    }
}