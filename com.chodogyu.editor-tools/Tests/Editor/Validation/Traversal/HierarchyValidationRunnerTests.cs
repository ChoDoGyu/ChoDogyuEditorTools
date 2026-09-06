using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class HierarchyValidationRunnerTests
    {
        private sealed class RecordingValidationRule : IValidationRule
        {
            private readonly bool _addIssue;

            public ValidationIssueType Type { get; }

            internal List<GameObject> VisitedObjects { get; } = new List<GameObject>();

            internal List<string> ReceivedAssetPaths { get; } = new List<string>();

            internal RecordingValidationRule(ValidationIssueType type, bool addIssue)
            {
                Type = type;
                _addIssue = addIssue;
            }

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                VisitedObjects.Add(gameObject);
                ReceivedAssetPaths.Add(assetPath);

                if (!_addIssue)
                {
                    return;
                }

                ValidationLocation location = ValidationLocationFactory.Create(
                    gameObject,
                    null,
                    assetPath,
                    string.Empty);

                issues.Add(new ValidationIssue(
                    Type,
                    $"{gameObject.name} 테스트 문제입니다.",
                    location));
            }
        }

        [Test]
        public void Validate_WithNestedHierarchy_ExecutesRuleForEveryObjectInHierarchyOrder()
        {
            GameObject root = new GameObject("Root");
            GameObject first = new GameObject("First");
            GameObject firstChild = new GameObject("FirstChild");
            GameObject second = new GameObject("Second");

            try
            {
                first.transform.SetParent(root.transform);
                second.transform.SetParent(root.transform);
                firstChild.transform.SetParent(first.transform);

                RecordingValidationRule rule = new RecordingValidationRule(
                    ValidationIssueType.MissingScript,
                    false);

                IReadOnlyList<IValidationRule> rules = new IValidationRule[] { rule };
                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(root, "Assets/Prefabs/Test.prefab", rules, issues);

                CollectionAssert.AreEqual(
                    new[] { root, first, firstChild, second },
                    rule.VisitedObjects);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Validate_WithMultipleRules_ExecutesEveryRuleForEveryObject()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");

            try
            {
                child.transform.SetParent(root.transform);

                RecordingValidationRule firstRule = new RecordingValidationRule(
                    ValidationIssueType.MissingScript,
                    false);

                RecordingValidationRule secondRule = new RecordingValidationRule(
                    ValidationIssueType.BrokenSerializedReference,
                    false);

                IReadOnlyList<IValidationRule> rules = new IValidationRule[]
                {
                    firstRule,
                    secondRule
                };

                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(root, string.Empty, rules, issues);

                Assert.AreEqual(2, firstRule.VisitedObjects.Count);
                Assert.AreEqual(2, secondRule.VisitedObjects.Count);
                Assert.AreSame(root, firstRule.VisitedObjects[0]);
                Assert.AreSame(child, firstRule.VisitedObjects[1]);
                Assert.AreSame(root, secondRule.VisitedObjects[0]);
                Assert.AreSame(child, secondRule.VisitedObjects[1]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Validate_PassesAssetPathToEveryValidationCall()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");

            try
            {
                child.transform.SetParent(root.transform);

                RecordingValidationRule rule = new RecordingValidationRule(
                    ValidationIssueType.MissingScript,
                    false);

                IReadOnlyList<IValidationRule> rules = new IValidationRule[] { rule };
                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(root, "Assets/Scenes/Test.unity", rules, issues);

                CollectionAssert.AreEqual(
                    new[]
                    {
                        "Assets/Scenes/Test.unity",
                        "Assets/Scenes/Test.unity"
                    },
                    rule.ReceivedAssetPaths);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Validate_WhenRuleAddsIssues_AccumulatesIssuesAcrossHierarchy()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");
            GameObject grandChild = new GameObject("GrandChild");

            try
            {
                child.transform.SetParent(root.transform);
                grandChild.transform.SetParent(child.transform);

                RecordingValidationRule rule = new RecordingValidationRule(
                    ValidationIssueType.MissingScript,
                    true);

                IReadOnlyList<IValidationRule> rules = new IValidationRule[] { rule };
                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(root, "Assets/Prefabs/Test.prefab", rules, issues);

                Assert.AreEqual(3, issues.Count);
                Assert.AreEqual("Root", issues[0].Location.ObjectPath);
                Assert.AreEqual("Root/Child", issues[1].Location.ObjectPath);
                Assert.AreEqual("Root/Child/GrandChild", issues[2].Location.ObjectPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Validate_WithNoRules_DoesNotAddIssues()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");

            try
            {
                child.transform.SetParent(root.transform);

                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(root, string.Empty, Array.Empty<IValidationRule>(), issues);

                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Validate_WithNullRoot_ThrowsArgumentNullException()
        {
            HierarchyValidationRunner runner = new HierarchyValidationRunner();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.Throws<ArgumentNullException>(() => runner.Validate(
                null,
                string.Empty,
                Array.Empty<IValidationRule>(),
                issues));
        }
    }
}