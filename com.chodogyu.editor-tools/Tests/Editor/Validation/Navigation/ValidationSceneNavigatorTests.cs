using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationSceneNavigatorTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";
        private const string ScenePath = TestFolderPath + "/NavigationScene.unity";

        private readonly List<Scene> _openedScenes = new List<Scene>();

        private UnityEngine.Object _previousSelection;
        private Scene _previousActiveScene;
        private ValidationLocation _childLocation;
        private ValidationLocation _componentLocation;

        [SetUp]
        public void SetUp()
        {
            _previousSelection = Selection.activeObject;
            _previousActiveScene = SceneManager.GetActiveScene();

            Selection.activeObject = null;

            EnsureTestFolderExists();
            CreateTestScene();
        }

        [TearDown]
        public void TearDown()
        {
            Scene navigationScene = SceneManager.GetSceneByPath(ScenePath);

            if (navigationScene.IsValid() && navigationScene.isLoaded)
            {
                EditorSceneManager.CloseScene(navigationScene, true);
            }

            for (int i = _openedScenes.Count - 1; i >= 0; i--)
            {
                Scene scene = _openedScenes[i];

                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            _openedScenes.Clear();

            if (_previousActiveScene.IsValid() && _previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(_previousActiveScene);
            }

            Selection.activeObject = _previousSelection;

            AssetDatabase.DeleteAsset(ScenePath);
            DeleteTestFolderIfEmpty();
        }

        [Test]
        public void TryOpenSceneAndSelect_WithClosedScene_OpensSceneAndSelectsChild()
        {
            Assert.IsFalse(IsSceneLoaded(ScenePath));

            bool result = ValidationSceneNavigator.TryOpenSceneAndSelect(_childLocation);

            Scene scene = SceneManager.GetSceneByPath(ScenePath);

            Assert.IsTrue(result);
            Assert.IsTrue(scene.IsValid());
            Assert.IsTrue(scene.isLoaded);
            Assert.IsNotNull(Selection.activeGameObject);
            Assert.AreEqual("Child", Selection.activeGameObject.name);
            Assert.AreEqual(scene, Selection.activeGameObject.scene);
        }

        [Test]
        public void TryOpenAndSelect_WithAlreadyLoadedScene_SelectsChildWithoutClosingScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            _openedScenes.Add(scene);

            bool result = ValidationSceneNavigator.TryOpenAndSelect(_childLocation);

            Assert.IsTrue(result);
            Assert.IsTrue(scene.isLoaded);
            Assert.AreEqual("Child", Selection.activeGameObject.name);
            Assert.AreEqual(scene, Selection.activeGameObject.scene);
        }

        [Test]
        public void TryOpenSceneAndSelect_WithComponentLocation_SelectsOwningGameObject()
        {
            bool result = ValidationSceneNavigator.TryOpenSceneAndSelect(_componentLocation);

            Assert.IsTrue(result);
            Assert.IsNotNull(Selection.activeGameObject);
            Assert.AreEqual("Child", Selection.activeGameObject.name);
        }

        [Test]
        public void TryOpenSceneAndSelect_WithInvalidGlobalId_UsesHierarchyPathFallback()
        {
            ValidationLocation fallbackLocation = new ValidationLocation(
                ScenePath,
                "Root/Child",
                string.Empty,
                string.Empty,
                "InvalidGlobalObjectId",
                string.Empty);

            bool result = ValidationSceneNavigator.TryOpenSceneAndSelect(fallbackLocation);

            Assert.IsTrue(result);
            Assert.AreEqual("Child", Selection.activeGameObject.name);
        }

        [Test]
        public void TryOpenSceneAndSelect_WithMissingObject_ReturnsFalseAndClosesOpenedScene()
        {
            ValidationLocation location = new ValidationLocation(
                ScenePath,
                "Root/DoesNotExist",
                string.Empty,
                string.Empty,
                "InvalidGlobalObjectId",
                string.Empty);

            bool result = ValidationSceneNavigator.TryOpenSceneAndSelect(location);

            Assert.IsFalse(result);
            Assert.IsFalse(IsSceneLoaded(ScenePath));
        }

        [Test]
        public void TryOpenAndSelect_WithNonScenePath_ReturnsFalse()
        {
            ValidationLocation location = new ValidationLocation(
                "Assets/Test.prefab",
                "Root",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            Assert.IsFalse(ValidationSceneNavigator.TryOpenAndSelect(location));
        }

        [Test]
        public void TryOpenAndSelect_WithNullLocation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationSceneNavigator.TryOpenAndSelect(null));
        }

        private void CreateTestScene()
        {
            Scene sourceScene = SceneManager.GetActiveScene();

            if (!EditorSceneManager.SaveScene(sourceScene, ScenePath, true))
            {
                throw new InvalidOperationException($"테스트 Scene 복사본을 저장하지 못했습니다: {ScenePath}");
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            try
            {
                GameObject[] existingRoots = scene.GetRootGameObjects();

                for (int i = 0; i < existingRoots.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(existingRoots[i]);
                }

                GameObject root = new GameObject("Root");
                GameObject child = new GameObject("Child");

                SceneManager.MoveGameObjectToScene(root, scene);
                SceneManager.MoveGameObjectToScene(child, scene);

                child.transform.SetParent(root.transform);

                BoxCollider component = child.AddComponent<BoxCollider>();

                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException($"테스트 Scene을 저장하지 못했습니다: {ScenePath}");
                }

                _childLocation = ValidationLocationFactory.Create(child, null, ScenePath, string.Empty);
                _componentLocation = ValidationLocationFactory.Create(child, component, ScenePath, "m_Enabled");
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