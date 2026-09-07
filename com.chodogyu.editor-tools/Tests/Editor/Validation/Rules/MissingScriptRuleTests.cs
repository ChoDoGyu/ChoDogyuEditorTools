using System.Collections.Generic;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class MissingScriptRuleTests
    {
        [Test]
        public void Type_ReturnsMissingScript()
        {
            MissingScriptRule rule = new MissingScriptRule();

            Assert.AreEqual(ValidationIssueType.MissingScript, rule.Type);
        }

        [Test]
        public void Validate_WithValidGameObject_DoesNotAddIssue()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                MissingScriptRule rule = new MissingScriptRule();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                rule.Validate(
                    gameObject,
                    "Assets/Prefabs/Enemy.prefab",
                    issues);

                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WithValidGameObject_PreservesExistingIssues()
        {
            GameObject gameObject = new GameObject("Enemy");

            try
            {
                ValidationLocation existingLocation = new ValidationLocation(
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
                        existingLocation)
                };

                MissingScriptRule rule = new MissingScriptRule();

                rule.Validate(
                    gameObject,
                    string.Empty,
                    issues);

                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual("기존 문제입니다.", issues[0].Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Validate_WithOneMissingScript_AddsOneIssue()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = MissingScriptPrefabFixture.Create(
                    "MissingScriptSingle",
                    1,
                    false,
                    out assetPath);

                MissingScriptRule rule = new MissingScriptRule();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                rule.Validate(prefab, assetPath, issues);

                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual(ValidationIssueType.MissingScript, issues[0].Type);
                Assert.AreEqual("Missing Script가 1개 발견되었습니다.", issues[0].Message);
                Assert.AreEqual(assetPath, issues[0].Location.AssetPath);
                Assert.AreEqual(prefab.name, issues[0].Location.ObjectPath);
                Assert.IsNotEmpty(issues[0].Location.ObjectGlobalId);
                Assert.AreEqual(string.Empty, issues[0].Location.ComponentName);
                Assert.AreEqual(string.Empty, issues[0].Location.ComponentGlobalId);
            }
            finally
            {
                MissingScriptPrefabFixture.Delete(assetPath);
            }
        }

        [Test]
        public void Validate_WithMultipleMissingScripts_AddsSingleIssueWithCorrectCount()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = MissingScriptPrefabFixture.Create(
                    "MissingScriptMultiple",
                    2,
                    false,
                    out assetPath);

                MissingScriptRule rule = new MissingScriptRule();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                rule.Validate(prefab, assetPath, issues);

                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual("Missing Script가 2개 발견되었습니다.", issues[0].Message);
            }
            finally
            {
                MissingScriptPrefabFixture.Delete(assetPath);
            }
        }

        [Test]
        public void HierarchyValidationRunner_WithMissingScriptOnChild_FindsChildIssue()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = MissingScriptPrefabFixture.Create(
                    "MissingScriptChild",
                    1,
                    true,
                    out assetPath);

                IReadOnlyList<IValidationRule> rules = new IValidationRule[]
                {
                    new MissingScriptRule()
                };

                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(prefab, assetPath, rules, issues);

                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual($"{prefab.name}/Child", issues[0].Location.ObjectPath);
                Assert.AreEqual("Missing Script가 1개 발견되었습니다.", issues[0].Message);
            }
            finally
            {
                MissingScriptPrefabFixture.Delete(assetPath);
            }
        }
    }
}