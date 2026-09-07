using System.Collections.Generic;
using System.Diagnostics;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class LargeHierarchyValidationTests
    {
        private sealed class CountingValidationRule : IValidationRule
        {
            public ValidationIssueType Type => ValidationIssueType.MissingScript;

            internal int VisitCount { get; private set; }

            internal HashSet<int> VisitedInstanceIds { get; } = new HashSet<int>();

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                VisitCount++;
                VisitedInstanceIds.Add(gameObject.GetInstanceID());
            }
        }

        [Test]
        public void Traverse_WithLargeWideHierarchy_VisitsEveryObjectExactlyOnce()
        {
            const int branchCount = 100;
            const int childrenPerBranch = 10;
            const int expectedObjectCount = 1 + branchCount + branchCount * childrenPerBranch;

            GameObject root = CreateWideHierarchy(branchCount, childrenPerBranch);

            try
            {
                HashSet<int> visitedInstanceIds = new HashSet<int>();

                GameObjectHierarchyTraversal.Traverse(root, gameObject =>
                {
                    Assert.IsTrue(visitedInstanceIds.Add(gameObject.GetInstanceID()));
                });

                Assert.AreEqual(expectedObjectCount, visitedInstanceIds.Count);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Traverse_WithDeepHierarchy_VisitsEveryObjectExactlyOnce()
        {
            const int depth = 200;
            const int expectedObjectCount = depth + 1;

            GameObject root = CreateDeepHierarchy(depth);

            try
            {
                HashSet<int> visitedInstanceIds = new HashSet<int>();

                GameObjectHierarchyTraversal.Traverse(root, gameObject =>
                {
                    Assert.IsTrue(visitedInstanceIds.Add(gameObject.GetInstanceID()));
                });

                Assert.AreEqual(expectedObjectCount, visitedInstanceIds.Count);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void HierarchyValidationRunner_WithLargeHierarchy_ExecutesRuleExactlyOncePerObject()
        {
            const int branchCount = 50;
            const int childrenPerBranch = 10;
            const int expectedObjectCount = 1 + branchCount + branchCount * childrenPerBranch;

            GameObject root = CreateWideHierarchy(branchCount, childrenPerBranch);

            try
            {
                CountingValidationRule rule = new CountingValidationRule();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                runner.Validate(root, string.Empty, new IValidationRule[] { rule }, issues);

                Assert.AreEqual(expectedObjectCount, rule.VisitCount);
                Assert.AreEqual(expectedObjectCount, rule.VisitedInstanceIds.Count);
                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void HierarchyValidationRunner_WithActualRules_ValidatesLargeHierarchyWithoutFalsePositive()
        {
            const int childCount = 300;

            GameObject root = CreateReferenceHierarchy(childCount);

            try
            {
                IReadOnlyList<IValidationRule> rules = new IValidationRule[]
                {
                    new MissingScriptRule(),
                    new BrokenSerializedReferenceRule()
                };

                HierarchyValidationRunner runner = new HierarchyValidationRunner();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                Stopwatch stopwatch = Stopwatch.StartNew();

                runner.Validate(root, string.Empty, rules, issues);

                stopwatch.Stop();

                TestContext.WriteLine($"Large hierarchy validation: {childCount + 1} GameObjects, {stopwatch.ElapsedMilliseconds} ms");

                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateWideHierarchy(int branchCount, int childrenPerBranch)
        {
            GameObject root = new GameObject("LargeRoot");

            for (int branchIndex = 0; branchIndex < branchCount; branchIndex++)
            {
                GameObject branch = new GameObject($"Branch_{branchIndex}");
                branch.transform.SetParent(root.transform);

                for (int childIndex = 0; childIndex < childrenPerBranch; childIndex++)
                {
                    GameObject child = new GameObject($"Child_{branchIndex}_{childIndex}");
                    child.transform.SetParent(branch.transform);
                }
            }

            return root;
        }

        private static GameObject CreateDeepHierarchy(int depth)
        {
            GameObject root = new GameObject("DeepRoot");
            Transform parent = root.transform;

            for (int i = 0; i < depth; i++)
            {
                GameObject child = new GameObject($"Depth_{i}");
                child.transform.SetParent(parent);
                parent = child.transform;
            }

            return root;
        }

        private static GameObject CreateReferenceHierarchy(int childCount)
        {
            GameObject root = new GameObject("ReferenceRoot");

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = new GameObject($"Reference_{i}");
                child.transform.SetParent(root.transform);
                child.AddComponent<MeshFilter>();
            }

            return root;
        }
    }
}