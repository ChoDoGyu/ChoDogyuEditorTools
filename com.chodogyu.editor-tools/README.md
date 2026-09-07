# ChoDogyu General Editor Tools

Unity 프로젝트에서 반복적으로 수행하는 검증 작업을 자동화하고, 발견된 문제 위치를 빠르게 탐색할 수 있도록 만든 범용 Editor 전용 UPM 패키지입니다.

특정 게임 장르나 특정 ChoDogyu 패키지에 종속되지 않으며 일반적인 Unity 프로젝트에 독립적으로 설치하여 사용할 수 있습니다.

## 주요 기능

### Validation

다음 문제를 검사할 수 있습니다.

- Missing Script
- Broken Serialized Reference

다음 검사 범위를 지원합니다.

- Selection
- Loaded Scenes
- Project Prefabs
- Project Scenes
- Project

### Result Navigation

검사 결과의 `Go To` 버튼을 사용해 문제 위치로 이동할 수 있습니다.

- 현재 로드된 GameObject 선택 및 Ping
- Prefab Mode 열기 및 문제 GameObject 선택
- Scene 열기 및 문제 GameObject 선택

### Search / Filter

검사 결과를 다음 정보를 기준으로 검색할 수 있습니다.

- Issue Type
- Asset Path
- Object Path
- Component
- Property
- Message

Missing Script와 Broken Serialized Reference 결과를 종류별로 필터링할 수 있습니다.

### Progress / Cancel

Project Prefabs, Project Scenes, Project 검사에서는 현재 검사 중인 Asset과 진행 상태를 표시합니다.

검사 도중 취소할 수 있으며 취소된 검사의 부분 결과는 표시하지 않습니다.

## 요구 사항

- Unity 6 이상
- Unity Editor 전용
- 다른 ChoDogyu 패키지 의존성 없음

개발 및 검증에 사용한 Unity 버전:

`6000.3.9f1`

## 설치

### Git URL 설치

이 패키지는 저장소 루트가 아니라 `com.chodogyu.editor-tools` 하위 폴더에 위치합니다.

Unity Package Manager에서 Git 저장소를 통해 설치할 때는 저장소 Git URL 뒤에 다음 경로를 추가합니다.

```text
?path=/com.chodogyu.editor-tools
```

Unity에서 다음 순서로 설치합니다.

1. `Window > Package Manager`를 엽니다.
2. 좌측 상단 `+` 버튼을 누릅니다.
3. `Add package from git URL...`을 선택합니다.
4. 저장소 Git URL과 `?path=/com.chodogyu.editor-tools` 경로를 함께 입력합니다.
5. `Add`를 눌러 설치합니다.

### Local Package 설치

로컬에 저장된 패키지를 직접 설치할 수 있습니다.

1. `Window > Package Manager`를 엽니다.
2. 좌측 상단 `+` 버튼을 누릅니다.
3. `Add package from disk...`를 선택합니다.
4. `com.chodogyu.editor-tools/package.json`을 선택합니다.
5. 패키지 설치가 완료될 때까지 기다립니다.

## 사용 방법

Unity 상단 메뉴에서 다음 경로로 Validation Window를 엽니다.

`Tools > ChoDogyu > General Editor Tools > Validation`

1. 검사 범위를 선택합니다.
2. 실행할 Validation Rule을 선택합니다.
3. `Validate`를 실행합니다.
4. 결과를 Search / Filter로 확인합니다.
5. 필요한 결과의 `Go To`를 눌러 문제 위치로 이동합니다.

## Validation Rules

### Missing Script

GameObject에 연결되어 있던 MonoBehaviour Script가 삭제되었거나 찾을 수 없는 상태를 검사합니다.

하나의 GameObject에서 여러 Missing Script가 발견되면 해당 GameObject의 Missing Script 개수를 하나의 Validation Issue로 기록합니다.

### Broken Serialized Reference

Serialized Object Reference가 정상적인 `null` 상태가 아니라 삭제된 Unity Object를 가리키는 상태를 검사합니다.

다음 구조를 지원합니다.

- 일반 Serialized Object Reference
- Array
- List
- 중첩 Serializable 타입

정상적인 `null` Reference는 문제로 처리하지 않습니다.

## 검사 범위

### Selection

현재 선택된 GameObject와 해당 Hierarchy를 검사합니다.

### Loaded Scenes

현재 로드된 모든 Scene의 전체 Hierarchy를 검사합니다.

### Project Prefabs

`Assets/` 영역의 모든 Prefab을 검사합니다.

`Packages/` 영역은 검사하지 않습니다.

### Project Scenes

`Assets/` 영역의 모든 Scene Asset을 검사합니다.

닫혀 있는 Scene은 검사 과정에서 Additive로 임시 로드한 뒤 다시 닫습니다.

### Project

Project Prefabs와 Project Scenes를 한 번에 검사합니다.

## Scene 안전 정책

Project Scenes 또는 Project 검사를 실행할 때 저장되지 않은 Scene이나 Dirty Scene이 존재하면 검사를 시작하지 않습니다.

패키지는 사용자의 Scene을 자동으로 저장하지 않습니다.

이미 열려 있던 Scene은 임의로 닫지 않으며, Validation이 직접 연 임시 Scene만 검사 완료 후 정리합니다.

검사 중 예외 또는 취소가 발생해도 Validation이 직접 연 Scene은 정리됩니다.

자세한 내용은 `Documentation~/safety.md`를 참고하세요.

## 성능 검증

v1.0 개발 과정에서 다음 규모를 기준으로 검증했습니다.

- 1,001 GameObject Hierarchy Traversal
- 1,001 GameObject에 실제 Validation Rule 적용
- 50개 이상의 Project Prefab 검사
- 20개 이상의 Project Scene 검사
- 대량 Asset 검사 중 취소 및 재실행

개발 환경에서 측정한 대표 결과:

- Hierarchy Traversal — 1,001 GameObjects: 1 ms 미만
- Hierarchy + Actual Rules — 1,001 GameObjects: 약 27 ms
- Project Prefabs — 50개 테스트 Prefab 및 기존 Asset: 약 4 ms
- Project Scenes — 20개 테스트 Scene 및 기존 Asset: 약 47 ms

측정 결과를 기준으로 v1.0에서는 추가 캐싱이나 병렬 처리를 도입하지 않았습니다.

## 패키지 구조

```text
com.chodogyu.editor-tools/
├─ Editor/
├─ Tests/
├─ Documentation~/
├─ package.json
├─ README.md
└─ CHANGELOG.md
```

이 패키지는 Runtime 게임 기능을 제공하지 않으므로 `Runtime/` 폴더를 포함하지 않습니다.

## 테스트

Unity Test Framework의 EditMode 테스트를 통해 다음 영역을 검증합니다.

- Validation Core
- Hierarchy Traversal
- Missing Script
- Broken Serialized Reference
- Validation Scopes
- Scene 안전성
- Result Navigation
- Search / Filter
- Progress / Cancel
- 반복 실행 안정성
- 대규모 Hierarchy
- 대량 Prefab / Scene
- 성능 측정

v1.0 제품화 단계 기준:

`170 EditMode Tests Passed`

## 문서

- `Documentation~/index.md`
- `Documentation~/validation.md`
- `Documentation~/safety.md`