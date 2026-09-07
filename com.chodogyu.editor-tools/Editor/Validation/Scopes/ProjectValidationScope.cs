using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation.Rules;

namespace CDG.EditorTools.Validation.Scopes
{
    /// <summary>
    /// Unity 프로젝트의 Assets 영역에 존재하는 Prefab과 Scene을 모두 검사합니다.
    /// 각 Asset 종류의 탐색과 검사는 전용 Scope에 위임합니다.
    /// </summary>
    internal sealed class ProjectValidationScope : IValidationScope
    {
        private readonly ProjectPrefabsValidationScope _prefabsScope;
        private readonly ProjectScenesValidationScope _scenesScope;

        internal ProjectValidationScope(IValidationProgress progress = null)
        {
            _prefabsScope = new ProjectPrefabsValidationScope(progress);
            _scenesScope = new ProjectScenesValidationScope(progress);
        }

        /// <summary>
        /// Scene 안전성을 먼저 확인한 뒤 Project Prefab과 Project Scene을 순서대로 검사합니다.
        /// </summary>
        public void Validate(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            ValidateArguments(rules, issues);

            ProjectScenesValidationScope.EnsureLoadedScenesAreSafe();
            ValidateProjectAssets(rules, issues);
        }

        /// <summary>
        /// Scene 안전성 선행 검사를 수행하지 않고 Project Prefab과 Project Scene을 검사합니다.
        /// 안전성 검사가 이미 수행된 내부 흐름과 테스트에서 사용합니다.
        /// </summary>
        internal void ValidateProjectAssets(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            ValidateArguments(rules, issues);

            _prefabsScope.Validate(rules, issues);
            _scenesScope.ValidateSceneAssets(rules, issues);
        }

        private static void ValidateArguments(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (issues == null)
            {
                throw new ArgumentNullException(nameof(issues));
            }
        }
    }
}