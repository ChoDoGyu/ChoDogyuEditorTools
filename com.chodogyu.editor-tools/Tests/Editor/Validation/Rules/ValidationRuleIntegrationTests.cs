using System.Collections.Generic;
using System.Linq;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationRuleIntegrationTests
    {
        [Test]
        public void Validate_WithAllRulesOnMissingScriptPrefab_ReportsOnlyMissingScript()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = MissingScriptPrefabFixture.Create(
                    "AllRulesMissingScript",
                    1,
                    true,
                    out assetPath);

                IReadOnlyList<IValidationRule> rules = CreateAllRules();
                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(prefab, assetPath, rules, issues);

                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual(ValidationIssueType.MissingScript, issues[0].Type);
            }
            finally
            {
                MissingScriptPrefabFixture.Delete(assetPath);
            }
        }

        [Test]
        public void Validate_WithAllRulesOnBrokenReferencePrefab_ReportsOnlyBrokenReference()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = BrokenSerializedReferenceFixture.Create(out assetPath);

                IReadOnlyList<IValidationRule> rules = CreateAllRules();
                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(prefab, assetPath, rules, issues);

                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual(ValidationIssueType.BrokenSerializedReference, issues[0].Type);
                Assert.AreEqual("m_Mesh", issues[0].Location.PropertyPath);
            }
            finally
            {
                BrokenSerializedReferenceFixture.Delete(assetPath);
            }
        }

        [Test]
        public void Validate_MultipleRootsWithSharedIssueCollection_AccumulatesBothIssueTypes()
        {
            string missingScriptAssetPath = null;
            string brokenReferenceAssetPath = null;

            try
            {
                GameObject missingScriptPrefab = MissingScriptPrefabFixture.Create(
                    "CombinedMissingScript",
                    1,
                    false,
                    out missingScriptAssetPath);

                GameObject brokenReferencePrefab = BrokenSerializedReferenceFixture.Create(
                    out brokenReferenceAssetPath);

                IReadOnlyList<IValidationRule> rules = CreateAllRules();
                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(
                    missingScriptPrefab,
                    missingScriptAssetPath,
                    rules,
                    issues);

                runner.Validate(
                    brokenReferencePrefab,
                    brokenReferenceAssetPath,
                    rules,
                    issues);

                Assert.AreEqual(2, issues.Count);
                Assert.IsTrue(issues.Any(issue => issue.Type == ValidationIssueType.MissingScript));
                Assert.IsTrue(issues.Any(issue => issue.Type == ValidationIssueType.BrokenSerializedReference));
            }
            finally
            {
                MissingScriptPrefabFixture.Delete(missingScriptAssetPath);
                BrokenSerializedReferenceFixture.Delete(brokenReferenceAssetPath);
            }
        }

        private static IReadOnlyList<IValidationRule> CreateAllRules()
        {
            return new IValidationRule[]
            {
                new MissingScriptRule(),
                new BrokenSerializedReferenceRule()
            };
        }
    }
}