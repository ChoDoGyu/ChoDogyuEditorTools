using System;

namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// Validation 문제가 발견된 Unity 프로젝트 내 위치 정보를 나타냅니다.
    /// Asset, GameObject, Component 및 Serialized Property 위치와 재탐색에 필요한 식별 정보를 저장합니다.
    /// </summary>
    internal sealed class ValidationLocation
    {
        /// <summary>
        /// 문제가 포함된 Asset의 프로젝트 상대 경로입니다.
        /// 현재 열린 Scene의 오브젝트처럼 Asset 경로가 필요하지 않은 경우 빈 문자열일 수 있습니다.
        /// </summary>
        internal string AssetPath { get; }

        /// <summary>
        /// 문제가 발견된 GameObject의 Hierarchy 경로입니다.
        /// 사용자에게 위치를 표시하기 위한 정보이며, 실제 재탐색에는 Global Object ID를 우선 사용합니다.
        /// </summary>
        internal string ObjectPath { get; }

        /// <summary>
        /// 문제가 발견된 Component의 타입 이름입니다.
        /// Component를 특정할 수 없는 문제에서는 빈 문자열일 수 있습니다.
        /// </summary>
        internal string ComponentName { get; }

        /// <summary>
        /// 문제가 발견된 SerializedProperty의 Property Path입니다.
        /// Property와 관련되지 않은 문제에서는 빈 문자열일 수 있습니다.
        /// </summary>
        internal string PropertyPath { get; }

        /// <summary>
        /// 문제가 발견된 GameObject를 다시 찾기 위한 Unity Global Object ID 문자열입니다.
        /// 식별자를 생성할 수 없는 경우 빈 문자열일 수 있습니다.
        /// </summary>
        internal string ObjectGlobalId { get; }

        /// <summary>
        /// 문제가 발견된 Component를 다시 찾기 위한 Unity Global Object ID 문자열입니다.
        /// Component를 특정할 수 없거나 식별자를 생성할 수 없는 경우 빈 문자열일 수 있습니다.
        /// </summary>
        internal string ComponentGlobalId { get; }

        internal ValidationLocation(string assetPath, string objectPath, string componentName, string propertyPath, string objectGlobalId, string componentGlobalId)
        {
            AssetPath = assetPath ?? throw new ArgumentNullException(nameof(assetPath));
            ObjectPath = objectPath ?? throw new ArgumentNullException(nameof(objectPath));
            ComponentName = componentName ?? throw new ArgumentNullException(nameof(componentName));
            PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
            ObjectGlobalId = objectGlobalId ?? throw new ArgumentNullException(nameof(objectGlobalId));
            ComponentGlobalId = componentGlobalId ?? throw new ArgumentNullException(nameof(componentGlobalId));
        }
    }
}