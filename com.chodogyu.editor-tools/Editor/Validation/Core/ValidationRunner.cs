using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation.Rules;
using UnityEngine;

namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// 하나의 GameObject에 선택된 Validation 규칙들을 실행하고 발견된 문제를 결과 컬렉션에 누적합니다.
    /// 검사 대상의 탐색과 범위 결정은 담당하지 않습니다.
    /// </summary>
    internal sealed class ValidationRunner
    {
        /// <summary>
        /// 지정된 GameObject에 모든 Validation 규칙을 실행합니다.
        /// 기존 결과 컬렉션을 초기화하지 않고 새로 발견된 문제만 추가합니다.
        /// </summary>
        internal void Validate(GameObject gameObject, string assetPath, IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
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

            for (int i = 0; i < rules.Count; i++)
            {
                IValidationRule rule = rules[i];

                if (rule == null)
                {
                    throw new ArgumentException("Validation 규칙 컬렉션에는 null이 포함될 수 없습니다.", nameof(rules));
                }

                rule.Validate(gameObject, assetPath, issues);
            }
        }
    }
}