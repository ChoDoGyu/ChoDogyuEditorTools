namespace CDG.EditorTools.Validation.Navigation
{
    /// <summary>
    /// Validation 결과가 이동해야 하는 Unity Editor 대상 종류를 나타냅니다.
    /// </summary>
    internal enum ValidationNavigationTarget
    {
        LoadedObject,
        Prefab,
        Scene
    }
}