using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Validation.Rules
{
    /// <summary>
    /// GameObject에 연결된 Component의 SerializedProperty를 검사하여
    /// 삭제된 Unity Object를 참조하는 깨진 Object Reference를 탐지합니다.
    /// </summary>
    internal sealed class BrokenSerializedReferenceRule : IValidationRule
    {
        /// <summary>
        /// 이 규칙이 검사하는 문제 종류입니다.
        /// </summary>
        public ValidationIssueType Type => ValidationIssueType.BrokenSerializedReference;

        /// <summary>
        /// 지정된 GameObject의 모든 정상 Component를 검사하고 깨진 Object Reference마다 문제를 추가합니다.
        /// Missing Script는 별도의 Validation 규칙이 담당하므로 null Component는 건너뜁니다.
        /// </summary>
        public void Validate(GameObject gameObject, string assetPath, ICollection<ValidationIssue> issues)
        {
            Component[] components = gameObject.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];

                if (component == null)
                {
                    continue;
                }

                ValidateComponent(gameObject, component, assetPath, issues);
            }
        }

        private static void ValidateComponent(GameObject gameObject, Component component, string assetPath, ICollection<ValidationIssue> issues)
        {
            SerializedObject serializedObject = new SerializedObject(component);
            serializedObject.Update();

            SerializedProperty iterator = serializedObject.GetIterator();

            while (iterator.Next(true))
            {
                if (!SerializedReferenceUtility.IsBrokenObjectReference(iterator))
                {
                    continue;
                }

                ValidationLocation location = ValidationLocationFactory.Create(
                    gameObject,
                    component,
                    assetPath,
                    iterator.propertyPath);

                issues.Add(new ValidationIssue(
                    ValidationIssueType.BrokenSerializedReference,
                    "삭제된 Unity Object를 참조하는 Serialized Reference가 발견되었습니다.",
                    location));
            }
        }
    }
}