using System;
using System.Collections.Generic;
using System.IO;
using CDG.EditorTools.Validation.Rules;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Validation.Scopes
{
    /// <summary>
    /// Unity 프로젝트의 Assets 폴더에 존재하는 모든 Prefab Asset을 검사합니다.
    /// Packages 등 외부 패키지 영역은 검사하지 않습니다.
    /// </summary>
    internal sealed class ProjectPrefabsValidationScope : IValidationScope
    {
        private readonly HierarchyValidationRunner _hierarchyValidationRunner = new HierarchyValidationRunner();
        private readonly IValidationProgress _progress;

        internal ProjectPrefabsValidationScope(IValidationProgress progress = null)
        {
            _progress = progress;
        }

        /// <summary>
        /// Assets 폴더의 모든 .prefab Asset을 찾아 지정된 Validation 규칙을 실행합니다.
        /// </summary>
        public void Validate(IReadOnlyList<IValidationRule> rules, ICollection<ValidationIssue> issues)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (issues == null)
            {
                throw new ArgumentNullException(nameof(issues));
            }

            List<string> prefabPaths = GetPrefabPaths();

            for (int i = 0; i < prefabPaths.Count; i++)
            {
                string prefabPath = prefabPaths[i];
                ReportProgress(prefabPath, i, prefabPaths.Count);

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                {
                    continue;
                }

                _hierarchyValidationRunner.Validate(prefab, prefabPath, rules, issues);
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

            if (_progress.Report("CDG Validation - Project Prefabs", detail, progress))
            {
                throw new OperationCanceledException("Validation이 사용자에 의해 취소되었습니다.");
            }
        }

        private static List<string> GetPrefabPaths()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            List<string> paths = new List<string>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!string.Equals(Path.GetExtension(path), ".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                paths.Add(path);
            }

            paths.Sort(StringComparer.Ordinal);

            return paths;
        }
    }
}