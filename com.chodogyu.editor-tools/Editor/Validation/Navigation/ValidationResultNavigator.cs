using System;
using System.IO;

namespace CDG.EditorTools.Validation.Navigation
{
    /// <summary>
    /// Validation 결과의 위치 정보를 기준으로 적절한 Editor Navigation 방식을 선택하고 실행합니다.
    /// UI는 대상이 Scene인지 Prefab인지 직접 판단하지 않고 이 클래스를 통해 이동을 요청합니다.
    /// </summary>
    internal static class ValidationResultNavigator
    {
        /// <summary>
        /// 지정된 Validation 문제의 위치로 이동합니다.
        /// 현재 Object, Prefab 또는 Scene에 맞는 Navigation을 자동으로 선택하며 이동할 수 없으면 false를 반환합니다.
        /// </summary>
        internal static bool TryNavigate(ValidationIssue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            ValidationNavigationTarget target = GetNavigationTarget(issue.Location);

            switch (target)
            {
                case ValidationNavigationTarget.Prefab:
                    return ValidationPrefabNavigator.TryOpenAndSelect(issue.Location);

                case ValidationNavigationTarget.Scene:
                    return ValidationSceneNavigator.TryOpenAndSelect(issue.Location);

                default:
                    return ValidationLoadedObjectNavigator.TrySelectAndPing(issue.Location);
            }
        }

        /// <summary>
        /// Validation 위치의 Asset Path를 기준으로 사용할 Navigation 종류를 결정합니다.
        /// </summary>
        internal static ValidationNavigationTarget GetNavigationTarget(ValidationLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            string extension = Path.GetExtension(location.AssetPath);

            if (string.Equals(extension, ".prefab", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationNavigationTarget.Prefab;
            }

            if (string.Equals(extension, ".unity", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationNavigationTarget.Scene;
            }

            return ValidationNavigationTarget.LoadedObject;
        }
    }
}