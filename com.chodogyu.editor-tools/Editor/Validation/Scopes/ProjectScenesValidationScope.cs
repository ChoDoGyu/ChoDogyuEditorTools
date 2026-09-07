using System;
using System.Collections.Generic;
using System.IO;
using CDG.EditorTools.Validation.Rules;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDG.EditorTools.Validation.Scopes
{
    /// <summary>
    /// Unity 프로젝트의 Assets 폴더에 존재하는 모든 Scene Asset을 검사합니다.
    /// 저장되지 않은 Scene이나 변경 사항이 있으면 일반 Project Scene 검사를 시작하지 않습니다.
    /// </summary>
    internal sealed class ProjectScenesValidationScope : IValidationScope
    {
        private const string UnsafeSceneStateMessage = "저장되지 않은 Scene 또는 변경 사항이 있어 Project Scene Validation을 실행할 수 없습니다. 열린 Scene을 모두 저장한 뒤 다시 실행해주세요.";

        private readonly HierarchyValidationRunner _hierarchyValidationRunner = new HierarchyValidationRunner();
        private readonly IValidationProgress _progress;

        internal ProjectScenesValidationScope(IValidationProgress progress = null)
        {
            _progress = progress;
        }

        /// <summary>
        /// 현재 열린 Scene 상태가 안전한지 확인한 뒤 Assets 폴더의 모든 .unity Scene을 검사합니다.
        /// </summary>
        public void Validate(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            ValidateArguments(rules, issues);
            EnsureLoadedScenesAreSafe();
            ValidateSceneAssets(rules, issues);
        }

        /// <summary>
        /// Scene 안전성 선행 검사를 수행하지 않고 Project Scene Asset을 검사합니다.
        /// 안전성 검사가 이미 수행된 내부 흐름과 테스트에서 사용합니다.
        /// </summary>
        internal void ValidateSceneAssets(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            ValidateArguments(rules, issues);

            Scene previousActiveScene = SceneManager.GetActiveScene();
            List<string> scenePaths = GetScenePaths();

            try
            {
                for (int i = 0; i < scenePaths.Count; i++)
                {
                    ReportProgress(scenePaths[i], i, scenePaths.Count);
                    ValidateScenePath(scenePaths[i], rules, issues);
                }
            }
            finally
            {
                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousActiveScene);
                }
            }
        }

        /// <summary>
        /// 현재 로드된 일반 Scene에 저장되지 않은 상태가 있는지 검사합니다.
        /// Unity의 Preview Scene은 사용자 작업 Scene이 아니므로 검사 대상에서 제외합니다.
        /// </summary>
        internal static void EnsureLoadedScenesAreSafe()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.IsValid() || !scene.isLoaded || EditorSceneManager.IsPreviewScene(scene))
                {
                    continue;
                }

                EnsureSceneIsSafe(scene.path, scene.isDirty);
            }
        }

        /// <summary>
        /// 하나의 Scene 상태가 Project Scene Validation을 실행하기에 안전한지 확인합니다.
        /// </summary>
        internal static void EnsureSceneIsSafe(string scenePath, bool isDirty)
        {
            if (string.IsNullOrEmpty(scenePath) || isDirty)
            {
                throw new InvalidOperationException(UnsafeSceneStateMessage);
            }
        }

        private void ValidateScenePath(string scenePath, IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            Scene loadedScene = SceneManager.GetSceneByPath(scenePath);

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                ValidateScene(loadedScene, scenePath, rules, issues);
                return;
            }

            Scene openedScene = default;

            try
            {
                openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                ValidateScene(openedScene, scenePath, rules, issues);
            }
            finally
            {
                if (openedScene.IsValid() && openedScene.isLoaded)
                {
                    EditorSceneManager.CloseScene(openedScene, true);
                }
            }
        }

        private void ValidateScene(Scene scene, string scenePath, IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                _hierarchyValidationRunner.Validate(roots[i], scenePath, rules, issues);
            }
        }

        private void ReportProgress(string assetPath, int index, int totalCount)
        {
            if (_progress == null)
            {
                return;
            }

            float progress = totalCount == 0 ? 1f : (float)index / totalCount;
            string detail = $"{index + 1}/{totalCount}  {assetPath}";

            if (_progress.Report("CDG Validation - Project Scenes", detail, progress))
            {
                throw new OperationCanceledException("Validation이 사용자에 의해 취소되었습니다.");
            }
        }

        private static List<string> GetScenePaths()
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            List<string> paths = new List<string>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!string.Equals(Path.GetExtension(path), ".unity", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                paths.Add(path);
            }

            paths.Sort(StringComparer.Ordinal);

            return paths;
        }

        private static void ValidateArguments(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (issues == null)
            {
                throw new ArgumentNullException(nameof(issues));
            }
        }
    }
}