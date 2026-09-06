using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// 지정된 GameObject를 시작점으로 Hierarchy의 모든 GameObject를 깊이 우선 순서로 순회합니다.
    /// 비활성 GameObject도 검사 대상에 포함하며, 순회 중 Hierarchy를 변경하는 동작은 지원하지 않습니다.
    /// </summary>
    internal static class GameObjectHierarchyTraversal
    {
        /// <summary>
        /// Root GameObject와 모든 자식을 Hierarchy 순서에 따라 한 번씩 방문합니다.
        /// </summary>
        internal static void Traverse(GameObject root, Action<GameObject> visitor)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(root.transform);

            while (stack.Count > 0)
            {
                Transform current = stack.Pop();

                visitor(current.gameObject);

                for (int i = current.childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }
            }
        }
    }
}