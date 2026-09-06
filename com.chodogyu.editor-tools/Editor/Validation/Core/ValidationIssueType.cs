namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// General Editor Tools가 보고할 수 있는 Validation 문제의 종류를 나타냅니다.
    /// </summary>
    internal enum ValidationIssueType
    {
        MissingScript,
        BrokenSerializedReference
    }
}