using System;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationPrefabNavigatorTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";
        private const string PrefabPath = TestFolderPath + "/NavigationPrefab.prefab";

        private UnityEngine.Object _previousSelection;
        private GameObject _prefab;
        private GameObject _child;
        private BoxCollider _component;

        [SetUp]
        public void SetUp()
        {
            StageUtility.GoToMainStage();

            _previousSelection = Selection.activeObject;
            Selection.activeObject = null;

            EnsureTestFolderExists();

            GameObject root = new GameObject("NavigationPrefab");
            GameObject child = new GameObject("Child");

            try
            {
                child.transform.SetParent(root.transform);
                child.AddComponent<BoxCollider>();

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Assert.IsNotNull(savedPrefab);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            _child = _prefab.transform.Find("Child").gameObject;
            _component = _child.GetComponent<BoxCollider>();

            Assert.IsNotNull(_prefab);
            Assert.IsNotNull(_child);
            Assert.IsNotNull(_component);
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
        public void TryOpenAndSelect_WithPrefabChildLocation_OpensPrefabAndSelectsChild()
        {
            ValidationLocation location = ValidationLocationFactory.Create(_child, null, PrefabPath, string.Empty);

            bool result = ValidationPrefabNavigator.TryOpenAndSelect(location);

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();

            Assert.IsTrue(result);
            Assert.IsNotNull(stage);
            Assert.AreEqual(PrefabPath, stage.assetPath);
            Assert.AreEqual("Child", Selection.activeGameObject.name);
            Assert.IsTrue(stage.IsPartOfPrefabContents(Selection.activeGameObject));
        }

        [Test]
        public void TryOpenAndSelect_WithComponentLocation_SelectsOwningGameObject()
        {
            ValidationLocation location = ValidationLocationFactory.Create(_child, _component, PrefabPath, "m_Enabled");

            bool result = ValidationPrefabNavigator.TryOpenAndSelect(location);

            Assert.IsTrue(result);
            Assert.AreEqual("Child", Selection.activeGameObject.name);
        }

        [Test]
        public void TryOpenAndSelect_WithPrefabRootLocation_SelectsPrefabRoot()
        {
            ValidationLocation location = ValidationLocationFactory.Create(_prefab, null, PrefabPath, string.Empty);

            bool result = ValidationPrefabNavigator.TryOpenAndSelect(location);

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();

            Assert.IsTrue(result);
            Assert.IsNotNull(stage);
            Assert.AreSame(stage.prefabContentsRoot, Selection.activeGameObject);
        }

        [Test]
        public void TryOpenAndSelect_WithNonPrefabPath_ReturnsFalseWithoutOpeningPrefabStage()
        {
            ValidationLocation location = new ValidationLocation(
                "Assets/Test.unity",
                "Root",
                string.Empty,
                string.Empty,
                _child != null ? GlobalObjectId.GetGlobalObjectIdSlow(_child).ToString() : string.Empty,
                string.Empty);

            bool result = ValidationPrefabNavigator.TryOpenAndSelect(location);

            Assert.IsFalse(result);
            Assert.IsNull(PrefabStageUtility.GetCurrentPrefabStage());
        }

        [Test]
        public void TryOpenAndSelect_WithInvalidGlobalObjectId_ReturnsFalseWithoutOpeningPrefabStage()
        {
            ValidationLocation location = new ValidationLocation(
                PrefabPath,
                "NavigationPrefab/Child",
                string.Empty,
                string.Empty,
                "InvalidGlobalObjectId",
                string.Empty);

            bool result = ValidationPrefabNavigator.TryOpenAndSelect(location);

            Assert.IsFalse(result);
            Assert.IsNull(PrefabStageUtility.GetCurrentPrefabStage());
        }

        [Test]
        public void TryOpenAndSelect_WithNullLocation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationPrefabNavigator.TryOpenAndSelect(null));
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