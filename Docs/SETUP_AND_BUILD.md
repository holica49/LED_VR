# LED Screen VR Demo — 처음부터 끝까지 (Unity 6 + Quest 2)

> Unity 6.3 LTS (6000.3.x) 기준, VR 개발 처음인 사람을 위한 가이드

---

## STEP 0. PC에 필요한 것 (1회만)

### 0-1. Unity Hub + Unity 6 설치

이미 설치되어 있다면 → **Android 모듈만 확인**

```
Unity Hub → Installs → 설치된 Unity 6.3 LTS 옆 톱니바퀴(⚙)
  → Add Modules
  → ☑ Android Build Support
  → ☑ Android SDK & NDK Tools
  → ☑ OpenJDK
  → Install
```

> Android 항목이 이미 체크되어 있으면 STEP 1로 건너뛰기

### 0-2. Quest 2 개발자 모드 켜기

```
1. 스마트폰에 "Meta Quest" 앱 설치 → 로그인
2. Quest 2와 블루투스 페어링
3. 메뉴 → 디바이스 → 개발자 모드(Developer Mode) → ON
4. Quest 2 재부팅
```

> "개발자 모드"가 안 보이면 https://developer.oculus.com 에서 "조직 생성" 먼저

### 0-3. ADB 드라이버 (Windows만)

```
"Oculus ADB Drivers" 검색 → 다운로드 → 압축 풀기
→ android_winusb.inf 우클릭 → 설치
```

macOS / Linux는 필요 없음.

---

## STEP 1. 프로젝트 만들기

### 방법 A: 지금 열려있는 프로젝트에 스크립트만 복사 (가장 간단)

이미 "My project" (URP Empty)가 열려있다면:

```
1. GitHub에서 코드 다운로드:
   - https://github.com/holica49/LED_VR 접속
   - 초록색 "Code" 버튼 → "Download ZIP"
   - 압축 풀기

2. 압축 푼 폴더에서 Scripts 폴더만 복사:
   LED_VR/Assets/Scripts/ 폴더 통째로
   → My project/Assets/ 안에 붙여넣기

⚠️ 주의: Assets/Scripts/ 폴더만 복사하세요!
   Packages/ 폴더나 ProjectSettings/ 폴더를 덮어쓰면
   기존 프로젝트의 URP 설정이 깨져서 핑크색 화면이 됩니다.

3. Unity로 돌아오면 자동으로 스크립트를 인식함
```

최종 경로 확인:
```
My project/
  Assets/
    Scripts/
      ScreenConfig.cs
      ScreenController.cs
      UIController.cs
      SceneBootstrap.cs
```

### 방법 B: 새 프로젝트로 시작

```
Unity Hub → New Project
  → 템플릿: "Universal 3D" (URP)
  → 프로젝트 이름: LED_VR_Demo
  → Create
```

위와 같이 Scripts 폴더 복사.

---

## STEP 2. 필수 패키지 설치

### 2-1. 패키지 매니저 열기

```
Window → Package Manager
```

### 2-2. XR 패키지 설치

왼쪽 상단 드롭다운을 **"Packages: Unity Registry"** 로 변경 후 검색:

| 순서 | 검색어 | 패키지명 | 버튼 |
|---|---|---|---|
| 1 | `XR Plugin` | **XR Plugin Management** | Install |
| 2 | `OpenXR` | **OpenXR Plugin** | Install |
| 3 | `Meta` | **Unity OpenXR: Meta** | Install |
| 4 | `XR Interaction` | **XR Interaction Toolkit** | Install |

> 설치 순서대로 하면 의존성이 자동 해결됨
> "Input System" 백엔드 변경 팝업 뜨면 → **"Yes"** 클릭 (Unity 재시작됨)

### 2-3. XR Interaction Toolkit 샘플 import

```
Package Manager → XR Interaction Toolkit 선택
  → 오른쪽 "Samples" 탭
  → "Starter Assets" → Import
```

### 2-4. TextMeshPro 리소스 import

Unity 6에서는 TMP가 내장되어 있지만 폰트 리소스는 별도 import 필요:

```
Window → TextMeshPro → Import TMP Essential Resources
→ 팝업에서 Import 클릭
```

> 메뉴가 안 보이면: 상단에 Edit → Project Settings 검색 후 TextMeshPro 확인

---

## STEP 3. Android + Quest 2 설정

### 3-1. Android 플랫폼 전환

```
File → Build Profiles    (단축키: Ctrl+Shift+B)
  → 왼쪽 하단 "Add Build Profile" → "Android" 선택
  → "Switch Platform" 클릭
  → 1~2분 대기 (에셋 재변환)
```

### 3-2. XR 활성화

