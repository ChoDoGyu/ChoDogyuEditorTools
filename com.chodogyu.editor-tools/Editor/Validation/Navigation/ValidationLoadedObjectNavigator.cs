using System;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Validation.Navigation
{
    /// <summary>
    /// 현재 Unity Editor에서 바로 찾을 수 있는 Validation 문제 GameObject를 선택하고 Ping합니다.
    /// 닫힌 Scene이나 아직 열리지 않은 Prefab 내부 Object는 이 단계에서 열지 않습니다.
    /// </summary>
    internal static class ValidationLoadedObjectNavigator
    {
        /// <summary>
        /// 지정된 Validation 위치의 GameObject를 현재 상태에서 찾을 수 있으면 선택하고 Ping합니다.
        /// 대상을 찾을 수 없으면 Editor 상태를 변경하지 않고 false를 반환합니다.
        /// </summary>
        internal static bool TrySelectAndPing(ValidationLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            GameObject gameObject = ValidationObjectResolver.ResolveGameObject(location);

            if (gameObject == null)
            {
                return false;
            }

            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);

            return true;
        }
    }
}