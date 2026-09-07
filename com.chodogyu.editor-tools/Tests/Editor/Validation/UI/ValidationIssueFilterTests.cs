using System;
using CDG.EditorTools.Validation;
using CDG.EditorTools.Validation.UI;
using NUnit.Framework;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationIssueFilterTests
    {
        [Test]
        public void Matches_WithDisabledIssueType_ReturnsFalse()
        {
            ValidationIssue missingScriptIssue = CreateIssue(ValidationIssueType.MissingScript);
            ValidationIssue brokenReferenceIssue = CreateIssue(ValidationIssueType.BrokenSerializedReference);

            Assert.IsFalse(ValidationIssueFilter.Matches(missingScriptIssue, string.Empty, false, true));
            Assert.IsFalse(ValidationIssueFilter.Matches(brokenReferenceIssue, string.Empty, true, false));
        }

        [Test]
        public void Matches_WithEmptySearch_ReturnsTrue()
        {
            ValidationIssue issue = CreateIssue(ValidationIssueType.MissingScript);

            Assert.IsTrue(ValidationIssueFilter.Matches(issue, string.Empty, true, true));
        }

        [Test]
        public void Matches_WithAssetPathSearch_ReturnsTrue()
        {
            ValidationIssue issue = CreateIssue(ValidationIssueType.MissingScript);

            Assert.IsTrue(ValidationIssueFilter.Matches(issue, "enemy.prefab", true, true));
        }

        [Test]
        public void Matches_WithObjectPathSearch_ReturnsTrue()
        {
            ValidationIssue issue = CreateIssue(ValidationIssueType.MissingScript);

            Assert.IsTrue(ValidationIssueFilter.Matches(issue, "weapon", true, true));
        }

        [Test]
        public void Matches_WithComponentNameSearch_ReturnsTrue()
        {
            ValidationIssue issue = CreateIssue(ValidationIssueType.BrokenSerializedReference);

            Assert.IsTrue(ValidationIssueFilter.Matches(issue, "MeshFilter", true, true));
        }

        [Test]
        public void Matches_WithPropertyPathSearch_ReturnsTrue()
        {
            ValidationIssue issue = CreateIssue(ValidationIssueType.BrokenSerializedReference);

            Assert.IsTrue(ValidationIssueFilter.Matches(issue, "m_Mesh", true, true));
        }

        [Test]
        public void Matches_WithMessageSearch_IsCaseInsensitive()
        {
            ValidationIssue issue = CreateIssue(ValidationIssueType.BrokenSerializedReference);

            Assert.IsTrue(ValidationIssueFilter.Matches(issue, "DELETED REFERENCE", true, true));
        }

        [Test]
        public void Matches_WithUnmatchedSearch_ReturnsFalse()
        {
            ValidationIssue issue = CreateIssue(ValidationIssueType.MissingScript);

            Assert.IsFalse(ValidationIssueFilter.Matches(issue, "DoesNotExist", true, true));
        }

        [Test]
        public void Matches_WithNullIssue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ValidationIssueFilter.Matches(null, string.Empty, true, true));
        }

        private static ValidationIssue CreateIssue(ValidationIssueType type)
        {
            ValidationLocation location = new ValidationLocation(
                "Assets/Prefabs/Enemy.prefab",
                "Enemy/Weapon",
                "MeshFilter",
                "m_Mesh",
                string.Empty,
                string.Empty);

            string message = type == ValidationIssueType.MissingScript
                ? "Missing Script가 발견되었습니다."
                : "Deleted Reference가 발견되었습니다.";

            return new ValidationIssue(type, message, location);
        }
    }
}