\# Validation



\## Validation Window



Unity 메뉴에서 다음 경로로 엽니다.



`Tools > ChoDogyu > General Editor Tools > Validation`



Validation Window에서 검사 범위와 Rule을 선택하고 Validation을 실행할 수 있습니다.



\## Validation Rules



\### Missing Script



GameObject에 연결되어 있던 MonoBehaviour Script가 삭제되었거나 찾을 수 없는 상태를 검사합니다.



하나의 GameObject에 여러 Missing Script가 존재하면 해당 GameObject에서 발견된 Missing Script 개수를 하나의 Validation Issue로 기록합니다.



\### Broken Serialized Reference



Serialized Object Reference가 정상적인 `null` 상태가 아니라 삭제된 Unity Object를 가리키는 상태를 검사합니다.



다음 구조를 지원합니다.



\- 일반 Serialized Object Reference

\- Array

\- List

\- 중첩 Serializable 타입



정상적인 `null` Reference는 문제로 처리하지 않습니다.



\## Validation Scopes



\### Selection



현재 Unity Editor에서 선택된 GameObject와 해당 Hierarchy를 검사합니다.



동일 GameObject와 해당 Component가 동시에 선택되어도 동일 GameObject를 중복 검사하지 않습니다.



\### Loaded Scenes



현재 로드된 모든 Scene의 Root GameObject와 전체 Hierarchy를 검사합니다.



저장되지 않은 Scene의 Asset Path는 빈 값으로 기록될 수 있습니다.



\### Project Prefabs



`Assets/` 영역의 Prefab Asset을 검색하고 전체 Hierarchy를 검사합니다.



`Packages/` 영역은 검사하지 않습니다.



\### Project Scenes



`Assets/` 영역의 Scene Asset을 검사합니다.



이미 로드된 Scene은 그대로 사용합니다.



닫혀 있는 Scene은 검사 동안 Additive로 임시 로드하고 검사가 끝난 뒤 다시 닫습니다.



Project Scene 검사는 저장되지 않은 Scene 또는 Dirty Scene이 존재할 경우 시작하지 않습니다.



\### Project



Project Prefabs와 Project Scenes를 하나의 Validation 작업으로 실행합니다.



현재 열린 Scene의 안전성 검사를 먼저 통과한 경우에만 Project 검사를 진행합니다.



\## Validation Result



Validation 결과에는 다음 위치 정보가 포함될 수 있습니다.



\- Asset Path

\- Object Path

\- Component Name

\- Property Path

\- GameObject GlobalObjectId

\- Component GlobalObjectId



Unity Object 자체를 결과에 장기간 보관하지 않고 식별 정보를 저장하기 때문에 검사 후 Scene이 닫혀도 Validation 결과를 유지할 수 있습니다.



\## Search



검색은 다음 정보를 대상으로 합니다.



\- Issue Type

\- Asset Path

\- Object Path

\- Component Name

\- Property Path

\- Message



검색은 영문 대소문자를 구분하지 않습니다.



검색 조건을 변경해도 Validation을 다시 실행하지 않습니다.



\## Filter



현재 지원하는 Validation Issue 종류별로 결과 표시 여부를 변경할 수 있습니다.



\- Missing Script

\- Broken Serialized Reference



Result Filter는 기존 검사 결과의 표시만 변경하며 Validation 자체를 다시 실행하지 않습니다.



\## Go To



각 결과의 `Go To` 버튼은 Asset 종류에 따라 적절한 Navigation을 실행합니다.



\### Loaded Object



현재 접근 가능한 GameObject를 선택하고 Ping합니다.



\### Prefab



Prefab Mode를 열고 문제 GameObject를 선택합니다.



\### Scene



필요한 Scene을 열고 문제 GameObject를 선택합니다.



이미 열려 있는 Scene은 그대로 사용합니다.



Validation 이후 대상 Asset이나 Object가 삭제되어 더 이상 찾을 수 없는 경우 Navigation 실패로 처리합니다.



대상을 찾을 수 없다는 이유만으로 불필요한 Scene이나 Prefab Stage를 열린 상태로 남기지 않습니다.



\## Progress



Project Prefabs, Project Scenes 및 Project 검사에서는 현재 검사 중인 Asset과 진행 상태를 표시합니다.



진행 상태는 Asset 단위로 갱신됩니다.



\## Cancel



Project 검사 도중 사용자가 작업을 취소할 수 있습니다.



취소가 요청되면 이후 Asset 검사를 진행하지 않습니다.



취소 시 부분 검사 결과는 Validation Window에 표시하지 않습니다.



취소된 이후에도 새로운 Validation을 다시 실행할 수 있습니다.

