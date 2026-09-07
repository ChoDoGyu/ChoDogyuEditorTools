using System;
using System.Collections.Generic;
using System.Linq;
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
    internal sealed class ProjectValidationScopeTests
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";

        private sealed class RecordingValidationRule : IValidationRule
        {
            public ValidationIssueType Type => ValidationIssueType.MissingScript;

            internal List<string> ObjectNames { get; } = new List<string>();

            internal List<string> AssetPaths { get; } = new List<string>();

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                ObjectNames.Add(gameObject.name);
                AssetPaths.Add(assetPath);
            }
        }

        private readonly List<string> _createdAssetPaths = new List<string>();

        [SetUp]
        public void SetUp()
        {
            EnsureTestFolderExists();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = _createdAssetPaths.Count - 1; i >= 0; i--)
            {
                AssetDatabase.DeleteAsset(_createdAssetPaths[i]);
            }

            _createdAssetPaths.Clear();

            DeleteTestFolderIfEmpty();
        }

        [Test]
        public void Validate_WithPrefabAndScene_ValidatesBothAssetTypes()
        {
            const string prefabPath = TestFolderPath + "/ProjectScope.prefab";
            const string scenePath = TestFolderPath + "/ProjectScope.unity";

            CreatePrefab(prefabPath, "ProjectPrefabRoot");
            CreateScene(scenePath, "ProjectSceneRoot");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectValidationScope scope = new ProjectValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.ValidateProjectAssets(new IValidationRule[] { rule }, issues);

            Assert.IsTrue(rule.AssetPaths.Contains(prefabPath));
            Assert.IsTrue(rule.AssetPaths.Contains(scenePath));
            Assert.Contains("ProjectScope", rule.ObjectNames);
            Assert.Contains("ProjectSceneRoot", rule.ObjectNames);
        }

        [Test]
        public void Validate_WithPrefabHierarchy_ValidatesPrefabChildren()
        {
            const string prefabPath = TestFolderPath + "/ProjectHierarchy.prefab";

            CreatePrefab(prefabPath, "PrefabRoot", "PrefabChild");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectValidationScope scope = new ProjectValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.ValidateProjectAssets(new IValidationRule[] { rule }, issues);

            Assert.Contains("ProjectHierarchy", rule.ObjectNames);
            Assert.Contains("PrefabChild", rule.ObjectNames);
            Assert.AreEqual(2, rule.AssetPaths.Count(path => path == prefabPath));
        }

        [Test]
        public void Validate_WithSceneHierarchy_ValidatesSceneChildren()
        {
            const string scenePath = TestFolderPath + "/ProjectSceneHierarchy.unity";

            CreateScene(scenePath, "SceneRoot", "SceneChild");

            RecordingValidationRule rule = new RecordingValidationRule();
            ProjectValidationScope scope = new ProjectValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            scope.ValidateProjectAssets(new IValidationRule[] { rule }, issues);

            Assert.Contains("SceneRoot", rule.ObjectNames);
            Assert.Contains("SceneChild", rule.ObjectNames);
            Assert.AreEqual(2, rule.AssetPaths.Count(path => path == scenePath));
        }

        [Test]
        public void Validate_WithActualRules_AccumulatesPrefabAndSceneIssues()
        {
            string missingScriptPrefabPath = null;
            const string brokenScenePath = TestFolderPath + "/BrokenReferenceScene.unity";

            try
            {
                MissingScriptPrefabFixture.Create("ProjectCombinedMissingScript", 1, false, out missingScriptPrefabPath);
                CreateBrokenReferenceScene(brokenScenePath);

                ProjectValidationScope scope = new ProjectValidationScope();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                scope.ValidateProjectAssets(
                    new IValidationRule[]
                    {
                        new MissingScriptRule(),
                        new BrokenSerializedReferenceRule()
                    },
                    issues);

                Assert.IsTrue(issues.Any(issue => issue.Type == ValidationIssueType.MissingScript && issue.Location.AssetPath == missingScriptPrefabPath));
                Assert.IsTrue(issues.Any(issue => issue.Type == ValidationIssueType.BrokenSerializedReference && issue.Location.AssetPath == brokenScenePath));
            }
            finally
            {
                MissingScriptPrefabFixture.Delete(missingScriptPrefabPath);
                EnsureTestFolderExists();
            }
        }

        [Test]
        public void Validate_WithNullRules_ThrowsArgumentNullException()
        {
            ProjectValidationScope scope = new ProjectValidationScope();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(null, issues));
        }

        [Test]
        public void Validate_WithNullIssues_ThrowsArgumentNullException()
        {
            ProjectValidationScope scope = new ProjectValidationScope();

            Assert.Throws<ArgumentNullException>(() => scope.Validate(Array.Empty<IValidationRule>(), null));
        }

        private void CreatePrefab(string assetPath, params string[] hierarchyNames)
        {
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

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                Assert.IsNotNull(prefab);

                _createdAssetPaths.Add(assetPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private void CreateScene(string scenePath, params string[] hierarchyNames)
        {
            Scene sourceScene = SceneManager.GetActiveScene();

            if (!EditorSceneManager.SaveScene(sourceScene, scenePath, true))
            {
                throw new InvalidOperationException($"테스트 Scene 복사본을 저장하지 못했습니다: {scenePath}");
            }

            _createdAssetPaths.Add(scenePath);

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

        private void CreateBrokenReferenceScene(string scenePath)
        {
            const string referenceAssetPath = TestFolderPath + "/DeletedMesh.asset";

            Mesh mesh = new Mesh
            {
                name = "DeletedMesh"
            };

            AssetDatabase.CreateAsset(mesh, referenceAssetPath);

            Scene sourceScene = SceneManager.GetActiveScene();

            if (!EditorSceneManager.SaveScene(sourceScene, scenePath, true))
            {
                throw new InvalidOperationException($"테스트 Scene 복사본을 저장하지 못했습니다: {scenePath}");
            }

            _createdAssetPaths.Add(scenePath);

            Scene testScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                GameObject[] existingRoots = testScene.GetRootGameObjects();

                for (int i = 0; i < existingRoots.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(existingRoots[i]);
                }

                GameObject root = new GameObject("BrokenReferenceSceneRoot");
                SceneManager.MoveGameObjectToScene(root, testScene);

                MeshFilter meshFilter = root.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

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

            AssetDatabase.SaveAssets();

            if (!AssetDatabase.DeleteAsset(referenceAssetPath))
            {
                throw new InvalidOperationException($"Broken Reference용 Asset을 삭제하지 못했습니다: {referenceAssetPath}");
            }

            AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
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