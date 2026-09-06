using System;

namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// Validation 과정에서 발견된 하나의 문제를 나타냅니다.
    /// 문제 종류와 설명, 실제 문제 위치를 함께 보관합니다.
    /// </summary>
    internal sealed class ValidationIssue
    {
        /// <summary>
        /// 발견된 Validation 문제의 종류입니다.
        /// </summary>
        internal ValidationIssueType Type { get; }

        /// <summary>
        /// 사용자에게 표시할 문제 설명입니다.
        /// </summary>
        internal string Message { get; }

        /// <summary>
        /// 문제가 발견된 Asset 및 Object 위치 정보입니다.
        /// </summary>
        internal ValidationLocation Location { get; }

        internal ValidationIssue(ValidationIssueType type, string message, ValidationLocation location)
        {
            Type = type;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Location = location ?? throw new ArgumentNullException(nameof(location));
        }
    }
}