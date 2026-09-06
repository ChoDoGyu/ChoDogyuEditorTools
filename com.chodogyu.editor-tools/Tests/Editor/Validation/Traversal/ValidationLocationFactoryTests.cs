using System;
using CDG.EditorTools.Validation;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationLocationFactoryTests
    {
        private sealed class TestComponent : MonoBehaviour
        {
        }

        [Test]
        public void Create_WithGameObjectOnly_CreatesLocationWithoutComponentInformation()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");

            try
            {
                child.transform.SetParent(root.transform);

                ValidationLocation location = ValidationLocationFactory.Create(
                    child,
                    null,
                    "Assets/Scenes/Test.unity",
                    string.Empty);

                Assert.AreEqual("Assets/Scenes/Test.unity", location.AssetPath);
                Assert.AreEqual("Root/Child", location.ObjectPath);
                Assert.AreEqual(string.Empty, location.ComponentName);
                Assert.AreEqual(string.Empty, location.PropertyPath);
                Assert.IsNotNull(location.ObjectGlobalId);
                Assert.AreEqual(string.Empty, location.ComponentGlobalId);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Create_WithComponent_CreatesComponentInformation()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                TestComponent component = gameObject.AddComponent<TestComponent>();

                ValidationLocation location = ValidationLocationFactory.Create(
                    gameObject,
                    component,
                    "Assets/Prefabs/Enemy.prefab",
                    "target");

                Assert.AreEqual("Assets/Prefabs/Enemy.prefab", location.AssetPath);
                Assert.AreEqual("Enemy", location.ObjectPath);
                Assert.AreEqual(nameof(TestComponent), location.ComponentName);
                Assert.AreEqual("target", location.PropertyPath);
                Assert.IsNotNull(location.ObjectGlobalId);
                Assert.IsNotNull(location.ComponentGlobalId);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Create_WithComponentFromDifferentGameObject_ThrowsArgumentException()
        {
            GameObject first = new GameObject("First");
            GameObject second = new GameObject("Second");

            try
            {
                TestComponent component = second.AddComponent<TestComponent>();

                Assert.Throws<ArgumentException>(() => ValidationLocationFactory.Create(
                    first,
                    component,
                    string.Empty,
                    string.Empty));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Create_WithNullGameObject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationLocationFactory.Create(
                null,
                null,
                string.Empty,
                string.Empty));
        }

        [Test]
        public void Create_WithNullAssetPath_ThrowsArgumentNullException()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                Assert.Throws<ArgumentNullException>(() => ValidationLocationFactory.Create(
                    gameObject,
                    null,
                    null,
                    string.Empty));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Create_WithNullPropertyPath_ThrowsArgumentNullException()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                Assert.Throws<ArgumentNullException>(() => ValidationLocationFactory.Create(
                    gameObject,
                    null,
                    string.Empty,
                    null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}