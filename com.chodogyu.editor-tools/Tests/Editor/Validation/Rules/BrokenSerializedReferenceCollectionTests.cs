using System.Linq;
using CDG.EditorTools.Tests.Fixtures;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class BrokenSerializedReferenceCollectionTests
    {
        private string _prefabAssetPath;
        private GameObject _prefab;
        private SerializedReferenceCollectionTestComponent _component;

        [SetUp]
        public void SetUp()
        {
            _prefab = BrokenSerializedReferenceCollectionFixture.Create(out _prefabAssetPath);
            _component = _prefab.GetComponent<SerializedReferenceCollectionTestComponent>();

            Assert.IsNotNull(_component);
        }

        [TearDown]
        public void TearDown()
        {
            BrokenSerializedReferenceCollectionFixture.Delete(_prefabAssetPath);
        }

        [Test]
        public void Validate_WithArrayBrokenReference_FindsArrayElement()
        {
            ValidationIssue[] issues = Validate();

            Assert.IsTrue(issues.Any(issue =>
                issue.Location.PropertyPath == "_arrayReferences.Array.data[0]"));
        }

        [Test]
        public void Validate_WithListBrokenReference_FindsListElement()
        {
            ValidationIssue[] issues = Validate();

            Assert.IsTrue(issues.Any(issue =>
                issue.Location.PropertyPath == "_listReferences.Array.data[0]"));
        }

        [Test]
        public void Validate_WithNestedBrokenReference_FindsNestedProperty()
        {
            ValidationIssue[] issues = Validate();

            Assert.IsTrue(issues.Any(issue =>
                issue.Location.PropertyPath == "_nestedData._reference"));
        }

        [Test]
        public void Validate_WithThreeBrokenReferences_AddsOneIssuePerProperty()
        {
            ValidationIssue[] issues = Validate();

            Assert.AreEqual(3, issues.Length);
            Assert.IsTrue(issues.All(issue =>
                issue.Type == ValidationIssueType.BrokenSerializedReference));
            Assert.IsTrue(issues.All(issue =>
                issue.Location.ComponentName == nameof(SerializedReferenceCollectionTestComponent)));
        }

        private ValidationIssue[] Validate()
        {
            BrokenSerializedReferenceRule rule = new BrokenSerializedReferenceRule();
            System.Collections.Generic.List<ValidationIssue> issues = new System.Collections.Generic.List<ValidationIssue>();

            rule.Validate(_prefab, _prefabAssetPath, issues);

            return issues.ToArray();
        }
    }
}