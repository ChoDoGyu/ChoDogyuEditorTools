namespace CDG.EditorTools.Validation
{
    /// <summary>
    /// 시간이 오래 걸릴 수 있는 Validation 작업의 진행 상태와 취소 요청을 전달합니다.
    /// </summary>
    internal interface IValidationProgress
    {
        /// <summary>
        /// 현재 진행 상태를 보고합니다. 사용자가 취소를 요청했다면 true를 반환합니다.
        /// </summary>
        bool Report(string title, string detail, float progress);
    }
}