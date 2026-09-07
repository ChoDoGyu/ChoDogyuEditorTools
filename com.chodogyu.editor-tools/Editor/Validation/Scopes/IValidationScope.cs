using System.Collections.Generic;
using CDG.EditorTools.Validation.Rules;

namespace CDG.EditorTools.Validation.Scopes
{
    /// <summary>
    /// Validation이 검사할 Unity 대상 범위를 정의하는 내부 계약입니다.
    /// 각 Scope는 검사 대상을 찾는 책임만 가지며 실제 문제 판별은 Validation Rule에 위임합니다.
    /// </summary>
    internal interface IValidationScope
    {
        /// <summary>
        /// 현재 Scope에 포함되는 대상을 찾아 지정된 Validation 규칙들을 실행합니다.
        /// 발견된 문제는 기존 결과 컬렉션에 누적됩니다.
        /// </summary>
        void Validate(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues);
    }
}