using System;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Validation.Navigation
{
    /// <summary>
    /// Validation 결과에 저장된 Global Object ID를 이용해 실제 Unity Object를 다시 찾습니다.
    /// Scene 내부 Object는 해당 Scene이 로드되어 있어야 정상적으로 찾을 수 있습니다.
    /// </summary>
    internal static class ValidationObjectResolver
    {
        /// <summary>
        /// Validation 위치에 기록된 GameObject 식별자를 이용해 실제 GameObject를 찾습니다.
        /// 현재 상태에서 대상을 찾을 수 없으면 null을 반환합니다.
        /// </summary>
        internal static GameObject ResolveGameObject(ValidationLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            return Resolve(location.ObjectGlobalId) as GameObject;
        }

        /// <summary>
        /// Validation 위치에 기록된 Component 식별자를 이용해 실제 Component를 찾습니다.
        /// Component 정보가 없거나 현재 상태에서 대상을 찾을 수 없으면 null을 반환합니다.
        /// </summary>
        internal static Component ResolveComponent(ValidationLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            return Resolve(location.ComponentGlobalId) as Component;
        }

        /// <summary>
        /// 문자열 형태의 Global Object ID를 실제 Unity Object로 변환합니다.
        /// 빈 문자열, 잘못된 식별자 또는 현재 찾을 수 없는 대상은 null을 반환합니다.
        /// </summary>
        internal static UnityEngine.Object Resolve(string globalObjectId)
        {
            if (string.IsNullOrEmpty(globalObjectId))
            {
                return null;
            }

            if (!GlobalObjectId.TryParse(globalObjectId, out GlobalObjectId parsedId))
            {
                return null;
            }

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(parsedId);
        }
    }
}