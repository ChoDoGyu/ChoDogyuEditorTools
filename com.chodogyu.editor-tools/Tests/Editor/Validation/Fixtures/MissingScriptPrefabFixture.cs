using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    /// <summary>
    /// Missing Script 테스트에 사용할 임시 Prefab Asset을 생성하고 제거합니다.
    /// 실제로 존재하지 않는 MonoScript GUID를 직렬화하여 Unity가 Missing Script 상태로 인식하도록 합니다.
    /// </summary>
    internal static class MissingScriptPrefabFixture
    {
        private const string TestFolderPath = "Assets/CDGEditorToolsTests";

        internal static GameObject Create(string prefabName, int missingScriptCount, bool placeOnChild, out string assetPath)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                throw new ArgumentException("Prefab 이름은 비어 있을 수 없습니다.", nameof(prefabName));
            }

            if (missingScriptCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(missingScriptCount));
            }

            EnsureTestFolderExists();

            assetPath = $"{TestFolderPath}/{prefabName}.prefab";
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);

            string yaml = placeOnChild
                ? CreateChildPrefabYaml(missingScriptCount)
                : CreateRootPrefabYaml(missingScriptCount);

            File.WriteAllText(absolutePath, yaml);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                throw new InvalidOperationException($"테스트 Prefab을 불러오지 못했습니다: {assetPath}");
            }

            return prefab;
        }

        internal static void Delete(string assetPath)
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            if (AssetDatabase.IsValidFolder(TestFolderPath))
            {
                string[] remainingAssets = AssetDatabase.FindAssets(string.Empty, new[] { TestFolderPath });

                if (remainingAssets.Length == 0)
                {
                    AssetDatabase.DeleteAsset(TestFolderPath);
                }
            }
        }

        private static void EnsureTestFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "CDGEditorToolsTests");
            }
        }

        private static string CreateRootPrefabYaml(int missingScriptCount)
        {
            string componentReferences = string.Empty;
            string missingComponents = string.Empty;

            for (int i = 0; i < missingScriptCount; i++)
            {
                long componentFileId = 11400000 + i;
                componentReferences += $"  - component: {{fileID: {componentFileId}}}\n";
                missingComponents += CreateMissingComponentYaml(componentFileId, 1000);
            }

            return
                "%YAML 1.1\n" +
                "%TAG !u! tag:unity3d.com,2011:\n" +
                "--- !u!1 &1000\n" +
                "GameObject:\n" +
                "  m_ObjectHideFlags: 0\n" +
                "  m_CorrespondingSourceObject: {fileID: 0}\n" +
                "  m_PrefabInstance: {fileID: 0}\n" +
                "  m_PrefabAsset: {fileID: 0}\n" +
                "  serializedVersion: 6\n" +
                "  m_Component:\n" +
                "  - component: {fileID: 4000}\n" +
                componentReferences +
                "  m_Layer: 0\n" +
                "  m_Name: Root\n" +
                "  m_TagString: Untagged\n" +
                "  m_Icon: {fileID: 0}\n" +
                "  m_NavMeshLayer: 0\n" +
                "  m_StaticEditorFlags: 0\n" +
                "  m_IsActive: 1\n" +
                "--- !u!4 &4000\n" +
                "Transform:\n" +
                "  m_ObjectHideFlags: 0\n" +
                "  m_CorrespondingSourceObject: {fileID: 0}\n" +
                "  m_PrefabInstance: {fileID: 0}\n" +
                "  m_PrefabAsset: {fileID: 0}\n" +
                "  m_GameObject: {fileID: 1000}\n" +
                "  serializedVersion: 2\n" +
                "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n" +
                "  m_LocalPosition: {x: 0, y: 0, z: 0}\n" +
                "  m_LocalScale: {x: 1, y: 1, z: 1}\n" +
                "  m_ConstrainProportionsScale: 0\n" +
                "  m_Children: []\n" +
                "  m_Father: {fileID: 0}\n" +
                "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n" +
                missingComponents;
        }

        private static string CreateChildPrefabYaml(int missingScriptCount)
        {
            string childComponentReferences = string.Empty;
            string missingComponents = string.Empty;

            for (int i = 0; i < missingScriptCount; i++)
            {
                long componentFileId = 11400000 + i;
                childComponentReferences += $"  - component: {{fileID: {componentFileId}}}\n";
                missingComponents += CreateMissingComponentYaml(componentFileId, 2000);
            }

            return
                "%YAML 1.1\n" +
                "%TAG !u! tag:unity3d.com,2011:\n" +
                "--- !u!1 &1000\n" +
                "GameObject:\n" +
                "  m_ObjectHideFlags: 0\n" +
                "  m_CorrespondingSourceObject: {fileID: 0}\n" +
                "  m_PrefabInstance: {fileID: 0}\n" +
                "  m_PrefabAsset: {fileID: 0}\n" +
                "  serializedVersion: 6\n" +
                "  m_Component:\n" +
                "  - component: {fileID: 4000}\n" +
                "  m_Layer: 0\n" +
                "  m_Name: Root\n" +
                "  m_TagString: Untagged\n" +
                "  m_Icon: {fileID: 0}\n" +
                "  m_NavMeshLayer: 0\n" +
                "  m_StaticEditorFlags: 0\n" +
                "  m_IsActive: 1\n" +
                "--- !u!4 &4000\n" +
                "Transform:\n" +
                "  m_ObjectHideFlags: 0\n" +
                "  m_CorrespondingSourceObject: {fileID: 0}\n" +
                "  m_PrefabInstance: {fileID: 0}\n" +
                "  m_PrefabAsset: {fileID: 0}\n" +
                "  m_GameObject: {fileID: 1000}\n" +
                "  serializedVersion: 2\n" +
                "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n" +
                "  m_LocalPosition: {x: 0, y: 0, z: 0}\n" +
                "  m_LocalScale: {x: 1, y: 1, z: 1}\n" +
                "  m_ConstrainProportionsScale: 0\n" +
                "  m_Children:\n" +
                "  - {fileID: 5000}\n" +
                "  m_Father: {fileID: 0}\n" +
                "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n" +
                "--- !u!1 &2000\n" +
                "GameObject:\n" +
                "  m_ObjectHideFlags: 0\n" +
                "  m_CorrespondingSourceObject: {fileID: 0}\n" +
                "  m_PrefabInstance: {fileID: 0}\n" +
                "  m_PrefabAsset: {fileID: 0}\n" +
                "  serializedVersion: 6\n" +
                "  m_Component:\n" +
                "  - component: {fileID: 5000}\n" +
                childComponentReferences +
                "  m_Layer: 0\n" +
                "  m_Name: Child\n" +
                "  m_TagString: Untagged\n" +
                "  m_Icon: {fileID: 0}\n" +
                "  m_NavMeshLayer: 0\n" +
                "  m_StaticEditorFlags: 0\n" +
                "  m_IsActive: 1\n" +
                "--- !u!4 &5000\n" +
                "Transform:\n" +
                "  m_ObjectHideFlags: 0\n" +
                "  m_CorrespondingSourceObject: {fileID: 0}\n" +
                "  m_PrefabInstance: {fileID: 0}\n" +
                "  m_PrefabAsset: {fileID: 0}\n" +
                "  m_GameObject: {fileID: 2000}\n" +
                "  serializedVersion: 2\n" +
                "  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n" +
                "  m_LocalPosition: {x: 0, y: 0, z: 0}\n" +
                "  m_LocalScale: {x: 1, y: 1, z: 1}\n" +
                "  m_ConstrainProportionsScale: 0\n" +
                "  m_Children: []\n" +
                "  m_Father: {fileID: 4000}\n" +
                "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n" +
                missingComponents;
        }

        private static string CreateMissingComponentYaml(long componentFileId, long gameObjectFileId)
        {
            return
                $"--- !u!114 &{componentFileId}\n" +
                "MonoBehaviour:\n" +
                "  m_ObjectHideFlags: 0\n" +
                "  m_CorrespondingSourceObject: {fileID: 0}\n" +
                "  m_PrefabInstance: {fileID: 0}\n" +
                "  m_PrefabAsset: {fileID: 0}\n" +
                $"  m_GameObject: {{fileID: {gameObjectFileId}}}\n" +
                "  m_Enabled: 1\n" +
                "  m_EditorHideFlags: 0\n" +
                "  m_Script: {fileID: 11500000, guid: ffffffffffffffffffffffffffffffff, type: 3}\n" +
                "  m_Name:\n" +
                "  m_EditorClassIdentifier:\n";
        }
    }
}