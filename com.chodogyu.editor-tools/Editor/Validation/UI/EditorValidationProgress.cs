using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Validation.UI
{
    /// <summary>
    /// Unity Editor의 취소 가능한 Progress Bar를 사용하는 Validation 진행 상태 표시기입니다.
    /// </summary>
    internal sealed class EditorValidationProgress : IValidationProgress
    {
        public bool Report(string title, string detail, float progress)
        {
            return EditorUtility.DisplayCancelableProgressBar(title, detail, Mathf.Clamp01(progress));
        }

        internal void Clear()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}