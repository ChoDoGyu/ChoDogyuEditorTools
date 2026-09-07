using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CDG.EditorTools.Tests.Validation
{
    internal sealed class SerializedReferenceStateTests
    {
        private string _prefabAssetPath;
        private MeshFilter _normalNullFilter;
        private MeshFilter _validReferenceFilter;
        private MeshFilter _brokenReferenceFilter;

        [SetUp]
        public void SetUp()
        {
            GameObject prefab = BrokenSerializedReferenceFixture.Create(out _prefabAssetPath);

            Transform normalNullTransform = prefab.transform.Find("NormalNull");
            Transform validReferenceTransform = prefab.transform.Find("ValidReference");
            Transform brokenReferenceTransform = prefab.transform.Find("BrokenReference");

            Assert.IsNotNull(normalNullTransform);
            Assert.IsNotNull(validReferenceTransform);
            Assert.IsNotNull(brokenReferenceTransform);

            _normalNullFilter = normalNullTransform.GetComponent<MeshFilter>();
            _validReferenceFilter = validReferenceTransform.GetComponent<MeshFilter>();
            _brokenReferenceFilter = brokenReferenceTransform.GetComponent<MeshFilter>();

            Assert.IsNotNull(_normalNullFilter);
            Assert.IsNotNull(_validReferenceFilter);
            Assert.IsNotNull(_brokenReferenceFilter);
        }

        [TearDown]
        public void TearDown()
        {
            BrokenSerializedReferenceFixture.Delete(_prefabAssetPath);
        }

        [Test]
        public void NormalNullReference_HasNullValueAndZeroInstanceId()
        {
            SerializedObject serializedObject = new SerializedObject(_normalNullFilter);
            serializedObject.Update();

            SerializedProperty property = serializedObject.FindProperty("m_Mesh");

            Assert.IsNotNull(property);
            Assert.AreEqual(SerializedPropertyType.ObjectReference, property.propertyType);
            Assert.IsTrue(property.objectReferenceValue == null);
            Assert.AreEqual(0, property.objectReferenceInstanceIDValue);
        }

        [Test]
        public void ValidReference_HasObjectValueAndNonZeroInstanceId()
        {
            SerializedObject serializedObject = new SerializedObject(_validReferenceFilter);
            serializedObject.Update();

            SerializedProperty property = serializedObject.FindProperty("m_Mesh");

            Assert.IsNotNull(property);
            Assert.AreEqual(SerializedPropertyType.ObjectReference, property.propertyType);
            Assert.IsTrue(property.objectReferenceValue != null);
            Assert.AreNotEqual(0, property.objectReferenceInstanceIDValue);
        }

        [Test]
        public void BrokenReference_HasNullValueAndNonZeroInstanceId()
        {
            SerializedObject serializedObject = new SerializedObject(_brokenReferenceFilter);
            serializedObject.Update();

            SerializedProperty property = serializedObject.FindProperty("m_Mesh");

            Assert.IsNotNull(property);
            Assert.AreEqual(SerializedPropertyType.ObjectReference, property.propertyType);
            Assert.IsTrue(property.objectReferenceValue == null);
            Assert.AreNotEqual(0, property.objectReferenceInstanceIDValue);
        }
    }
}