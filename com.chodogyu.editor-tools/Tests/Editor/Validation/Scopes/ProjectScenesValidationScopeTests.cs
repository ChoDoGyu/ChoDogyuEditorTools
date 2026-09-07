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
    internal sealed class ProjectScenesValidationScopeTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";

        private sealed class RecordingValidationRule : IValidationRule
        {
            public ValidationIssueType Type => ValidationIssueType.MissingScript;

            internal List<string> VisitedObjectNames { get; } = new List<string>();

            internal List<string> AssetPaths { get; } = new List<string>();

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                VisitedObjectNames.Add(gameObject.name);
                AssetPaths.Add(assetPath);
            }
        }

        private readonly List<string> _createdScenePaths = new List<string>();
        private readonly List<Scene> _openedTestScenes = new List<Scene>();

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
            for (int i = _openedTestScenes.Count - 1; i >= 0; i--)
            {
                Scene scene = _openedTestScenes[i];

                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            _openedTestScenes.Clear();

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
        public void Validate_WithProjectScenes_ValidatesEveryCreatedScene()
        {
            const string firstScenePath = TestFolderPath + "/First.unity";
            const string secondScenePath = TestFolderPath + "/Second.unity";

            CreateSceneAsset(firstScenePath, "FirstRoot", "FirstChild");
            CreateSceneAsset(secondScenePath, "SecondRoot", "SecondChild");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.ValidateSceneAssets(new IValidationRule[] { rule }, issues);

            Assert.IsTrue(rule.AssetPaths.Contains(firstScenePath));
            Assert.IsTrue(rule.AssetPaths.Contains(secondScenePath));
            Assert.Contains("FirstRoot", rule.VisitedObjectNames);
            Assert.Contains("FirstChild", rule.VisitedObjectNames);
            Assert.Contains("SecondRoot", rule.VisitedObjectNames);
            Assert.Contains("SecondChild", rule.VisitedObjectNames);
        }

        [Test]
        public void Validate_WithSceneHierarchy_ValidatesRootAndChildren()
        {
            const string scenePath = TestFolderPath + "/Hierarchy.unity";

            CreateSceneAsset(scenePath, "Root", "Child", "GrandChild");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.ValidateSceneAssets(new IValidationRule[] { rule }, issues);

            Assert.AreEqual(3, rule.AssetPaths.FindAll(path => path == scenePath).Count);
            Assert.Contains("Root", rule.VisitedObjectNames);
            Assert.Contains("Child", rule.VisitedObjectNames);
            Assert.Contains("GrandChild", rule.VisitedObjectNames);
        }

        [Test]
        public void Validate_PassesCorrectSceneAssetPathToEveryObject()
        {
            const string scenePath = TestFolderPath + "/AssetPath.unity";

            CreateSceneAsset(scenePath, "Root", "Child");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.ValidateSceneAssets(new IValidationRule[] { rule }, issues);

            Assert.AreEqual(2, rule.AssetPaths.FindAll(path => path == scenePath).Count);
        }

        [Test]
        public void Validate_WithClosedProjectScene_DoesNotLeaveSceneLoaded()
        {
            const string scenePath = TestFolderPath + "/Temporary.unity";

            CreateSceneAsset(scenePath, "TemporaryRoot");

            Scene beforeValidation = SceneManager.GetSceneByPath(scenePath);

            Assert.IsFalse(beforeValidation.IsValid() && beforeValidation.isLoaded);

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.ValidateSceneAssets(new IValidationRule[] { rule }, issues);

            Scene afterValidation = SceneManager.GetSceneByPath(scenePath);

            Assert.IsFalse(afterValidation.IsValid() && afterValidation.isLoaded);
        }

        [Test]
        public void Validate_WithAlreadyLoadedProjectScene_DoesNotCloseExistingScene()
        {
            const string scenePath = TestFolderPath + "/AlreadyLoaded.unity";

            CreateSceneAsset(scenePath, "LoadedRoot");

            Scene loadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            _openedTestScenes.Add(loadedScene);

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.ValidateSceneAssets(new IValidationRule[] { rule }, issues);

            Scene sceneAfterValidation = SceneManager.GetSceneByPath(scenePath);

            Assert.IsTrue(sceneAfterValidation.IsValid());
            Assert.IsTrue(sceneAfterValidation.isLoaded);
            Assert.IsTrue(rule.AssetPaths.Contains(scenePath));
        }

        [Test]
        public void Validate_WithNullRules_ThrowsArgumentNullException()
        {
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(null, issues));
        }

        [Test]
        public void Validate_WithNullIssues_ThrowsArgumentNullException()
        {
            ProjectScenesValidationScope scope = new ProjectScenesValidationScope();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(Array.Empty<IValidationRule>(), null));
        }

        private void CreateSceneAsset(string scenePath, params string[] hierarchyNames)
        {
            if (hierarchyNames == null || hierarchyNames.Length == 0)
            {
                throw new ArgumentException("Scene Hierarchy에는 하나 이상의 이름이 필요합니다.", nameof(hierarchyNames));
            }

            Scene sourceScene = SceneManager.GetActiveScene();

            if (!EditorSceneManager.SaveScene(sourceScene, scenePath, true))
            {
                throw new InvalidOperationException($"테스트 Scene 복사본을 저장하지 못했습니다: {scenePath}");
            }

            _createdScenePaths.Add(scenePath);

            Scene testScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                GameObject[] existingRoots = testScene.GetRootGameObjects();

                for (int i = 0; i < existingRoots.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(existingRoots[i]);
                }

                GameObject root = new GameObject(hierarchyNames[0]);
                SceneManager.MoveGameObjectToScene(root, testScene);

                Transform parent = root.transform;

                for (int i = 1; i < hierarchyNames.Length; i++)
                {
                    GameObject child = new GameObject(hierarchyNames[i]);
                    SceneManager.MoveGameObjectToScene(child, testScene);
                    child.transform.SetParent(parent);
                    parent = child.transform;
                }

                if (!EditorSceneManager.SaveScene(testScene))
                {
                    throw new InvalidOperationException($"테스트 Scene을 저장하지 못했습니다: {scenePath}");
                }
            }
            finally
            {
                if (testScene.IsValid() && testScene.isLoaded)
                {
                    EditorSceneManager.CloseScene(testScene, true);
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