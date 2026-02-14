using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// World-space UI: mode toggle, input fields, preset buttons,
/// Apply button, result/warning display.
/// Connects to ScreenController.Apply() on button press only.
/// </summary>
public class UIController : MonoBehaviour
{
    // ── Scene wiring ─────────────────────────────────────────────
    [Header("Controller")]
    public ScreenController screenController;

    // ── Mode toggle ──────────────────────────────────────────────
    [Header("Mode Toggle")]
    [Tooltip("Toggle: ON = Mode A (metres), OFF = Mode B (panels)")]
    public Toggle modeToggle;           // isOn → Mode A
    public TMP_Text modeLabel;

    // ── Mode A fields ────────────────────────────────────────────
    [Header("Mode A – Direct metres")]
    public GameObject modeAGroup;
    public TMP_InputField inputWm;
    public TMP_InputField inputHm;

    // ── Mode B fields ────────────────────────────────────────────
    [Header("Mode B – Panel count (default)")]
    public GameObject modeBGroup;
    public TMP_InputField inputCols;
    public TMP_InputField inputRows;

    // ── Common fields ────────────────────────────────────────────
    [Header("Common Inputs")]
    public TMP_InputField inputStageHeight;
    public TMP_InputField inputB;
    public TMP_InputField inputD;
    public TMP_InputField inputE;

    // ── Stage-height preset buttons ──────────────────────────────
    [Header("Stage Height Presets")]
    public Button btnStage06;
    public Button btnStage10;
    public Button btnStage12;

    // ── Apply ────────────────────────────────────────────────────
    [Header("Apply")]
    public Button btnApply;

    // ── Output ───────────────────────────────────────────────────
    [Header("Output")]
    public TMP_Text resultText;
    public TMP_Text warningText;

    // ══════════════════════════════════════════════════════════════
    // MonoBehaviour
    // ══════════════════════════════════════════════════════════════
    void Start()
    {
        // Wire buttons
        btnApply.onClick.AddListener(OnApply);
        btnStage06.onClick.AddListener(() => SetStageHeight(0.6f));
        btnStage10.onClick.AddListener(() => SetStageHeight(1.0f));
        btnStage12.onClick.AddListener(() => SetStageHeight(1.2f));

        // Wire mode toggle
        modeToggle.onValueChanged.AddListener(OnModeChanged);

        // Set defaults into fields
        inputCols.text        = ScreenConfig.DefaultCols.ToString();
        inputRows.text        = ScreenConfig.DefaultRows.ToString();
        inputWm.text          = (ScreenConfig.DefaultCols * ScreenConfig.PanelW).ToString("F1");
        inputHm.text          = (ScreenConfig.DefaultRows * ScreenConfig.PanelH).ToString("F1");
        inputStageHeight.text = ScreenConfig.DefaultStageHeight.ToString("F1");
        inputB.text           = ScreenConfig.DefaultStageHeight.ToString("F1");
        inputD.text           = ScreenConfig.DefaultD.ToString("F1");
        inputE.text           = ScreenConfig.DefaultE.ToString("F1");

        // Start in Mode B (panel count)
        modeToggle.isOn = false;
        OnModeChanged(false);

        // Initial apply
        OnApply();
    }

    // ── Mode switching ───────────────────────────────────────────
    void OnModeChanged(bool isOn)
    {
        bool modeA = isOn;
        modeAGroup.SetActive(modeA);
        modeBGroup.SetActive(!modeA);
        modeLabel.text = modeA ? "Mode A (metres)" : "Mode B (panels)";
    }

    // ── Stage-height preset ──────────────────────────────────────
    void SetStageHeight(float h)
    {
        inputStageHeight.text = h.ToString("F1");
        // Also update B to match if user hasn't manually changed it
        inputB.text = h.ToString("F1");
    }

    // ══════════════════════════════════════════════════════════════
    // Apply
    // ══════════════════════════════════════════════════════════════
    void OnApply()
    {
        warningText.text = "";
        bool modeA = modeToggle.isOn;

        // ── Parse Wm / Hm ───────────────────────────────────────
        float Wm, Hm;
        if (modeA)
        {
            if (!TryParseFloat(inputWm, "Wm", out Wm)) return;
            if (!TryParseFloat(inputHm, "Hm", out Hm)) return;
            if (!Check(ScreenConfig.ValidateWm(Wm, out string ew), ew)) return;
            if (!Check(ScreenConfig.ValidateHm(Hm, out string eh), eh)) return;
        }
        else
        {
            if (!TryParseInt(inputCols, "cols", out int cols)) return;
            if (!TryParseInt(inputRows, "rows", out int rows)) return;
            if (!Check(ScreenConfig.ValidateCols(cols, out string ec), ec)) return;
            if (!Check(ScreenConfig.ValidateRows(rows, out string er), er)) return;
            Wm = cols * ScreenConfig.PanelW;
            Hm = rows * ScreenConfig.PanelH;
        }

        // ── Parse common fields ──────────────────────────────────
        if (!TryParseFloat(inputStageHeight, "stageHeight", out float stageH)) return;
        if (!Check(ScreenConfig.ValidateStageHeight(stageH, out string es), es)) return;

        if (!TryParseFloat(inputB, "B", out float B)) return;
        if (!Check(ScreenConfig.ValidateB(B, out string eb), eb)) return;

        if (!TryParseFloat(inputD, "D", out float D)) return;
        if (!Check(ScreenConfig.ValidateD(D, out string ed), ed)) return;

        if (!TryParseFloat(inputE, "E", out float E)) return;
        if (!Check(ScreenConfig.ValidateE(E, out string ee), ee)) return;

        // ── Delegate to controller ───────────────────────────────
        screenController.Apply(Wm, Hm, B, D, E);

        // ── Show result ──────────────────────────────────────────
        resultText.text =
            $"LED  {screenController.lastWm:F2} x {screenController.lastHm:F2} m\n" +
            $"Horiz FOV   {screenController.lastThetaH:F1}°\n" +
            $"Vert  FOV   {screenController.lastThetaV:F1}°\n" +
            $"Pitch       {screenController.lastPitch:F1}°";
    }

    // ══════════════════════════════════════════════════════════════
    // Parsing helpers
    // ══════════════════════════════════════════════════════════════
    bool TryParseFloat(TMP_InputField field, string name, out float value)
    {
        value = 0f;
        if (float.TryParse(field.text, System.Globalization.NumberStyles.Float,
                           System.Globalization.CultureInfo.InvariantCulture, out value))
            return true;

        warningText.text = $"Cannot parse '{name}' – enter a valid number.";
        return false;
    }

    bool TryParseInt(TMP_InputField field, string name, out int value)
    {
        value = 0;
        if (int.TryParse(field.text, out value))
            return true;

        warningText.text = $"Cannot parse '{name}' – enter a whole number.";
        return false;
    }

    bool Check(bool ok, string err)
    {
        if (!ok && err != null)
            warningText.text = err;
        return ok;
    }
}
