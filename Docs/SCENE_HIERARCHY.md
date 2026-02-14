# Scene Hierarchy

The scene can be built **manually** in Unity Editor or **automatically** via `SceneBootstrap.cs`.

## Hierarchy (manual setup)

```
LEDScreenDemo (Scene)
│
├── Directional Light
│
├── XR Origin (XR Rig)                    ← XR Interaction Toolkit prefab
│   └── Camera Offset
│       ├── Main Camera                   ← tracked HMD
│       ├── Left Controller (optional)
│       └── Right Controller (optional)
│
├── Floor                                 ← Plane, scale (10,1,10)
│
├── LEDBox                                ← Cube primitive
│   └── LEDFrontFace                      ← Quad (child), shows emissive "screen" colour
│
├── StagePlatform                         ← Cube, visual-only stage
│
├── Ruler_1m                              ← Cylinder, scale-reference
│
├── UICanvas                              ← Canvas (World Space)
│   ├── ModeToggle
│   ├── ModeAGroup
│   │   ├── Label_Wm  + InputField_Wm
│   │   └── Label_Hm  + InputField_Hm
│   ├── ModeBGroup
│   │   ├── Label_Cols + InputField_Cols
│   │   └── Label_Rows + InputField_Rows
│   ├── Label_StageHeight + InputField_StageHeight
│   ├── Btn_0.6 / Btn_1.0 / Btn_1.2      ← stage-height presets
│   ├── Label_B + InputField_B
│   ├── Label_D + InputField_D
│   ├── Label_E + InputField_E
│   ├── Btn_APPLY
│   ├── WarningText
│   └── ResultText
│
├── ScreenController (empty GO)           ← ScreenController.cs
│
├── EventSystem                           ← required for UI
│   └── XR UI Input Module               ← for VR pointer interaction
│
└── Bootstrap (empty GO)                  ← SceneBootstrap.cs (if auto-building)
```

## Coordinate Convention

| Item | Position |
|---|---|
| LED front face | z = 0 |
| LED centre | z = +0.045 (PanelT/2) |
| LED bottom edge | y = B |
| LED centre Y | y = B + Hm/2 |
| Audience (XR Origin) | (0, 0, −D) |
| Eye (conceptual) | (0, E, −D) |
| Floor | y = 0 |

**Distance D** is always measured from the audience eye to the LED **front face** (z = 0).

## Auto-Build (SceneBootstrap)

1. Create a blank scene.
2. Add an empty GameObject, attach `SceneBootstrap.cs`.
3. Press Play — everything is created at runtime.
4. For Quest 2 builds, replace the auto-created camera with the **XR Origin (XR Rig)** prefab
   from XR Interaction Toolkit and assign it to `ScreenController.xrOrigin`.

## Manual Build

Follow the hierarchy above. Drag references into Inspector:

- `ScreenController`: assign LEDBox, XR Origin, Floor, Ruler_1m
- `UIController`: assign ScreenController + all UI elements
