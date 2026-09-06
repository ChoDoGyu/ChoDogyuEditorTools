using System;
using CDG.EditorTools.Validation;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class HierarchyPathUtilityTests
    {
        [Test]
        public void GetPath_WithRootObject_ReturnsObjectName()
        {
            GameObject root = new GameObject("Enemy");

            try
            {
                string path = HierarchyPathUtility.GetPath(root);

                Assert.AreEqual("Enemy", path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetPath_WithNestedObject_ReturnsFullHierarchyPath()
        {
            GameObject root = new GameObject("Enemy");
            GameObject visual = new GameObject("Visual");
            GameObject weapon = new GameObject("Weapon");

            try
            {
                visual.transform.SetParent(root.transform);
                weapon.transform.SetParent(visual.transform);

                string path = HierarchyPathUtility.GetPath(weapon);

                Assert.AreEqual("Enemy/Visual/Weapon", path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetPath_WithNullGameObject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => HierarchyPathUtility.GetPath(null));
        }
    }
}