using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationRunnerTests
    {
        private sealed class TestValidationRule : IValidationRule
        {
            private readonly bool _addIssue;

            public ValidationIssueType Type { get; }

            internal int CallCount { get; private set; }

            internal GameObject LastGameObject { get; private set; }

            internal string LastAssetPath { get; private set; }

            internal TestValidationRule(ValidationIssueType type, bool addIssue)
            {
                Type = type;
                _addIssue = addIssue;
            }

            public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
            {
                CallCount++;
                LastGameObject = gameObject;
                LastAssetPath = assetPath;

                if (!_addIssue)
                {
                    return;
                }

                ValidationLocation location = new ValidationLocation(
                    assetPath,
                    gameObject.name,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);

                issues.Add(new ValidationIssue(
                    Type,
                    $"{Type} 테스트 문제입니다.",
                    location));
            }
        }

        [Test]
        public void Validate_WithMultipleRules_InvokesEveryRule()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                TestValidationRule firstRule = new TestValidationRule(ValidationIssueType.MissingScript, false);
                TestValidationRule secondRule = new TestValidationRule(ValidationIssueType.BrokenSerializedReference, false);
                IReadOnlyList<IValidationRule> rules = new IValidationRule[] { firstRule, secondRule };
                List<ValidationIssue> issues = new List<ValidationIssue>();
                ValidationRunner runner = new ValidationRunner();

                runner.Validate(gameObject, "Assets/Prefabs/Enemy.prefab", rules, issues);

                Assert.AreEqual(1, firstRule.CallCount);
                Assert.AreEqual(1, secondRule.CallCount);
                Assert.AreSame(gameObject, firstRule.LastGameObject);
                Assert.AreSame(gameObject, secondRule.LastGameObject);
                Assert.AreEqual("Assets/Prefabs/Enemy.prefab", firstRule.LastAssetPath);
                Assert.AreEqual("Assets/Prefabs/Enemy.prefab", secondRule.LastAssetPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WhenRulesAddIssues_PreservesAllIssues()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                IReadOnlyList<IValidationRule> rules = new IValidationRule[]
                {
                    new TestValidationRule(ValidationIssueType.MissingScript, true),
                    new TestValidationRule(ValidationIssueType.BrokenSerializedReference, true)
                };

                List<ValidationIssue> issues = new List<ValidationIssue>();
                ValidationRunner runner = new ValidationRunner();

                runner.Validate(gameObject, "Assets/Prefabs/Enemy.prefab", rules, issues);

                Assert.AreEqual(2, issues.Count);
                Assert.AreEqual(ValidationIssueType.MissingScript, issues[0].Type);
                Assert.AreEqual(ValidationIssueType.BrokenSerializedReference, issues[1].Type);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WithNoRules_DoesNotModifyIssues()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                ValidationLocation location = new ValidationLocation(
                    string.Empty,
                    "Existing",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);

                List<ValidationIssue> issues = new List<ValidationIssue>
                {
                    new ValidationIssue(
                        ValidationIssueType.MissingScript,
                        "기존 문제입니다.",
                        location)
                };

                ValidationRunner runner = new ValidationRunner();

                runner.Validate(gameObject, string.Empty, Array.Empty<IValidationRule>(), issues);

                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual("기존 문제입니다.", issues[0].Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WithNullGameObject_ThrowsArgumentNullException()
        {
            ValidationRunner runner = new ValidationRunner();
            List<ValidationIssue> issues = new List<ValidationIssue>();

            Assert.Throws<ArgumentNullException>(() => runner.Validate(null, string.Empty, Array.Empty<IValidationRule>(), issues));
        }

        [Test]
        public void Validate_WithNullAssetPath_ThrowsArgumentNullException()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                ValidationRunner runner = new ValidationRunner();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                Assert.Throws<ArgumentNullException>(() => runner.Validate(gameObject, null, Array.Empty<IValidationRule>(), issues));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WithNullRules_ThrowsArgumentNullException()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                ValidationRunner runner = new ValidationRunner();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                Assert.Throws<ArgumentNullException>(() => runner.Validate(gameObject, string.Empty, null, issues));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WithNullIssues_ThrowsArgumentNullException()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                ValidationRunner runner = new ValidationRunner();

                Assert.Throws<ArgumentNullException>(() => runner.Validate(gameObject, string.Empty, Array.Empty<IValidationRule>(), null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WithNullRule_ThrowsArgumentException()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                ValidationRunner runner = new ValidationRunner();
                IReadOnlyList<IValidationRule> rules = new IValidationRule[] { null };
                List<ValidationIssue> issues = new List<ValidationIssue>();

                Assert.Throws<ArgumentException>(() => runner.Validate(gameObject, string.Empty, rules, issues));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}