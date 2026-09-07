\# Changelog



이 문서에서는 ChoDogyu General Editor Tools의 주요 변경 사항을 기록합니다.



\## \[1.0.0] - 2026-09-07



\### Added



\- Missing Script Validation 추가

\- Broken Serialized Reference Validation 추가

\- Array, List 및 중첩 Serializable Reference 검사 지원

\- Selection 검사 범위 추가

\- Loaded Scenes 검사 범위 추가

\- Project Prefabs 검사 범위 추가

\- Project Scenes 검사 범위 추가

\- Project 통합 검사 범위 추가

\- Prefab 및 Scene Hierarchy 전체 순회 지원

\- Validation 결과 위치 정보 저장

\- GlobalObjectId 기반 Object 재탐색 지원

\- 현재 Object 선택 및 Ping 지원

\- Prefab Mode Navigation 지원

\- Scene Navigation 지원

\- Validation Editor Window 추가

\- 결과 Search 기능 추가

\- 결과 Type Filter 기능 추가

\- 검사 결과 `Go To` 기능 추가

\- Project 검사 Progress 표시 추가

\- Project 검사 Cancel 기능 추가



\### Safety



\- Dirty Scene이 존재할 경우 Project Scene 검사 차단

\- 저장되지 않은 Scene이 존재할 경우 Project Scene 검사 차단

\- 사용자 Scene 자동 저장 방지

\- 임시 Scene 검사 후 자동 정리

\- 검사 예외 발생 시 임시 Scene 정리

\- 검사 취소 후 임시 Scene 정리

\- 검사 취소 이후 재실행 안정성 검증

\- 삭제된 Validation 대상 Navigation 안전 처리



\### Validation



\- 대규모 Hierarchy 순회 검증

\- 깊은 Hierarchy 순회 검증

\- 대량 Prefab 검사 검증

\- 대량 Scene 검사 검증

\- 반복 Validation 안정성 검증

\- 170개 EditMode 테스트 통과



\### Performance



v1.0 개발 환경에서 측정한 대표 결과:



\- Hierarchy Traversal — 1,001 GameObjects: 1 ms 미만

\- Hierarchy + Actual Rules — 1,001 GameObjects: 약 27 ms

\- Project Prefabs — 50개 테스트 Prefab 및 기존 Asset: 약 4 ms

\- Project Scenes — 20개 테스트 Scene 및 기존 Asset: 약 47 ms



측정 결과를 기준으로 v1.0에서는 추가 캐싱이나 병렬 처리를 도입하지 않았습니다.

