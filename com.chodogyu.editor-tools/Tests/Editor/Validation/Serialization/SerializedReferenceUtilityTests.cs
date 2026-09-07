using CDG.EditorTools.Validation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;


namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class SerializedReferenceUtilityTests
    {
        private string _prefabAssetPath;
        private MeshFilter _normalNullFilter;
        private MeshFilter _validReferenceFilter;
        private MeshFilter _brokenReferenceFilter;

        [SetUp]
        public void SetUp()
        {
            GameObject prefab = BrokenSerializedReferenceFixture.Create(out _prefabAssetPath);

            _normalNullFilter = prefab.transform.Find("NormalNull").GetComponent<MeshFilter>();
            _validReferenceFilter = prefab.transform.Find("ValidReference").GetComponent<MeshFilter>();
            _brokenReferenceFilter = prefab.transform.Find("BrokenReference").GetComponent<MeshFilter>();
        }

        [TearDown]
        public void TearDown()
        {
            BrokenSerializedReferenceFixture.Delete(_prefabAssetPath);
        }

        [Test]
        public void IsBrokenObjectReference_WithNormalNull_ReturnsFalse()
        {
            SerializedObject serializedObject = new SerializedObject(_normalNullFilter);
            SerializedProperty property = serializedObject.FindProperty("m_Mesh");

            Assert.IsFalse(SerializedReferenceUtility.IsBrokenObjectReference(property));
        }

        [Test]
        public void IsBrokenObjectReference_WithValidReference_ReturnsFalse()
        {
            SerializedObject serializedObject = new SerializedObject(_validReferenceFilter);
            SerializedProperty property = serializedObject.FindProperty("m_Mesh");

            Assert.IsFalse(SerializedReferenceUtility.IsBrokenObjectReference(property));
        }

        [Test]
        public void IsBrokenObjectReference_WithBrokenReference_ReturnsTrue()
        {
            SerializedObject serializedObject = new SerializedObject(_brokenReferenceFilter);
            SerializedProperty property = serializedObject.FindProperty("m_Mesh");

            Assert.IsTrue(SerializedReferenceUtility.IsBrokenObjectReference(property));
        }

        [Test]
        public void IsBrokenObjectReference_WithNullProperty_ReturnsFalse()
        {
            Assert.IsFalse(SerializedReferenceUtility.IsBrokenObjectReference(null));
        }
    }
}