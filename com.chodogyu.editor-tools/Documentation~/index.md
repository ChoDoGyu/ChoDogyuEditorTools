\# ChoDogyu General Editor Tools Documentation



ChoDogyu General Editor Tools는 Unity 프로젝트에서 반복적인 검증 작업을 자동화하고 발견된 문제 위치를 빠르게 탐색하기 위한 Editor 전용 UPM 패키지입니다.



특정 게임 장르나 프로젝트 구조에 종속되지 않으며 다른 ChoDogyu 패키지에 의존하지 않습니다.



\## v1.0 기능



\### Validation Rules



\- Missing Script

\- Broken Serialized Reference



\### Validation Scopes



\- Selection

\- Loaded Scenes

\- Project Prefabs

\- Project Scenes

\- Project



\### Result Navigation



\- 현재 로드된 Object 선택 및 Ping

\- Prefab Mode Navigation

\- Scene Navigation



\### Editor UX



\- Validation Window

\- Search

\- Result Type Filter

\- Go To

\- Progress

\- Cancel



\## 설계 원칙



General Editor Tools는 다음 원칙을 따릅니다.



\- 특정 게임 장르에 종속되지 않습니다.

\- 특정 게임 프로젝트에 종속되지 않습니다.

\- 다른 ChoDogyu 패키지에 의존하지 않습니다.

\- Runtime 게임 기능을 제공하지 않습니다.

\- Editor 전용 코드만 포함합니다.

\- 특정 패키지 내부 구조를 알아야 하는 기능은 포함하지 않습니다.

\- 자동 수정보다 문제 탐지와 위치 탐색을 우선합니다.

\- 사용자의 Scene과 Asset을 임의로 수정하지 않습니다.

\- 불필요한 전체 검색과 반복적인 Object 탐색을 피합니다.

\- 성능 최적화는 실제 측정 결과를 기준으로 판단합니다.



\## 문서



\- `validation.md` — Validation Rule, Scope 및 결과 탐색

\- `safety.md` — Scene 안전 정책과 취소 및 예외 처리

