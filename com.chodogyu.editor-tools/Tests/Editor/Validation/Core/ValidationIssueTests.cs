using System;
using CDG.EditorTools.Validation;
using NUnit.Framework;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationIssueTests
    {
        [Test]
        public void Constructor_WithValidValues_StoresAllValues()
        {
            ValidationLocation location = new ValidationLocation(
                "Assets/Prefabs/Enemy.prefab",
                "Enemy",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            ValidationIssue issue = new ValidationIssue(
                ValidationIssueType.MissingScript,
                "Missing Script가 발견되었습니다.",
                location);

            Assert.AreEqual(ValidationIssueType.MissingScript, issue.Type);
            Assert.AreEqual("Missing Script가 발견되었습니다.", issue.Message);
            Assert.AreSame(location, issue.Location);
        }

        [Test]
        public void Constructor_WithNullMessage_ThrowsArgumentNullException()
        {
            ValidationLocation location = new ValidationLocation(
                string.Empty,
                "Enemy",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            Assert.Throws<ArgumentNullException>(() => new ValidationIssue(
                ValidationIssueType.MissingScript,
                null,
                location));
        }

        [Test]
        public void Constructor_WithNullLocation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ValidationIssue(
                ValidationIssueType.MissingScript,
                "Missing Script가 발견되었습니다.",
                null));
        }
    }
}