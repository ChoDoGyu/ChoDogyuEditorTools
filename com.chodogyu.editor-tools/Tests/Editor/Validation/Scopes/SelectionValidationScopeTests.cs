using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using CDG.EditorTools.Validation.Scopes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class SelectionValidationScopeTests
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

        private UnityEngine.Object[] _previousSelection;

        [SetUp]
        public void SetUp()
        {
            _previousSelection = Selection.objects;
            Selection.objects = Array.Empty<UnityEngine.Object>();
        }

        [TearDown]
        public void TearDown()
        {
            Selection.objects = _previousSelection;
        }

        [Test]
        public void Validate_WithSelectedSceneGameObject_ValidatesSelectedHierarchy()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");

            try
            {
                child.transform.SetParent(root.transform);
                Selection.objects = new UnityEngine.Object[] { root };

                RecordingValidationRule rule = new RecordingValidationRule();
                SelectionValidationScope scope = new SelectionValidationScope();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                scope.Validate(
                    new IValidationRule[] { rule },
                    issues);

                Assert.AreEqual(2, rule.VisitedObjects.Count);
                Assert.AreSame(root, rule.VisitedObjects[0]);
                Assert.AreSame(child, rule.VisitedObjects[1]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Validate_WithSelectedParentAndChild_DoesNotValidateChildTwice()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");

            try
            {
                child.transform.SetParent(root.transform);

                Selection.objects = new UnityEngine.Object[]
                {
                    root,
                    child
                };

                RecordingValidationRule rule = new RecordingValidationRule();
                SelectionValidationScope scope = new SelectionValidationScope();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                scope.Validate(
                    new IValidationRule[] { rule },
                    issues);

                Assert.AreEqual(2, rule.VisitedObjects.Count);
                Assert.AreSame(root, rule.VisitedObjects[0]);
                Assert.AreSame(child, rule.VisitedObjects[1]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Validate_WithSelectedComponent_ValidatesOwningGameObject()
        {
            GameObject gameObject = new GameObject("SelectedObject");

            try
            {
                BoxCollider component = gameObject.AddComponent<BoxCollider>();
                Selection.objects = new UnityEngine.Object[] { component };

                RecordingValidationRule rule = new RecordingValidationRule();
                SelectionValidationScope scope = new SelectionValidationScope();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                scope.Validate(
                    new IValidationRule[] { rule },
                    issues);

                Assert.AreEqual(1, rule.VisitedObjects.Count);
                Assert.AreSame(gameObject, rule.VisitedObjects[0]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WithSelectedPrefabAsset_UsesPrefabAssetPath()
        {
            const string folderPath = "Assets/CDGEditorToolsTests";
            const string prefabPath = folderPath + "/SelectionScopePrefab.prefab";

            GameObject root = new GameObject("SelectionScopePrefab");

            try
            {
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "CDGEditorToolsTests");
                }

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

                Assert.IsNotNull(savedPrefab);

                Selection.objects = new UnityEngine.Object[] { savedPrefab };

                RecordingValidationRule rule = new RecordingValidationRule();
                SelectionValidationScope scope = new SelectionValidationScope();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                scope.Validate(
                    new IValidationRule[] { rule },
                    issues);

                Assert.AreEqual(1, rule.VisitedObjects.Count);
                Assert.AreEqual(prefabPath, rule.AssetPaths[0]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                AssetDatabase.DeleteAsset(prefabPath);

                if (AssetDatabase.IsValidFolder(folderPath))
                {
                    string[] remainingAssets = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });

                    if (remainingAssets.Length == 0)
                    {
                        AssetDatabase.DeleteAsset(folderPath);
                    }
                }
            }
        }

        [Test]
        public void Validate_WithUnsupportedAssetSelection_DoesNothing()
        {
            const string folderPath = "Assets/CDGEditorToolsTests";
            const string assetPath = folderPath + "/SelectionScopeMesh.asset";

            try
            {
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "CDGEditorToolsTests");
                }

                Mesh mesh = new Mesh();
                AssetDatabase.CreateAsset(mesh, assetPath);

                Selection.objects = new UnityEngine.Object[] { mesh };

                RecordingValidationRule rule = new RecordingValidationRule();
                SelectionValidationScope scope = new SelectionValidationScope();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                scope.Validate(
                    new IValidationRule[] { rule },
                    issues);

                Assert.AreEqual(0, rule.VisitedObjects.Count);
                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                AssetDatabase.DeleteAsset(assetPath);

                if (AssetDatabase.IsValidFolder(folderPath))
                {
                    string[] remainingAssets = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });

                    if (remainingAssets.Length == 0)
                    {
                        AssetDatabase.DeleteAsset(folderPath);
                    }
                }
            }
        }

        [Test]
        public void Validate_WithNoSelection_DoesNothing()
        {
            Selection.objects = Array.Empty<UnityEngine.Object>();

            RecordingValidationRule rule = new RecordingValidationRule();
            SelectionValidationScope scope = new SelectionValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(
                new IValidationRule[] { rule },
                issues);

            Assert.AreEqual(0, rule.VisitedObjects.Count);
            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void Validate_WithNullRules_ThrowsArgumentNullException()
        {
            SelectionValidationScope scope = new SelectionValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(
                null,
                issues));
        }

        [Test]
        public void Validate_WithNullIssues_ThrowsArgumentNullException()
        {
            SelectionValidationScope scope = new SelectionValidationScope();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(
                Array.Empty<IValidationRule>(),
                null));
        }
    }
}