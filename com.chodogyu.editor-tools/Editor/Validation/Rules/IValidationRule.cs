using System.Collections.Generic;
using UnityEngine;

namespace CDG.EditorTools.Validation.Rules
{
    /// <summary>
    /// 하나의 GameObject에 대해 특정 Validation 규칙을 수행하는 내부 계약입니다.
    /// 검사 범위와 독립적으로 동작하며 발견한 문제를 전달받은 결과 컬렉션에 추가합니다.
    /// </summary>
    internal interface IValidationRule
    {
        /// <summary>
        /// 이 규칙이 검사하는 문제의 종류입니다.
        /// </summary>
        ValidationIssueType Type { get; }

        /// <summary>
        /// 지정된 GameObject를 검사하고 발견한 문제를 결과 컬렉션에 추가합니다.
        /// assetPath는 현재 Scene처럼 별도의 Asset 경로가 필요하지 않은 경우 빈 문자열일 수 있습니다.
        /// </summary>
        void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues);
    }
}