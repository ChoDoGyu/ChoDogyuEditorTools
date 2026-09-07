namespace CDG.EditorTools.Validation.UI
{
    /// <summary>
    /// Validation Window의 현재 작업 상태를 나타냅니다.
    /// </summary>
    internal enum ValidationWindowState
    {
        Ready,
        Validating,
        Completed,
        Canceled,
        Failed
    }
}