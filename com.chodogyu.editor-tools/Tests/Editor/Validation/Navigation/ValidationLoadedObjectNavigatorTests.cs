using System;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationLoadedObjectNavigatorTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";
        private const string PrefabPath = TestFolderPath + "/LoadedObjectNavigator.prefab";

        private UnityEngine.Object _previousSelection;
        private GameObject _prefab;
        private GameObject _child;
        private BoxCollider _component;

        [SetUp]
        public void SetUp()
        {
            _previousSelection = Selection.activeObject;
            Selection.activeObject = null;

            EnsureTestFolderExists();

            GameObject root = new GameObject("LoadedObjectNavigator");
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
            Selection.activeObject = _previousSelection;

            AssetDatabase.DeleteAsset(PrefabPath);
            DeleteTestFolderIfEmpty();
        }

        [Test]
        public void TrySelectAndPing_WithResolvableGameObject_SelectsObjectAndReturnsTrue()
        {
            ValidationLocation location = ValidationLocationFactory.Create(_child, null, PrefabPath, string.Empty);

            bool result = ValidationLoadedObjectNavigator.TrySelectAndPing(location);

            Assert.IsTrue(result);
            Assert.AreSame(_child, Selection.activeGameObject);
        }

        [Test]
        public void TrySelectAndPing_WithComponentLocation_SelectsOwningGameObject()
        {
            ValidationLocation location = ValidationLocationFactory.Create(_child, _component, PrefabPath, "m_Enabled");

            bool result = ValidationLoadedObjectNavigator.TrySelectAndPing(location);

            Assert.IsTrue(result);
            Assert.AreSame(_child, Selection.activeGameObject);
        }

        [Test]
        public void TrySelectAndPing_WithEmptyObjectGlobalId_ReturnsFalseWithoutChangingSelection()
        {
            Selection.activeObject = _prefab;

            ValidationLocation location = new ValidationLocation(
                string.Empty,
                "Missing",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            bool result = ValidationLoadedObjectNavigator.TrySelectAndPing(location);

            Assert.IsFalse(result);
            Assert.AreSame(_prefab, Selection.activeObject);
        }

        [Test]
        public void TrySelectAndPing_WithInvalidObjectGlobalId_ReturnsFalseWithoutChangingSelection()
        {
            Selection.activeObject = _prefab;

            ValidationLocation location = new ValidationLocation(
                string.Empty,
                "Missing",
                string.Empty,
                string.Empty,
                "InvalidGlobalObjectId",
                string.Empty);

            bool result = ValidationLoadedObjectNavigator.TrySelectAndPing(location);

            Assert.IsFalse(result);
            Assert.AreSame(_prefab, Selection.activeObject);
        }

        [Test]
        public void TrySelectAndPing_WithNullLocation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationLoadedObjectNavigator.TrySelectAndPing(null));
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