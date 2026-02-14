using UnityEngine;

/// <summary>
/// LED panel constants, input defaults, and validation ranges.
/// All lengths in meters. Immutable at runtime.
/// </summary>
public static class ScreenConfig
{
    // ── Panel physical spec (fixed) ──────────────────────────────
    public const float PanelW = 0.5f;   // 500 mm
    public const float PanelH = 0.5f;   // 500 mm
    public const float PanelT = 0.09f;  // 90 mm  (thickness)

    // ── Input defaults ───────────────────────────────────────────
    public const int   DefaultCols        = 8;
    public const int   DefaultRows        = 4;
    public const float DefaultStageHeight = 0.6f;  // metres
    public const float DefaultD           = 10f;   // audience distance
    public const float DefaultE           = 1.6f;  // eye height

    // ── Validation ranges (inclusive) ────────────────────────────
    public const int   MinCols = 1,   MaxCols = 60;
    public const int   MinRows = 1,   MaxRows = 60;
    public const float MinWm   = 0.5f, MaxWm  = 30f;
    public const float MinHm   = 0.5f, MaxHm  = 30f;
    public const float MinB    = 0f,   MaxB   = 5f;
    public const float MinD    = 0.5f, MaxD   = 50f;
    public const float MinE    = 0.8f, MaxE   = 2.2f;
    public const float MinStageHeight = 0f, MaxStageHeight = 2f;

    // ── Stage-height presets ─────────────────────────────────────
    public static readonly float[] StageHeightPresets = { 0.6f, 1.0f, 1.2f };

    // ── Validation helpers ───────────────────────────────────────

    public static bool ValidateCols(int v, out string err)
    {
        err = null;
        if (v < MinCols || v > MaxCols)
        {
            err = $"cols must be {MinCols}–{MaxCols} (got {v})";
            return false;
        }
        return true;
    }

    public static bool ValidateRows(int v, out string err)
    {
        err = null;
        if (v < MinRows || v > MaxRows)
        {
            err = $"rows must be {MinRows}–{MaxRows} (got {v})";
            return false;
        }
        return true;
    }

    public static bool ValidateWm(float v, out string err)
    {
        err = null;
        if (v < MinWm || v > MaxWm)
        {
            err = $"Wm must be {MinWm}–{MaxWm} m (got {v})";
            return false;
        }
        return true;
    }

    public static bool ValidateHm(float v, out string err)
    {
        err = null;
        if (v < MinHm || v > MaxHm)
        {
            err = $"Hm must be {MinHm}–{MaxHm} m (got {v})";
            return false;
        }
        return true;
    }

    public static bool ValidateB(float v, out string err)
    {
        err = null;
        if (v < MinB || v > MaxB)
        {
            err = $"B (LED bottom) must be {MinB}–{MaxB} m (got {v})";
            return false;
        }
        return true;
    }

    public static bool ValidateD(float v, out string err)
    {
        err = null;
        if (v < MinD || v > MaxD)
        {
            err = $"D (distance) must be {MinD}–{MaxD} m (got {v})";
            return false;
        }
        return true;
    }

    public static bool ValidateE(float v, out string err)
    {
        err = null;
        if (v < MinE || v > MaxE)
        {
            err = $"E (eye height) must be {MinE}–{MaxE} m (got {v})";
            return false;
        }
        return true;
    }

    public static bool ValidateStageHeight(float v, out string err)
    {
        err = null;
        if (v < MinStageHeight || v > MaxStageHeight)
        {
            err = $"stageHeight must be {MinStageHeight}–{MaxStageHeight} m (got {v})";
            return false;
        }
        return true;
    }
}
