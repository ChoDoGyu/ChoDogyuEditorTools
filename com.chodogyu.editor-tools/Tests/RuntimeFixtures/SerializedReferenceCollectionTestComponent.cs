using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDG.EditorTools.Tests.Fixtures
{
    /// <summary>
    /// Array, List 및 중첩 Serializable 타입의 Object Reference 검증에 사용하는 테스트 전용 Component입니다.
    /// </summary>
    public sealed class SerializedReferenceCollectionTestComponent : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.Object[] _arrayReferences;

        [SerializeField]
        private List<UnityEngine.Object> _listReferences;

        [SerializeField]
        private NestedReferenceData _nestedData = new NestedReferenceData();

        [Serializable]
        private sealed class NestedReferenceData
        {
            [SerializeField]
            private UnityEngine.Object _reference;
        }
    }
}