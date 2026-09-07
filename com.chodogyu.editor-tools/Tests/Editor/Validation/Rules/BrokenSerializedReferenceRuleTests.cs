using System.Collections.Generic;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class BrokenSerializedReferenceRuleTests
    {
        [Test]
        public void Type_ReturnsBrokenSerializedReference()
        {
            BrokenSerializedReferenceRule rule = new BrokenSerializedReferenceRule();

            Assert.AreEqual(ValidationIssueType.BrokenSerializedReference, rule.Type);
        }

        [Test]
        public void Validate_WithNormalNullReference_DoesNotAddIssue()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = BrokenSerializedReferenceFixture.Create(out assetPath);
                GameObject target = prefab.transform.Find("NormalNull").gameObject;

                BrokenSerializedReferenceRule rule = new BrokenSerializedReferenceRule();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                rule.Validate(target, assetPath, issues);

                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                BrokenSerializedReferenceFixture.Delete(assetPath);
            }
        }

        [Test]
        public void Validate_WithValidReference_DoesNotAddIssue()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = BrokenSerializedReferenceFixture.Create(out assetPath);
                GameObject target = prefab.transform.Find("ValidReference").gameObject;

                BrokenSerializedReferenceRule rule = new BrokenSerializedReferenceRule();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                rule.Validate(target, assetPath, issues);

                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                BrokenSerializedReferenceFixture.Delete(assetPath);
            }
        }

        [Test]
        public void Validate_WithBrokenReference_AddsIssueWithCorrectLocation()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = BrokenSerializedReferenceFixture.Create(out assetPath);
                GameObject target = prefab.transform.Find("BrokenReference").gameObject;

                BrokenSerializedReferenceRule rule = new BrokenSerializedReferenceRule();
                List<ValidationIssue> issues = new List<ValidationIssue>();

                rule.Validate(target, assetPath, issues);

                Assert.AreEqual(1, issues.Count);

                ValidationIssue issue = issues[0];

                Assert.AreEqual(ValidationIssueType.BrokenSerializedReference, issue.Type);
                Assert.AreEqual("삭제된 Unity Object를 참조하는 Serialized Reference가 발견되었습니다.", issue.Message);
                Assert.AreEqual(assetPath, issue.Location.AssetPath);
                Assert.AreEqual($"{prefab.name}/BrokenReference", issue.Location.ObjectPath);
                Assert.AreEqual(nameof(MeshFilter), issue.Location.ComponentName);
                Assert.AreEqual("m_Mesh", issue.Location.PropertyPath);
                Assert.IsNotEmpty(issue.Location.ObjectGlobalId);
                Assert.IsNotEmpty(issue.Location.ComponentGlobalId);
            }
            finally
            {
                BrokenSerializedReferenceFixture.Delete(assetPath);
            }
        }

        [Test]
        public void HierarchyValidationRunner_WithBrokenReference_FindsIssue()
        {
            string assetPath = null;

            try
            {
                GameObject prefab = BrokenSerializedReferenceFixture.Create(out assetPath);

                IReadOnlyList<IValidationRule> rules = new IValidationRule[]
                {
                    new BrokenSerializedReferenceRule()
                };

                List<ValidationIssue> issues = new List<ValidationIssue>();
                HierarchyValidationRunner runner = new HierarchyValidationRunner();

                runner.Validate(prefab, assetPath, rules, issues);

                Assert.AreEqual(1, issues.Count);
                Assert.AreEqual($"{prefab.name}/BrokenReference", issues[0].Location.ObjectPath);
                Assert.AreEqual("m_Mesh", issues[0].Location.PropertyPath);
            }
            finally
            {
                BrokenSerializedReferenceFixture.Delete(assetPath);
            }
        }
    }
}