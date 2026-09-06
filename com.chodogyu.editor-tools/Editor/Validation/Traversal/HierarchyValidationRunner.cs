using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation.Rules;
using UnityEngine;

namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// Root GameObject의 전체 Hierarchy를 순회하면서 각 GameObject에 Validation 규칙을 실행합니다.
    /// 검사 범위에서 전달된 하나의 Root Hierarchy를 처리하는 역할만 담당합니다.
    /// </summary>
    internal sealed class HierarchyValidationRunner
    {
        private readonly ValidationRunner _validationRunner = new ValidationRunner();

        /// <summary>
        /// Root GameObject와 모든 자식을 순회하며 지정된 Validation 규칙들을 실행합니다.
        /// 발견된 문제는 기존 결과 컬렉션에 계속 누적됩니다.
        /// </summary>
        internal void Validate(GameObject root, string assetPath, IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (assetPath == null)
            {
                throw new ArgumentNullException(nameof(assetPath));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (issues == null)
            {
                throw new ArgumentNullException(nameof(issues));
            }

            GameObjectHierarchyTraversal.Traverse(root, gameObject =>
            {
                _validationRunner.Validate(gameObject, assetPath, rules, issues);
            });
        }
    }
}