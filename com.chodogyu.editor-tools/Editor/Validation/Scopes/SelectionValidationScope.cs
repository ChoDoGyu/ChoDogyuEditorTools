using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation.Rules;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Validation.Scopes
{
    /// <summary>
    /// Unity Editor에서 현재 선택된 GameObject, Component 또는 Prefab Asset을 검사합니다.
    /// 부모와 자식이 동시에 선택된 경우 부모 Hierarchy에서 한 번만 검사하여 중복 결과를 방지합니다.
    /// </summary>
    internal sealed class SelectionValidationScope : IValidationScope
    {
        private readonly HierarchyValidationRunner _hierarchyValidationRunner = new HierarchyValidationRunner();

        /// <summary>
        /// 현재 Unity Selection에서 검사 가능한 Root를 찾고 각 Hierarchy에 Validation 규칙을 실행합니다.
        /// GameObject와 Component, Prefab Asset을 지원하며 다른 종류의 Asset은 무시합니다.
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

            List<GameObject> selectedRoots = GetSelectedRoots();

            for (int i = 0; i < selectedRoots.Count; i++)
            {
                GameObject root = selectedRoots[i];
                string assetPath = GetAssetPath(root);

                _hierarchyValidationRunner.Validate(root, assetPath, rules, issues);
            }
        }

        private static List<GameObject> GetSelectedRoots()
        {
            UnityEngine.Object[] selectedObjects = Selection.objects;
            List<GameObject> candidates = new List<GameObject>();
            HashSet<GameObject> candidateSet = new HashSet<GameObject>();

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                GameObject gameObject = GetGameObject(selectedObjects[i]);

                if (gameObject == null)
                {
                    continue;
                }

                if (EditorUtility.IsPersistent(gameObject) && !PrefabUtility.IsPartOfPrefabAsset(gameObject))
                {
                    continue;
                }

                if (candidateSet.Add(gameObject))
                {
                    candidates.Add(gameObject);
                }
            }

            List<GameObject> roots = new List<GameObject>();

            for (int i = 0; i < candidates.Count; i++)
            {
                GameObject candidate = candidates[i];

                if (!HasSelectedAncestor(candidate, candidateSet))
                {
                    roots.Add(candidate);
                }
            }

            return roots;
        }

        private static GameObject GetGameObject(UnityEngine.Object selectedObject)
        {
            if (selectedObject is GameObject gameObject)
            {
                return gameObject;
            }

            if (selectedObject is Component component)
            {
                return component.gameObject;
            }

            return null;
        }

        private static bool HasSelectedAncestor(GameObject gameObject, HashSet<GameObject> selectedObjects)
        {
            Transform parent = gameObject.transform.parent;

            while (parent != null)
            {
                if (selectedObjects.Contains(parent.gameObject))
                {
                    return true;
                }

                parent = parent.parent;
            }

            return false;
        }

        private static string GetAssetPath(GameObject gameObject)
        {
            if (EditorUtility.IsPersistent(gameObject))
            {
                return AssetDatabase.GetAssetPath(gameObject) ?? string.Empty;
            }

            if (gameObject.scene.IsValid())
            {
                return gameObject.scene.path ?? string.Empty;
            }

            return string.Empty;
        }
    }
}