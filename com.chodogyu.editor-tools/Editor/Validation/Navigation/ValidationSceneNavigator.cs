using System;
using System.IO;
using CDG.EditorTools.Validation.Scopes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDG.EditorTools.Validation.Navigation
{
    /// <summary>
    /// Scene 내부에서 발견된 Validation 문제 위치로 이동합니다.
    /// 이미 로드된 Scene은 그대로 사용하고, 닫힌 Scene은 필요한 경우 Additive로 열어 문제 GameObject를 선택합니다.
    /// </summary>
    internal static class ValidationSceneNavigator
    {
        /// <summary>
        /// 지정된 Validation 위치가 Scene 내부 Object를 가리키면 해당 Scene과 GameObject로 이동합니다.
        /// 닫힌 Scene을 새로 열어야 하는 경우 현재 열린 Scene에 저장되지 않은 변경 사항이 있으면 검사를 중단합니다.
        /// </summary>
        internal static bool TryOpenAndSelect(ValidationLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (!IsScenePath(location.AssetPath))
            {
                return false;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(location.AssetPath);

            if (sceneAsset == null)
            {
                return false;
            }

            GameObject resolvedGameObject = ValidationObjectResolver.ResolveGameObject(location);

            if (resolvedGameObject != null && resolvedGameObject.scene.IsValid())
            {
                SelectAndPing(resolvedGameObject);
                return true;
            }

            ProjectScenesValidationScope.EnsureLoadedScenesAreSafe();

            return TryOpenSceneAndSelect(location);
        }

        /// <summary>
        /// Scene 안전성 선행 검사가 이미 수행되었다고 가정하고 Scene Asset을 열어 문제 GameObject를 찾습니다.
        /// 테스트와 내부 Navigation 흐름에서 사용합니다.
        /// </summary>
        internal static bool TryOpenSceneAndSelect(ValidationLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (!IsScenePath(location.AssetPath))
            {
                return false;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(location.AssetPath);

            if (sceneAsset == null)
            {
                return false;
            }

            Scene scene = SceneManager.GetSceneByPath(location.AssetPath);
            bool openedByNavigator = false;

            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(location.AssetPath, OpenSceneMode.Additive);
                openedByNavigator = true;
            }

            GameObject target = ValidationObjectResolver.ResolveGameObject(location);

            if (target == null || target.scene != scene)
            {
                target = FindByHierarchyPath(scene, location.ObjectPath);
            }

            if (target == null)
            {
                if (openedByNavigator && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }

                return false;
            }

            SceneManager.SetActiveScene(scene);
            SelectAndPing(target);

            return true;
        }

        private static GameObject FindByHierarchyPath(Scene scene, string objectPath)
        {
            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(objectPath))
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];

                if (objectPath == root.name)
                {
                    return root;
                }

                string rootPrefix = root.name + "/";

                if (!objectPath.StartsWith(rootPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string relativePath = objectPath.Substring(rootPrefix.Length);
                Transform target = root.transform.Find(relativePath);

                if (target != null)
                {
                    return target.gameObject;
                }
            }

            return null;
        }

        private static void SelectAndPing(GameObject gameObject)
        {
            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }

        private static bool IsScenePath(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && string.Equals(Path.GetExtension(assetPath), ".unity", StringComparison.OrdinalIgnoreCase);
        }
    }
}