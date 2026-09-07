using System;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    /// <summary>
    /// 정상 null, 정상 Object Reference, 삭제된 Object Reference 상태를 가진
    /// MeshFilter를 포함하는 테스트용 Prefab을 생성하고 제거합니다.
    /// </summary>
    internal static class BrokenSerializedReferenceFixture
    {
        private const string ParentFolderPath = "Assets/CDGEditorToolsTests";
        private const string TestFolderPath = "Assets/CDGEditorToolsTests/BrokenSerializedReferences";
        private const string ValidReferenceAssetPath = TestFolderPath + "/ValidReference.asset";
        private const string BrokenReferenceAssetPath = TestFolderPath + "/BrokenReference.asset";

        internal static GameObject Create(out string prefabAssetPath)
        {
            EnsureTestFolderExists();

            prefabAssetPath = TestFolderPath + "/BrokenReferenceTarget.prefab";

            Mesh validReference = new Mesh
            {
                name = "ValidReference"
            };

            Mesh brokenReference = new Mesh
            {
                name = "BrokenReference"
            };

            AssetDatabase.CreateAsset(validReference, ValidReferenceAssetPath);
            AssetDatabase.CreateAsset(brokenReference, BrokenReferenceAssetPath);

            GameObject root = new GameObject("BrokenReferenceTarget");
            GameObject normalNullObject = new GameObject("NormalNull");
            GameObject validReferenceObject = new GameObject("ValidReference");
            GameObject brokenReferenceObject = new GameObject("BrokenReference");

            try
            {
                normalNullObject.transform.SetParent(root.transform);
                validReferenceObject.transform.SetParent(root.transform);
                brokenReferenceObject.transform.SetParent(root.transform);

                MeshFilter normalNullFilter = normalNullObject.AddComponent<MeshFilter>();
                MeshFilter validReferenceFilter = validReferenceObject.AddComponent<MeshFilter>();
                MeshFilter brokenReferenceFilter = brokenReferenceObject.AddComponent<MeshFilter>();

                normalNullFilter.sharedMesh = null;
                validReferenceFilter.sharedMesh = validReference;
                brokenReferenceFilter.sharedMesh = brokenReference;

                if (PrefabUtility.SaveAsPrefabAsset(root, prefabAssetPath) == null)
                {
                    throw new InvalidOperationException($"테스트 Prefab을 저장하지 못했습니다: {prefabAssetPath}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            AssetDatabase.SaveAssets();

            if (!AssetDatabase.DeleteAsset(BrokenReferenceAssetPath))
            {
                throw new InvalidOperationException($"Broken Reference용 Asset을 삭제하지 못했습니다: {BrokenReferenceAssetPath}");
            }

            AssetDatabase.ImportAsset(
                prefabAssetPath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);

            if (prefab == null)
            {
                throw new InvalidOperationException($"테스트 Prefab을 불러오지 못했습니다: {prefabAssetPath}");
            }

            return prefab;
        }

        internal static void Delete(string prefabAssetPath)
        {
            if (!string.IsNullOrEmpty(prefabAssetPath))
            {
                AssetDatabase.DeleteAsset(prefabAssetPath);
            }

            AssetDatabase.DeleteAsset(ValidReferenceAssetPath);
            AssetDatabase.DeleteAsset(BrokenReferenceAssetPath);

            if (AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.DeleteAsset(TestFolderPath);
            }

            if (AssetDatabase.IsValidFolder(ParentFolderPath))
            {
                string[] remainingAssets = AssetDatabase.FindAssets(string.Empty, new[] { ParentFolderPath });

                if (remainingAssets.Length == 0)
                {
                    AssetDatabase.DeleteAsset(ParentFolderPath);
                }
            }
        }

        private static void EnsureTestFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(ParentFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "CDGEditorToolsTests");
            }

            if (!AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.CreateFolder(ParentFolderPath, "BrokenSerializedReferences");
            }
        }
    }
}