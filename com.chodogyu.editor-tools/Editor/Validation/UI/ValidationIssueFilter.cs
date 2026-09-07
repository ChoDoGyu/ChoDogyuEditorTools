using System;

namespace CDG.EditorTools.Validation.UI
{
    /// <summary>
    /// Validation 결과의 문제 종류와 검색어를 기준으로 화면 표시 여부를 판정합니다.
    /// </summary>
    internal static class ValidationIssueFilter
    {
        /// <summary>
        /// 지정된 Validation 문제가 현재 결과 필터와 검색 조건에 일치하는지 확인합니다.
        /// </summary>
        internal static bool Matches(ValidationIssue issue, string searchText, bool showMissingScript, bool showBrokenSerializedReference)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (!MatchesIssueType(issue.Type, showMissingScript, showBrokenSerializedReference))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            string trimmedSearchText = searchText.Trim();

            return Contains(issue.Location.AssetPath, trimmedSearchText)
                || Contains(issue.Location.ObjectPath, trimmedSearchText)
                || Contains(issue.Location.ComponentName, trimmedSearchText)
                || Contains(issue.Location.PropertyPath, trimmedSearchText)
                || Contains(issue.Message, trimmedSearchText)
                || Contains(GetIssueTypeDisplayName(issue.Type), trimmedSearchText);
        }

        private static bool MatchesIssueType(ValidationIssueType issueType, bool showMissingScript, bool showBrokenSerializedReference)
        {
            switch (issueType)
            {
                case ValidationIssueType.MissingScript:
                    return showMissingScript;

                case ValidationIssueType.BrokenSerializedReference:
                    return showBrokenSerializedReference;

                default:
                    return true;
            }
        }

        private static bool Contains(string value, string searchText)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetIssueTypeDisplayName(ValidationIssueType issueType)
        {
            switch (issueType)
            {
                case ValidationIssueType.MissingScript:
                    return "Missing Script";

                case ValidationIssueType.BrokenSerializedReference:
                    return "Broken Serialized Reference";

                default:
                    return issueType.ToString();
            }
        }
    }
}