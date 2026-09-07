using System;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationResultNavigatorTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";
        private const string PrefabPath = TestFolderPath + "/ResultNavigator.prefab";

        private UnityEngine.Object _previousSelection;
        private GameObject _prefab;
        private GameObject _child;

        [SetUp]
        public void SetUp()
        {
            StageUtility.GoToMainStage();

            _previousSelection = Selection.activeObject;
            Selection.activeObject = null;

            EnsureTestFolderExists();

            GameObject root = new GameObject("ResultNavigator");
            GameObject child = new GameObject("Child");

            try
            {
                child.transform.SetParent(root.transform);

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Assert.IsNotNull(savedPrefab);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            _child = _prefab.transform.Find("Child").gameObject;

            Assert.IsNotNull(_prefab);
            Assert.IsNotNull(_child);
        }

        [TearDown]
        public void TearDown()
        {
            StageUtility.GoToMainStage();
            Selection.activeObject = _previousSelection;

            AssetDatabase.DeleteAsset(PrefabPath);
            DeleteTestFolderIfEmpty();
        }

        [Test]
        public void GetNavigationTarget_WithPrefabPath_ReturnsPrefab()
        {
            ValidationLocation location = CreateLocation("Assets/Test.prefab");

            Assert.AreEqual(ValidationNavigationTarget.Prefab, ValidationResultNavigator.GetNavigationTarget(location));
        }

        [Test]
        public void GetNavigationTarget_WithScenePath_ReturnsScene()
        {
            ValidationLocation location = CreateLocation("Assets/Test.unity");

            Assert.AreEqual(ValidationNavigationTarget.Scene, ValidationResultNavigator.GetNavigationTarget(location));
        }

        [Test]
        public void GetNavigationTarget_WithEmptyAssetPath_ReturnsLoadedObject()
        {
            ValidationLocation location = CreateLocation(string.Empty);

            Assert.AreEqual(ValidationNavigationTarget.LoadedObject, ValidationResultNavigator.GetNavigationTarget(location));
        }

        [Test]
        public void TryNavigate_WithResolvableLoadedObject_SelectsObject()
        {
            string globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(_child).ToString();

            ValidationLocation location = new ValidationLocation(
                string.Empty,
                "Child",
                string.Empty,
                string.Empty,
                globalObjectId,
                string.Empty);

            ValidationIssue issue = CreateIssue(location);

            bool result = ValidationResultNavigator.TryNavigate(issue);

            Assert.IsTrue(result);
            Assert.AreSame(_child, Selection.activeGameObject);
        }

        [Test]
        public void TryNavigate_WithPrefabIssue_OpensPrefabAndSelectsChild()
        {
            ValidationLocation location = ValidationLocationFactory.Create(_child, null, PrefabPath, string.Empty);
            ValidationIssue issue = CreateIssue(location);

            bool result = ValidationResultNavigator.TryNavigate(issue);

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();

            Assert.IsTrue(result);
            Assert.IsNotNull(stage);
            Assert.AreEqual(PrefabPath, stage.assetPath);
            Assert.IsNotNull(Selection.activeGameObject);
            Assert.AreEqual("Child", Selection.activeGameObject.name);
        }

        [Test]
        public void TryNavigate_WithMissingSceneAsset_ReturnsFalse()
        {
            ValidationLocation location = new ValidationLocation(
                "Assets/DoesNotExist.unity",
                "Root",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            ValidationIssue issue = CreateIssue(location);

            Assert.IsFalse(ValidationResultNavigator.TryNavigate(issue));
        }

        [Test]
        public void TryNavigate_WithInvalidLoadedObject_ReturnsFalseWithoutChangingSelection()
        {
            Selection.activeObject = _prefab;

            ValidationLocation location = new ValidationLocation(
                string.Empty,
                "Missing",
                string.Empty,
                string.Empty,
                "InvalidGlobalObjectId",
                string.Empty);

            ValidationIssue issue = CreateIssue(location);

            bool result = ValidationResultNavigator.TryNavigate(issue);

            Assert.IsFalse(result);
            Assert.AreSame(_prefab, Selection.activeObject);
        }

        [Test]
        public void TryNavigate_WithNullIssue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationResultNavigator.TryNavigate(null));
        }

        private static ValidationLocation CreateLocation(string assetPath)
        {
            return new ValidationLocation(
                assetPath,
                "Root",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        private static ValidationIssue CreateIssue(ValidationLocation location)
        {
            return new ValidationIssue(
                ValidationIssueType.MissingScript,
                "테스트 문제입니다.",
                location);
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