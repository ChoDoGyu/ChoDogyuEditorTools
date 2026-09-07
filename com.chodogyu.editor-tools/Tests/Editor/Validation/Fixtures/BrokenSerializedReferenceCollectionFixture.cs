using System;
using CDG.EditorTools.Tests.Fixtures;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    /// <summary>
    /// Array, List 및 중첩 Serializable 타입에 각각 Broken Object Reference를 가진
    /// 테스트용 Prefab을 생성하고 제거합니다.
    /// </summary>
    internal static class BrokenSerializedReferenceCollectionFixture
    {
        private const string ParentFolderPath = "Assets/CDGEditorToolsTests";
        private const string TestFolderPath = "Assets/CDGEditorToolsTests/BrokenSerializedReferenceCollections";
        private const string ArrayReferenceAssetPath = TestFolderPath + "/ArrayReference.asset";
        private const string ListReferenceAssetPath = TestFolderPath + "/ListReference.asset";
        private const string NestedReferenceAssetPath = TestFolderPath + "/NestedReference.asset";

        internal static GameObject Create(out string prefabAssetPath)
        {
            EnsureTestFolderExists();

            prefabAssetPath = TestFolderPath + "/BrokenSerializedReferenceCollections.prefab";

            Mesh arrayReference = CreateMeshAsset("ArrayReference", ArrayReferenceAssetPath);
            Mesh listReference = CreateMeshAsset("ListReference", ListReferenceAssetPath);
            Mesh nestedReference = CreateMeshAsset("NestedReference", NestedReferenceAssetPath);

            GameObject root = new GameObject("BrokenSerializedReferenceCollections");

            try
            {
                SerializedReferenceCollectionTestComponent component = root.AddComponent<SerializedReferenceCollectionTestComponent>();
                SerializedObject serializedObject = new SerializedObject(component);

                SerializedProperty arrayProperty = serializedObject.FindProperty("_arrayReferences");
                SerializedProperty listProperty = serializedObject.FindProperty("_listReferences");
                SerializedProperty nestedReferenceProperty = serializedObject.FindProperty("_nestedData._reference");

                if (arrayProperty == null || listProperty == null || nestedReferenceProperty == null)
                {
                    throw new InvalidOperationException("테스트용 Serialized Property를 찾지 못했습니다.");
                }

                arrayProperty.arraySize = 1;
                arrayProperty.GetArrayElementAtIndex(0).objectReferenceValue = arrayReference;

                listProperty.arraySize = 1;
                listProperty.GetArrayElementAtIndex(0).objectReferenceValue = listReference;

                nestedReferenceProperty.objectReferenceValue = nestedReference;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();

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

            DeleteReferenceAsset(ArrayReferenceAssetPath);
            DeleteReferenceAsset(ListReferenceAssetPath);
            DeleteReferenceAsset(NestedReferenceAssetPath);

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

            AssetDatabase.DeleteAsset(ArrayReferenceAssetPath);
            AssetDatabase.DeleteAsset(ListReferenceAssetPath);
            AssetDatabase.DeleteAsset(NestedReferenceAssetPath);

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

        private static Mesh CreateMeshAsset(string name, string assetPath)
        {
            Mesh mesh = new Mesh
            {
                name = name
            };

            AssetDatabase.CreateAsset(mesh, assetPath);

            return mesh;
        }

        private static void DeleteReferenceAsset(string assetPath)
        {
            if (!AssetDatabase.DeleteAsset(assetPath))
            {
                throw new InvalidOperationException($"Broken Reference용 Asset을 삭제하지 못했습니다: {assetPath}");
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
                AssetDatabase.CreateFolder(ParentFolderPath, "BrokenSerializedReferenceCollections");
            }
        }
    }
}