using UnityEditor;

namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// Unity SerializedProperty의 Object Reference 상태를 판별하는 공통 기능을 제공합니다.
    /// </summary>
    internal static class SerializedReferenceUtility
    {
        /// <summary>
        /// 지정된 Property가 삭제된 Unity Object를 가리키는 깨진 Object Reference인지 확인합니다.
        /// 정상적인 null Reference는 문제로 판단하지 않습니다.
        /// </summary>
        internal static bool IsBrokenObjectReference(SerializedProperty property)
        {
            if (property == null)
            {
                return false;
            }

            return property.propertyType == SerializedPropertyType.ObjectReference
                && property.objectReferenceValue == null
                && property.objectReferenceInstanceIDValue != 0;
        }
    }
}