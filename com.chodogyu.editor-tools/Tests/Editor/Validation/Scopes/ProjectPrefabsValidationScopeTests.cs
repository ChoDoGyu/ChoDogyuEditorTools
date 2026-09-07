using System;
using System.Collections.Generic;
using System.Linq;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using CDG.EditorTools.Validation.Scopes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ProjectPrefabsValidationScopeTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";

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

        [SetUp]
        public void SetUp()
        {
            EnsureTestFolderExists();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTestFolder();
        }

        [Test]
        public void Validate_WithProjectPrefabs_ValidatesEveryCreatedPrefab()
        {
            const string firstPrefabPath = TestFolderPath + "/First.prefab";
            const string secondPrefabPath = TestFolderPath + "/Second.prefab";

            CreatePrefab(
                firstPrefabPath,
                "First",
                "FirstChild");

            CreatePrefab(
                secondPrefabPath,
                "Second",
                "SecondChild");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(
                new IValidationRule[] { rule },
                issues);

            Assert.IsTrue(rule.AssetPaths.Contains(firstPrefabPath));
            Assert.IsTrue(rule.AssetPaths.Contains(secondPrefabPath));

            Assert.IsTrue(rule.VisitedObjects.Any(item => item.name == "First"));
            Assert.IsTrue(rule.VisitedObjects.Any(item => item.name == "FirstChild"));
            Assert.IsTrue(rule.VisitedObjects.Any(item => item.name == "Second"));
            Assert.IsTrue(rule.VisitedObjects.Any(item => item.name == "SecondChild"));
        }

        [Test]
        public void Validate_WithPrefabHierarchy_ValidatesRootAndChildren()
        {
            const string prefabPath = TestFolderPath + "/Hierarchy.prefab";

            CreatePrefab(
                prefabPath,
                "Hierarchy",
                "Child",
                "GrandChild");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(
                new IValidationRule[] { rule },
                issues);

            Assert.AreEqual(
                3,
                rule.AssetPaths.FindAll(path => path == prefabPath).Count);

            Assert.IsTrue(rule.VisitedObjects.Any(item => item.name == "Hierarchy"));
            Assert.IsTrue(rule.VisitedObjects.Any(item => item.name == "Child"));
            Assert.IsTrue(rule.VisitedObjects.Any(item => item.name == "GrandChild"));
        }

        [Test]
        public void Validate_PassesCorrectPrefabAssetPathToEveryObject()
        {
            const string prefabPath = TestFolderPath + "/AssetPath.prefab";

            CreatePrefab(
                prefabPath,
                "AssetPath",
                "Child");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(
                new IValidationRule[] { rule },
                issues);

            List<int> matchingIndexes = new List<int>();

            for (int i = 0; i < rule.AssetPaths.Count; i++)
            {
                if (rule.AssetPaths[i] == prefabPath)
                {
                    matchingIndexes.Add(i);
                }
            }

            Assert.AreEqual(2, matchingIndexes.Count);

            for (int i = 0; i < matchingIndexes.Count; i++)
            {
                Assert.AreEqual(
                    prefabPath,
                    rule.AssetPaths[matchingIndexes[i]]);
            }
        }

        [Test]
        public void Validate_WithNonPrefabAsset_DoesNotValidateAsset()
        {
            const string meshPath = TestFolderPath + "/IgnoredMesh.asset";

            Mesh mesh = new Mesh
            {
                name = "IgnoredMesh"
            };

            AssetDatabase.CreateAsset(mesh, meshPath);

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.Validate(
                new IValidationRule[] { rule },
                issues);

            Assert.IsFalse(rule.AssetPaths.Contains(meshPath));
            Assert.IsFalse(rule.VisitedObjects.Any(item => item.name == "IgnoredMesh"));
        }

        [Test]
        public void Validate_WithActualMissingScriptPrefab_FindsIssue()
        {
            string assetPath = null;

            try
            {
                MissingScriptPrefabFixture.Create(
                    "ProjectScopeMissingScript",
                    1,
                    true,
                    out assetPath);

                ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                scope.Validate(
                    new IValidationRule[]
                    {
                        new MissingScriptRule()
                    },
                    issues);

                Assert.IsTrue(issues.Any(issue =>
                    issue.Type == ValidationIssueType.MissingScript
                    && issue.Location.AssetPath == assetPath));
            }
            finally
            {
                MissingScriptPrefabFixture.Delete(assetPath);
                EnsureTestFolderExists();
            }
        }

        [Test]
        public void Validate_WithNullRules_ThrowsArgumentNullException()
        {
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(
                null,
                issues));
        }

        [Test]
        public void Validate_WithNullIssues_ThrowsArgumentNullException()
        {
            ProjectPrefabsValidationScope scope = new ProjectPrefabsValidationScope();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(
                Array.Empty<IValidationRule>(),
                null));
        }

        private static void CreatePrefab(string assetPath, params string[] hierarchyNames)
        {
            if (hierarchyNames == null || hierarchyNames.Length == 0)
            {
                throw new ArgumentException("Prefab Hierarchy에는 하나 이상의 이름이 필요합니다.", nameof(hierarchyNames));
            }

            GameObject root = new GameObject(hierarchyNames[0]);

            try
            {
                Transform parent = root.transform;

                for (int i = 1; i < hierarchyNames.Length; i++)
                {
                    GameObject child = new GameObject(hierarchyNames[i]);
                    child.transform.SetParent(parent);
                    parent = child.transform;
                }

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    assetPath);

                Assert.IsNotNull(prefab);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void EnsureTestFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.CreateFolder(
                    "Assets",
                    "CDGEditorToolsTests");
            }
        }

        private static void DeleteTestFolder()
        {
            if (AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.DeleteAsset(TestFolderPath);
            }
        }
    }
}