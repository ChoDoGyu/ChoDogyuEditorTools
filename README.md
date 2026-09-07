# ChoDogyu General Editor Tools

Unity 프로젝트에서 반복적인 검증 작업을 자동화하고 발견된 문제 위치를 빠르게 탐색할 수 있도록 만든 범용 Editor 전용 UPM 패키지입니다.

특정 게임 장르나 프로젝트에 종속되지 않으며 다른 ChoDogyu 패키지 없이 독립적으로 설치하여 사용할 수 있습니다.

## Package

- Package: `com.chodogyu.editor-tools`
- Assembly: `CDG.EditorTools`
- Version: `1.0.0`
- Unity: `6000.0` 이상
- Editor Only

## 주요 기능

- Missing Script 검사
- Broken Serialized Reference 검사
- Selection 검사
- Loaded Scenes 검사
- Project Prefabs 검사
- Project Scenes 검사
- Project 전체 검사
- 결과 Search / Filter
- Loaded Object Navigation
- Prefab Mode Navigation
- Scene Navigation
- Project 검사 Progress / Cancel
- Dirty / Unsaved Scene 보호

## 설치

### v1.0.0 설치

Unity Package Manager에서 다음 Git URL을 사용합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuEditorTools.git?path=/com.chodogyu.editor-tools#v1.0.0
```

### 최신 main 설치

최신 개발 상태를 설치하려면 다음 URL을 사용합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuEditorTools.git?path=/com.chodogyu.editor-tools
```

일반적인 사용에는 버전이 고정된 `v1.0.0` 설치를 권장합니다.

## Repository Structure

```text
ChoDogyuEditorTools/
├─ EditorToolsDevelopment/
│  └─ 패키지 개발 및 검증용 Unity 프로젝트
│
└─ com.chodogyu.editor-tools/
   └─ 실제 배포 가능한 UPM 패키지
```

## Validation

v1.0 제품화 과정에서 다음 항목을 검증했습니다.

- 170 EditMode Tests Passed
- 1,001 GameObject Hierarchy 검사
- 대량 Prefab 검사
- 대량 Scene 검사
- 검사 취소 및 재실행
- 새 Unity 프로젝트 독립 설치
- 실제 Missing Script 검출
- 실제 Broken Serialized Reference 검출
- Prefab / Scene Navigation
- 패키지 제거 및 재설치
- Git URL 설치 및 Unity 재시작

## Documentation

상세 문서는 패키지 내부에서 확인할 수 있습니다.

- `com.chodogyu.editor-tools/README.md`
- `com.chodogyu.editor-tools/Documentation~/index.md`
- `com.chodogyu.editor-tools/Documentation~/validation.md`
- `com.chodogyu.editor-tools/Documentation~/safety.md`

## Status

`v1.0.0` 완료