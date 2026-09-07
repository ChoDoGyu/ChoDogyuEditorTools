using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation.Rules;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDG.EditorTools.Validation.Scopes
{
    /// <summary>
    /// 현재 Unity Editor에 로드되어 있는 모든 Scene의 Root GameObject와 그 Hierarchy를 검사합니다.
    /// Multi-Scene Editing 상태에서는 로드된 각 Scene을 모두 검사합니다.
    /// </summary>
    internal sealed class LoadedScenesValidationScope : IValidationScope
    {
        private readonly HierarchyValidationRunner _hierarchyValidationRunner = new HierarchyValidationRunner();

        /// <summary>
        /// 현재 로드된 모든 유효한 Scene을 순회하며 각 Root Hierarchy에 Validation 규칙을 실행합니다.
        /// 저장되지 않은 Scene은 Asset Path를 빈 문자열로 전달합니다.
        /// </summary>
        public void Validate(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (issues == null)
            {
                throw new ArgumentNullException(nameof(issues));
            }

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);

                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                string assetPath = scene.path ?? string.Empty;

                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    _hierarchyValidationRunner.Validate(roots[rootIndex], assetPath, rules, issues);
                }
            }
        }
    }
}