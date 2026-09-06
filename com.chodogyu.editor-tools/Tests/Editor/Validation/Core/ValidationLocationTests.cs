using System;
using CDG.EditorTools.Validation;
using NUnit.Framework;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class ValidationLocationTests
    {
        [Test]
        public void Constructor_WithValidValues_StoresAllValues()
        {
            ValidationLocation location = new ValidationLocation(
                "Assets/Prefabs/Enemy.prefab",
                "Enemy/Weapon",
                "EnemyAttack",
                "effectPrefab",
                "GlobalObjectId_V1-Object",
                "GlobalObjectId_V1-Component");

            Assert.AreEqual("Assets/Prefabs/Enemy.prefab", location.AssetPath);
            Assert.AreEqual("Enemy/Weapon", location.ObjectPath);
            Assert.AreEqual("EnemyAttack", location.ComponentName);
            Assert.AreEqual("effectPrefab", location.PropertyPath);
            Assert.AreEqual("GlobalObjectId_V1-Object", location.ObjectGlobalId);
            Assert.AreEqual("GlobalObjectId_V1-Component", location.ComponentGlobalId);
        }

        [Test]
        public void Constructor_WithEmptyOptionalValues_AllowsEmptyStrings()
        {
            ValidationLocation location = new ValidationLocation(
                string.Empty,
                "Enemy",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);

            Assert.AreEqual(string.Empty, location.AssetPath);
            Assert.AreEqual("Enemy", location.ObjectPath);
            Assert.AreEqual(string.Empty, location.ComponentName);
            Assert.AreEqual(string.Empty, location.PropertyPath);
            Assert.AreEqual(string.Empty, location.ObjectGlobalId);
            Assert.AreEqual(string.Empty, location.ComponentGlobalId);
        }

        [TestCase(null, "Enemy", "", "", "", "")]
        [TestCase("", null, "", "", "", "")]
        [TestCase("", "Enemy", null, "", "", "")]
        [TestCase("", "Enemy", "", null, "", "")]
        [TestCase("", "Enemy", "", "", null, "")]
        [TestCase("", "Enemy", "", "", "", null)]
        public void Constructor_WithNullValue_ThrowsArgumentNullException(string assetPath, string objectPath, string componentName, string propertyPath, string objectGlobalId, string componentGlobalId)
        {
            Assert.Throws<ArgumentNullException>(() => new ValidationLocation(assetPath, objectPath, componentName, propertyPath, objectGlobalId, componentGlobalId));
        }
    }
}