using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// GameObject의 부모 관계를 기준으로 사용자에게 표시할 Hierarchy 경로를 생성합니다.
    /// 생성된 경로는 위치 표시용이며 고유 객체 식별자로 사용하지 않습니다.
    /// </summary>
    internal static class HierarchyPathUtility
    {
        /// <summary>
        /// 지정된 GameObject부터 최상위 부모까지의 이름을 조합해 Hierarchy 경로를 반환합니다.
        /// </summary>
        internal static string GetPath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            List<string> names = new List<string>();
            Transform current = gameObject.transform;

            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();

            return string.Join("/", names);
        }
    }
}