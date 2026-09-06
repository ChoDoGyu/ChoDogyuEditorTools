using System;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// 실제 Unity Object를 기반으로 Validation 결과에 사용할 위치 정보를 생성합니다.
    /// 사용자 표시용 Hierarchy 경로와 문제 위치 재탐색을 위한 Global Object ID를 함께 구성합니다.
    /// </summary>
    internal static class ValidationLocationFactory
    {
        /// <summary>
        /// 지정된 GameObject와 선택적 Component 정보를 기반으로 Validation 위치를 생성합니다.
        /// Component가 필요하지 않거나 특정할 수 없는 문제에서는 null을 전달할 수 있습니다.
        /// </summary>
        internal static ValidationLocation Create(GameObject gameObject, Component component, string assetPath, string propertyPath)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            if (assetPath == null)
            {
                throw new ArgumentNullException(nameof(assetPath));
            }

            if (propertyPath == null)
            {
                throw new ArgumentNullException(nameof(propertyPath));
            }

            if (component != null && component.gameObject != gameObject)
            {
                throw new ArgumentException("Component는 지정된 GameObject에 속해야 합니다.", nameof(component));
            }

            string componentName = component != null ? component.GetType().Name : string.Empty;
            string componentGlobalId = component != null ? GetGlobalObjectId(component) : string.Empty;

            return new ValidationLocation(
                assetPath,
                HierarchyPathUtility.GetPath(gameObject),
                componentName,
                propertyPath,
                GetGlobalObjectId(gameObject),
                componentGlobalId);
        }

        private static string GetGlobalObjectId(UnityEngine.Object target)
        {
            return GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
        }
    }
}