```
Edit → Project Settings → XR Plug-in Management
  → Android 탭 (로봇 아이콘 🤖)
  → ☑ OpenXR 체크
```

### 3-3. OpenXR에서 Quest 활성화

```
왼쪽 메뉴에서 XR Plug-in Management → OpenXR 클릭

  Enabled Interaction Profiles:
    → "+" 버튼 → "Meta Quest Touch Pro Controller Profile" 추가
    (또는 "Oculus Touch Controller Profile")

  OpenXR Feature Groups:
    → ☑ "Meta Quest" 체크 (이것이 핵심!)
```

> 노란 경고(⚠) 뜨면 → **"Fix All"** 클릭

### 3-4. Player Settings (Android)

```
Edit → Project Settings → Player → Android 탭

Other Settings:
  Color Space               → Linear
  Auto Graphics API         → 체크 해제
  Graphics APIs             → Vulkan만 남기기 (OpenGL ES 있으면 "-" 로 제거)
  Minimum API Level         → Android 10.0 (API 29)
  Scripting Backend         → IL2CPP
  Target Architectures      → ☑ ARM64 만 체크
```

---

## STEP 4. 씬 만들기

### 4-1. 새 씬 생성

```
File → New Scene → Basic (Built-in) → Create
```

### 4-2. 기본 오브젝트 정리

Hierarchy에서 삭제:
```
- Main Camera (삭제)
- Directional Light (삭제해도 됨, SceneBootstrap이 만들어줌)
```

### 4-3. SceneBootstrap 추가

```
1. Hierarchy 우클릭 → Create Empty
2. 이름을 "Bootstrap"으로 변경
3. Inspector에서 Add Component → "SceneBootstrap" 검색 → 추가
```

### 4-4. XR Origin 추가 (Quest 컨트롤러로 UI 조작하려면 필수)

```
1. Hierarchy 우클릭 → XR → XR Origin (XR Rig)
2. Hierarchy 우클릭 → XR → Interaction Manager (없으면 추가)
3. Hierarchy에 EventSystem 확인 → 없으면:
   우클릭 → UI → Event System
```

### 4-5. 씬 저장

```
File → Save As → Assets/Scenes/LEDScreenDemo.unity
```

### 4-6. PC에서 테스트

```
▶ Play 버튼 클릭
→ 어두운 바닥 + 파란 LED 박스 + 왼쪽에 UI 패널이 보이면 성공!
```

---

## STEP 5. Quest 2 빌드

### 5-1. Quest 2를 USB-C로 PC에 연결

```
1. USB-C 케이블로 연결
2. Quest 2 쓰면 "USB 디버깅 허용?" 팝업 → 허용
3. PC 터미널(cmd/PowerShell)에서 확인:
   adb devices
   → 숫자가 보이면 연결 성공
```

### 5-2. 빌드

```
File → Build Profiles    (Ctrl+Shift+B)
  → 현재 Android 프로필 선택
  → "Add Open Scenes" → LEDScreenDemo 추가
  → Texture Compression: ASTC
  → "Build" 클릭
  → 저장 위치/이름 지정 (예: LED_VR_Demo.apk)
  → 첫 빌드 5~15분 대기
```

### 5-3. Quest 2에 설치

```bash
adb install LED_VR_Demo.apk
```

### 5-4. 실행

```
Quest 2 헤드셋에서:
  앱 라이브러리 → 오른쪽 상단 필터 → "알 수 없는 소스"
  → "LED_VR_Demo" 실행!
```

---

## 문제 해결

| 증상 | 해결 |
|---|---|
| Package Manager에 Android 없음 | Unity Hub → Installs → Add Modules → Android Build Support |
| OpenXR 설정에 Meta Quest 없음 | "Unity OpenXR: Meta" 패키지가 설치 안 됨 → STEP 2-2 |
| Play 누르면 핑크색 오브젝트 | URP 패키지 누락일 가능성 높음 → Window → Package Manager에서 "Universal RP" 설치 확인. ZIP에서 Packages/manifest.json을 덮어썼다면 URP가 제거됨 → 프로젝트를 새로 만들거나 Package Manager에서 URP 재설치 |
| TMP 글자가 안 보임 | Window → TextMeshPro → Import TMP Essential Resources |
| Quest에서 검은 화면 | OpenXR + Meta Quest Feature 체크 확인 → STEP 3-3 |
| 컨트롤러로 UI 못 누름 | XR Origin의 컨트롤러에 XR Ray Interactor 확인 |
| adb devices 비어있음 | 개발자 모드 ON 확인, USB 케이블 교체 시도 |
| 빌드 에러 | Scripting Backend = IL2CPP, ARM64 확인 → STEP 3-4 |
