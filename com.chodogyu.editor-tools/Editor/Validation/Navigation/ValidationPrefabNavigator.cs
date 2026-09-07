using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CDG.EditorTools.Validation.Navigation
{
    /// <summary>
    /// Prefab Asset 내부에서 발견된 Validation 문제 위치를 Prefab Mode로 열고 해당 GameObject를 선택합니다.
    /// Global Object ID를 우선 사용하며, Prefab Stage 내부 매핑이 불가능한 경우 Hierarchy 경로를 보조적으로 사용합니다.
    /// </summary>
    internal static class ValidationPrefabNavigator
    {
        /// <summary>
        /// Validation 위치가 유효한 Prefab Asset을 가리키면 Prefab Mode를 열고 문제 GameObject를 선택합니다.
        /// 대상 Prefab이나 문제 Object를 찾을 수 없으면 false를 반환합니다.
        /// </summary>
        internal static bool TryOpenAndSelect(ValidationLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (!IsPrefabPath(location.AssetPath))
            {
                return false;
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(location.AssetPath);

            if (prefabAsset == null)
            {
                return false;
            }

            GameObject sourceGameObject = ValidationObjectResolver.ResolveGameObject(location);

            if (sourceGameObject == null)
            {
                return false;
            }

            PrefabStage prefabStage = PrefabStageUtility.OpenPrefab(location.AssetPath);

            if (prefabStage == null || prefabStage.prefabContentsRoot == null)
            {
                return false;
            }

            GameObject target = FindStageObject(prefabStage.prefabContentsRoot, sourceGameObject, location.AssetPath);

            if (target == null)
            {
                target = FindByHierarchyPath(prefabStage.prefabContentsRoot, location.ObjectPath);
            }

            if (target == null)
            {
                StageUtility.GoBackToPreviousStage();
                return false;
            }

            Selection.activeGameObject = target;
            EditorGUIUtility.PingObject(target);

            return true;
        }

        private static GameObject FindStageObject(GameObject root, GameObject sourceGameObject, string prefabAssetPath)
        {
            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(root.transform);

            while (stack.Count > 0)
            {
                Transform current = stack.Pop();

                GameObject correspondingObject = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(current.gameObject, prefabAssetPath);

                if (correspondingObject == sourceGameObject)
                {
                    return current.gameObject;
                }

                for (int i = current.childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }
            }

            return null;
        }

        private static GameObject FindByHierarchyPath(GameObject root, string objectPath)
        {
            if (root == null || string.IsNullOrEmpty(objectPath))
            {
                return null;
            }

            if (objectPath == root.name)
            {
                return root;
            }

            string rootPrefix = root.name + "/";

            if (!objectPath.StartsWith(rootPrefix, StringComparison.Ordinal))
            {
                return null;
            }

            string relativePath = objectPath.Substring(rootPrefix.Length);
            Transform target = root.transform.Find(relativePath);

            return target != null ? target.gameObject : null;
        }

        private static bool IsPrefabPath(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath)
                && string.Equals(Path.GetExtension(assetPath), ".prefab", StringComparison.OrdinalIgnoreCase);
        }
    }
}