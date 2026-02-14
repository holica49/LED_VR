# Test Scenarios

All angles computed with the formulas:

```
θH = 2 · atan( (Wm/2) / D )      → degrees
θV = 2 · atan( (Hm/2) / D )      → degrees
pitch = atan( (B + Hm/2 − E) / D ) → degrees
```

Where **D** = distance from audience eye to LED front face (metres).

---

## Scenario 1 — Mode B: 8 × 4 panels, close audience

| Parameter | Value |
|---|---|
| Mode | B (panels) |
| cols | 8 |
| rows | 4 |
| stageHeight | 0.6 m |
| B | 0.6 m (= stageHeight) |
| D | 10 m |
| E | 1.6 m |

### Derived

```
Wm = 8 × 0.5 = 4.0 m
Hm = 4 × 0.5 = 2.0 m

θH = 2 · atan(2.0 / 10) = 2 · atan(0.2) = 2 × 11.310° = 22.6°
θV = 2 · atan(1.0 / 10) = 2 · atan(0.1) = 2 × 5.711°  = 11.4°

ledCenterY = 0.6 + 1.0 = 1.6
dy = 1.6 − 1.6 = 0.0
pitch = atan(0.0 / 10) = 0.0°
```

### Expected Result Text

```
LED  4.00 x 2.00 m
Horiz FOV   22.6°
Vert  FOV   11.4°
Pitch       0.0°
```

### LED Box

- Scale: (4, 2, 0.09)
- Position: (0, 1.6, 0.045)
- XR Origin: (0, 0, −10)

---

## Scenario 2 — Mode B: 12 × 6 panels, mid-distance

| Parameter | Value |
|---|---|
| Mode | B (panels) |
| cols | 12 |
| rows | 6 |
| stageHeight | 1.0 m |
| B | 1.0 m |
| D | 15 m |
| E | 1.6 m |

### Derived

```
Wm = 12 × 0.5 = 6.0 m
Hm = 6 × 0.5 = 3.0 m

θH = 2 · atan(3.0 / 15) = 2 · atan(0.2) = 2 × 11.310° = 22.6°
θV = 2 · atan(1.5 / 15) = 2 · atan(0.1) = 2 × 5.711°  = 11.4°

ledCenterY = 1.0 + 1.5 = 2.5
dy = 2.5 − 1.6 = 0.9
pitch = atan(0.9 / 15) = atan(0.06) = 3.4°
```

### Expected Result Text

```
LED  6.00 x 3.00 m
Horiz FOV   22.6°
Vert  FOV   11.4°
Pitch       3.4°
```

### LED Box

- Scale: (6, 3, 0.09)
- Position: (0, 2.5, 0.045)
- XR Origin: (0, 0, −15)

---

## Scenario 3 — Mode A: direct metre input

| Parameter | Value |
|---|---|
| Mode | A (metres) |
| Wm | 6.0 m |
| Hm | 3.0 m |
| stageHeight | 1.2 m |
| B | 1.2 m |
| D | 12 m |
| E | 1.65 m |

### Derived

```
θH = 2 · atan(3.0 / 12) = 2 · atan(0.25)  = 2 × 14.036° = 28.1°
θV = 2 · atan(1.5 / 12) = 2 · atan(0.125) = 2 × 7.125°  = 14.3°

ledCenterY = 1.2 + 1.5 = 2.7
dy = 2.7 − 1.65 = 1.05
pitch = atan(1.05 / 12) = atan(0.0875) = 5.0°
```

### Expected Result Text

```
LED  6.00 x 3.00 m
Horiz FOV   28.1°
Vert  FOV   14.3°
Pitch       5.0°
```

### LED Box

- Scale: (6, 3, 0.09)
- Position: (0, 2.7, 0.045)
- XR Origin: (0, 0, −12)

---

## How to Run Each Scenario

1. Launch the app on Quest 2 (or Play in Editor).
2. Select Mode A or B via the toggle.
3. Enter the values in the input fields.
4. Press **APPLY**.
5. Verify:
   - Result text matches expected values above.
   - LED box appears at correct position/scale.
   - Looking at the LED from the audience position feels proportionally correct.
   - The 1 m ruler next to the LED helps gauge real-world scale.
