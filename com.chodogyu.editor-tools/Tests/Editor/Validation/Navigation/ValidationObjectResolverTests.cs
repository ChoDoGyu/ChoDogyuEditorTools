using System;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationObjectResolverTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";
        private const string PrefabPath = TestFolderPath + "/Resolver.prefab";

        private GameObject _prefab;
        private GameObject _child;
        private BoxCollider _component;

        [SetUp]
        public void SetUp()
        {
            EnsureTestFolderExists();

            GameObject root = new GameObject("ResolverRoot");
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
            AssetDatabase.DeleteAsset(PrefabPath);
            DeleteTestFolderIfEmpty();
        }

        [Test]
        public void Resolve_WithValidGameObjectGlobalId_ReturnsGameObject()
        {
            string globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(_child).ToString();

            UnityEngine.Object resolved = ValidationObjectResolver.Resolve(globalObjectId);

            Assert.AreSame(_child, resolved);
        }

        [Test]
        public void Resolve_WithValidComponentGlobalId_ReturnsComponent()
        {
            string globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(_component).ToString();

            UnityEngine.Object resolved = ValidationObjectResolver.Resolve(globalObjectId);

            Assert.AreSame(_component, resolved);
        }

        [Test]
        public void ResolveGameObject_WithValidationLocation_ReturnsGameObject()
        {
            ValidationLocation location = ValidationLocationFactory.Create(_child, _component, PrefabPath, string.Empty);

            GameObject resolved = ValidationObjectResolver.ResolveGameObject(location);

            Assert.AreSame(_child, resolved);
        }

        [Test]
        public void ResolveComponent_WithValidationLocation_ReturnsComponent()
        {
            ValidationLocation location = ValidationLocationFactory.Create(_child, _component, PrefabPath, string.Empty);

            Component resolved = ValidationObjectResolver.ResolveComponent(location);

            Assert.AreSame(_component, resolved);
        }

        [Test]
        public void Resolve_WithEmptyGlobalId_ReturnsNull()
        {
            Assert.IsNull(ValidationObjectResolver.Resolve(string.Empty));
        }

        [Test]
        public void Resolve_WithInvalidGlobalId_ReturnsNull()
        {
            Assert.IsNull(ValidationObjectResolver.Resolve("InvalidGlobalObjectId"));
        }

        [Test]
        public void ResolveGameObject_WithNullLocation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationObjectResolver.ResolveGameObject(null));
        }

        [Test]
        public void ResolveComponent_WithNullLocation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationObjectResolver.ResolveComponent(null));
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