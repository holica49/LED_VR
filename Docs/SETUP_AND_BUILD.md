# Project Setup & Build Guide — LED Screen VR Demo (Quest 2)

## Prerequisites

| Tool | Version |
|---|---|
| Unity | **2022.3 LTS** (any 2022.3.x) |
| Android SDK | API 29+ (auto-installed via Unity Hub) |
| JDK | 11 (bundled with Unity) |
| Meta Quest 2 | Firmware ≥ v57, Developer Mode ON |
| Oculus ADB Drivers | Latest (Windows) |
| SideQuest / adb | For sideloading |

## 1. Open Project

```
Unity Hub → Open → select the LED_VR root folder
```

Unity will import packages from `Packages/manifest.json`.

## 2. Install Required Packages (verify)

Window → Package Manager — confirm these are installed:

- **XR Plugin Management** 4.4+
- **OpenXR Plugin** 1.9+
- **XR Interaction Toolkit** 2.5+  *(import Starter Assets sample)*
- **Input System** 1.7+
- **TextMeshPro** 3.0+  *(import TMP Essentials when prompted)*

## 3. XR / OpenXR Settings

### 3-a. Enable XR

Edit → Project Settings → **XR Plug-in Management**

| Tab | Setting |
|---|---|
| **Android** | ☑ OpenXR |
| **PC** (optional) | ☑ OpenXR |

### 3-b. OpenXR Features

Under XR Plug-in Management → OpenXR:

| Setting | Value |
|---|---|
| Interaction Profile | **Oculus Touch Controller Profile** |
| Render Mode | Multi-pass *(or Single-pass Instanced)* |
| Depth Submission Mode | None |
| Features ☑ | Meta Quest Support |

### 3-c. Android Player Settings

Edit → Project Settings → **Player → Android tab**:

| Setting | Value |
|---|---|
| Company Name | (anything) |
| Product Name | LED_VR_Demo |
| Minimum API Level | **Android 10.0 (API 29)** |
| Target API Level | **Automatic (highest)** or 32 |
| Scripting Backend | **IL2CPP** |
| Target Architectures | **ARM64** only |
| Install Location | Automatic |
| Graphics APIs | **Vulkan** (remove OpenGL ES if present) |
| Color Space | **Linear** |

### 3-d. Quality Settings (recommended)

Edit → Project Settings → Quality:

- Use **Medium** or **Low** preset for Android
- Anti-Aliasing: 4x MSAA (or 2x for performance)
- VSync Count: Don't Sync (XR handles vsync)

## 4. Scene Setup

### Option A — Automatic (SceneBootstrap)

1. Create a new scene (File → New Scene → Basic)
2. Delete default Main Camera and Directional Light
3. Create empty GameObject → name it `Bootstrap`
4. Attach `SceneBootstrap.cs`
5. Press Play — scene auto-builds

### Option B — Manual

Follow `Docs/SCENE_HIERARCHY.md` to build the hierarchy by hand.

### For Quest 2 Build (both options)

1. Delete the auto-created camera rig
2. Add **XR Origin (XR Rig)** prefab from
   `Packages/XR Interaction Toolkit/Runtime/Prefabs/XR Origin (XR Rig).prefab`
3. Add **XR Interaction Manager** if not present
4. Add **Event System** with **XR UI Input Module**
5. On the XR Origin's **Left/Right Controller**: add
   **XR Ray Interactor** + **XR Interactor Line Visual** for UI pointing
6. Assign the XR Origin transform to `ScreenController.xrOrigin` in Inspector
7. On `UICanvas`: set Canvas → **Event Camera** to the XR Origin's Main Camera
8. Add a **Tracked Device Graphic Raycaster** component to the Canvas
   (replace the default `GraphicRaycaster`)

## 5. Build APK

1. File → Build Settings
2. Switch Platform → **Android**
3. Add the scene to Scenes In Build
4. Texture Compression: **ASTC** (Quest default)
5. Click **Build** → save as `LED_VR_Demo.apk`

## 6. Install on Quest 2 (Sideload)

### Via adb

```bash
# Connect Quest 2 via USB-C, allow USB debugging on headset
adb devices                          # verify device shows up
adb install LED_VR_Demo.apk         # first install
adb install -r LED_VR_Demo.apk      # update existing
```

### Via SideQuest

1. Open SideQuest desktop app
2. Connect Quest 2
3. Drag-and-drop the `.apk` file into SideQuest

### Launch on Quest 2

- App Library → filter "Unknown Sources"
- Find **LED_VR_Demo** → Launch

## 7. Troubleshooting

| Problem | Fix |
|---|---|
| Black screen on Quest | Verify OpenXR + Meta Quest Support feature enabled |
| UI not responding to controller | Add XR Ray Interactor to controllers; use Tracked Device Graphic Raycaster on Canvas |
| App crashes on start | Check `adb logcat -s Unity` for errors; likely missing TMP resources |
| Low FPS | Reduce quality preset; ensure ARM64 + IL2CPP; use Vulkan |
| Input fields not editable | Ensure XR UI Input Module is on EventSystem; use physical keyboard overlay or pre-fill defaults |
