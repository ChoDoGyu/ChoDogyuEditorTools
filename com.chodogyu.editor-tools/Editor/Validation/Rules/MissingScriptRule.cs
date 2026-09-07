using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Validation.Rules
{
    /// <summary>
    /// GameObject에 연결된 MonoBehaviour 중 스크립트를 찾을 수 없는 Missing Script를 검사합니다.
    /// 하나의 GameObject에서 여러 Missing Script가 발견되더라도 하나의 문제로 묶어 보고합니다.
    /// </summary>
    internal sealed class MissingScriptRule : IValidationRule
    {
        /// <summary>
        /// 이 규칙이 검사하는 문제 종류입니다.
        /// </summary>
        public ValidationIssueType Type => ValidationIssueType.MissingScript;

        /// <summary>
        /// 지정된 GameObject의 Missing Script 개수를 검사하고 문제가 있으면 결과 컬렉션에 추가합니다.
        /// </summary>
        public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
        {
            int missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

            if (missingScriptCount <= 0)
            {
                return;
            }

            ValidationLocation location = ValidationLocationFactory.Create(
                gameObject,
                null,
                assetPath,
                string.Empty);

            ValidationIssue issue = new ValidationIssue(
                Type,
                $"Missing Script가 {missingScriptCount}개 발견되었습니다.",
                location);

            issues.Add(issue);
        }
    }
}