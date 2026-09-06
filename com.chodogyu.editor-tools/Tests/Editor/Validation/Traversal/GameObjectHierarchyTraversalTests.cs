using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation;
using NUnit.Framework;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class GameObjectHierarchyTraversalTests
    {
        [Test]
        public void Traverse_WithRootOnly_VisitsRootOnce()
        {
            GameObject root = new GameObject("Root");

            try
            {
                List<GameObject> visited = new List<GameObject>();

                GameObjectHierarchyTraversal.Traverse(root, visited.Add);

                Assert.AreEqual(1, visited.Count);
                Assert.AreSame(root, visited[0]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Traverse_WithNestedHierarchy_VisitsEveryObject()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("Child");
            GameObject grandChild = new GameObject("GrandChild");

            try
            {
                child.transform.SetParent(root.transform);
                grandChild.transform.SetParent(child.transform);

                List<GameObject> visited = new List<GameObject>();

                GameObjectHierarchyTraversal.Traverse(root, visited.Add);

                Assert.AreEqual(3, visited.Count);
                Assert.AreSame(root, visited[0]);
                Assert.AreSame(child, visited[1]);
                Assert.AreSame(grandChild, visited[2]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Traverse_WithMultipleBranches_UsesHierarchyDepthFirstOrder()
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

                List<GameObject> visited = new List<GameObject>();

                GameObjectHierarchyTraversal.Traverse(root, visited.Add);

                CollectionAssert.AreEqual(
                    new[] { root, first, firstChild, second },
                    visited);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Traverse_WithInactiveChild_IncludesInactiveObject()
        {
            GameObject root = new GameObject("Root");
            GameObject child = new GameObject("InactiveChild");

            try
            {
                child.transform.SetParent(root.transform);
                child.SetActive(false);

                List<GameObject> visited = new List<GameObject>();

                GameObjectHierarchyTraversal.Traverse(root, visited.Add);

                Assert.AreEqual(2, visited.Count);
                Assert.Contains(child, visited);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Traverse_WithNullRoot_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => GameObjectHierarchyTraversal.Traverse(null, _ => { }));
        }

        [Test]
        public void Traverse_WithNullVisitor_ThrowsArgumentNullException()
        {
            GameObject root = new GameObject("Root");

            try
            {
                Assert.Throws<ArgumentNullException>(() => GameObjectHierarchyTraversal.Traverse(root, null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}