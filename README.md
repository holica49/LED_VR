# LED Screen VR Demo — Meta Quest 2

**LED 스크린 크기/시청거리 체감 MVP**

VR 공간에서 LED 패널 크기, 무대 높이, 관객 거리, 눈높이를 입력하면
현실 스케일 그대로 체감할 수 있는 데모입니다.

## Quick Start

1. Unity 6 LTS (6000.3.x)에서 이 폴더를 프로젝트로 열기
2. 패키지 자동 import 대기
3. 빈 Scene → 빈 GameObject에 `SceneBootstrap.cs` 부착 → Play
4. Quest 2 빌드: `Docs/SETUP_AND_BUILD.md` 참조

## Project Structure

```
Assets/
  Scripts/
    ScreenConfig.cs      # 패널 상수, 검증 범위
    ScreenController.cs  # LED 배치, 관객 위치, 각도 계산
    UIController.cs      # 입력 파싱, 모드 전환, Apply 연결
    SceneBootstrap.cs    # 런타임 자동 씬 생성 (옵션)
Docs/
    SCENE_HIERARCHY.md   # 씬 계층 구조 + 좌표 규약
    SETUP_AND_BUILD.md   # 프로젝트 설정 및 Quest 2 빌드 가이드
    TEST_SCENARIOS.md    # 테스트 시나리오 3개 + 기대값
```

## Key Design Decisions

- **1 Unity unit = 1 metre** — XR에서 현실 스케일 보장
- **HMD FOV 변경 없음** — OpenXR 네이티브 projection 그대로 사용
- **LED = Box (두께 90mm)** — Quad가 아닌 Cube로 표현, 전면 z=0 기준 배치
- **D = 전면 기준 거리** — 관객 눈 ~ LED 전면까지의 거리
- **Apply 버튼 기반** — Update()에서 매프레임 재계산하지 않음

## Documentation

- [Scene Hierarchy](Docs/SCENE_HIERARCHY.md)
- [Setup & Build Guide](Docs/SETUP_AND_BUILD.md)
- [Test Scenarios](Docs/TEST_SCENARIOS.md)
