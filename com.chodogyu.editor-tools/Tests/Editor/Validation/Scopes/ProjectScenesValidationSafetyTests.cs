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
    internal sealed class ProjectScenesValidationSafetyTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";

        private sealed class ThrowingValidationRule : IValidationRule
        {
            private readonly string _targetAssetPath;

            public ValidationIssueType Type => ValidationIssueType.MissingScript;

            internal ThrowingValidationRule(string targetAssetPath)
            {
                _targetAssetPath = targetAssetPath;
            }

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                if (assetPath != _targetAssetPath)
                {
                    return;
                }

                SceneManager.SetActiveScene(gameObject.scene);
                throw new InvalidOperationException("테스트 Validation 예외입니다.");
            }
        }

        private readonly List<string> _createdScenePaths = new List<string>();

        private Scene _previousActiveScene;

        [SetUp]
        public void SetUp()
        {
            _previousActiveScene = SceneManager.GetActiveScene();
            EnsureTestFolderExists();
        }

        [TearDown]
        public void TearDown()
        {
            if (_previousActiveScene.IsValid() && _previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(_previousActiveScene);
            }

            for (int i = _createdScenePaths.Count - 1; i >= 0; i--)
            {
                AssetDatabase.DeleteAsset(_createdScenePaths[i]);
            }

            _createdScenePaths.Clear();

            DeleteTestFolderIfEmpty();
        }

        [Test]
        public void EnsureSceneIsSafe_WithSavedCleanScene_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => ProjectScenesValidationScope.EnsureSceneIsSafe("Assets/Scenes/Test.unity", false));
        }

        [Test]
        public void EnsureSceneIsSafe_WithDirtyScene_ThrowsInvalidOperationException()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => ProjectScenesValidationScope.EnsureSceneIsSafe("Assets/Scenes/Test.unity", true));

            StringAssert.Contains("저장", exception.Message);
        }

        [Test]
        public void EnsureSceneIsSafe_WithUnsavedScene_ThrowsInvalidOperationException()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => ProjectScenesValidationScope.EnsureSceneIsSafe(string.Empty, false));

            StringAssert.Contains("저장", exception.Message);
        }

        [Test]
        public void ValidateSceneAssets_WhenRuleThrows_ClosesTemporarySceneAndRestoresActiveScene()
        {
            const string scenePath = TestFolderPath + "/ExceptionRestore.unity";

            CreateSceneAsset(scenePath, "ExceptionRoot");

            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.Throws<InvalidOperationException>(() => scope.ValidateSceneAssets(new IValidationRule[] { new ThrowingValidationRule(scenePath) }, issues));

            Scene temporaryScene = SceneManager.GetSceneByPath(scenePath);

            Assert.IsFalse(temporaryScene.IsValid() && temporaryScene.isLoaded);
            Assert.IsTrue(SceneManager.GetActiveScene() == _previousActiveScene);
        }

        private void CreateSceneAsset(string scenePath, string rootName)
        {
            Scene sourceScene = SceneManager.GetActiveScene();

            if (!EditorSceneManager.SaveScene(sourceScene, scenePath, true))
            {
                throw new InvalidOperationException($"테스트 Scene 복사본을 저장하지 못했습니다: {scenePath}");
            }

            _createdScenePaths.Add(scenePath);

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

        private static void EnsureTestFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "CDGEditorToolsTests");
            }
        }

        private static void DeleteTestFolderIfEmpty()
        {
            if (!AssetDatabase.IsValidFolder(TestFolderPath))
            {
                return;
            }

            string[] remainingAssets = AssetDatabase.FindAssets(string.Empty, new[] { TestFolderPath });

            if (remainingAssets.Length == 0)
            {
                AssetDatabase.DeleteAsset(TestFolderPath);
            }
        }
    }
}