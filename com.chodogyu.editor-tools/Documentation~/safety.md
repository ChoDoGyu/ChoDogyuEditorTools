\# Safety



General Editor Tools는 Validation 실행 과정에서 사용자의 작업 상태를 임의로 변경하지 않는 것을 중요한 원칙으로 사용합니다.



\## Dirty Scene



Project Scenes 또는 Project 검사를 실행할 때 로드된 Scene에 저장되지 않은 변경 사항이 있으면 Validation을 시작하지 않습니다.



사용자가 직접 Scene을 저장한 뒤 다시 실행해야 합니다.



패키지는 사용자의 Scene을 자동으로 저장하지 않습니다.



\## Unsaved Scene



아직 Asset Path가 없는 저장되지 않은 Scene이 열려 있는 경우 Project Scenes 및 Project 검사를 시작하지 않습니다.



이는 검사 과정에서 Scene Open 및 Close가 사용자의 현재 작업 상태에 영향을 줄 가능성을 방지하기 위한 정책입니다.



\## Existing Loaded Scene



이미 로드되어 있던 Scene은 Validation을 위해 다시 열거나 임의로 닫지 않습니다.



\## Temporary Scene



Project Scene Validation에서 닫혀 있던 Scene은 Additive 방식으로 임시 로드합니다.



검사가 끝나면 Validation이 직접 연 Scene만 닫습니다.



\## Active Scene



Project Scene 검사 과정에서 Active Scene이 변경될 수 있으므로 검사 전 Active Scene을 기록합니다.



검사 완료, 예외 또는 취소 후 기존 Active Scene이 아직 유효하고 로드된 상태라면 다시 Active Scene으로 복원합니다.



\## Exception



Rule 실행 또는 Scene 검사 중 예외가 발생해도 Validation이 임시로 연 Scene은 정리됩니다.



예외로 인해 부분적으로 생성된 Validation 결과는 Validation Window에서 사용자에게 정상 완료 결과처럼 표시하지 않습니다.



\## Cancel



Project Prefabs, Project Scenes 및 Project 검사는 Asset 단위로 취소할 수 있습니다.



취소 시:



\- 이후 Asset 검사를 진행하지 않습니다.

\- Validation이 직접 연 임시 Scene을 정리합니다.

\- 부분 Validation 결과를 사용자에게 표시하지 않습니다.

\- 다음 Validation을 다시 정상적으로 실행할 수 있습니다.



\## Navigation



Validation 결과가 생성된 이후 대상 Asset이나 Object가 삭제될 수 있습니다.



Navigation은 이러한 상황을 정상적인 실패 상황으로 처리합니다.



대상을 찾을 수 없다는 이유만으로 Scene이나 Prefab Stage를 불필요하게 열린 상태로 남기지 않습니다.



\## Automatic Save



General Editor Tools는 Validation을 위해 사용자의 Scene을 자동 저장하지 않습니다.



저장이 필요한 상태라면 Validation을 중단하고 사용자가 직접 저장하도록 안내합니다.



\## Automatic Fix



v1.0은 자동 수정 기능을 제공하지 않습니다.



General Editor Tools의 우선 목적은 문제를 안전하게 탐지하고 사용자가 문제 위치를 빠르게 찾도록 돕는 것입니다.



데이터 변경 가능성이 있는 자동 수정 기능은 별도의 안전성 설계 없이 추가하지 않습니다.

