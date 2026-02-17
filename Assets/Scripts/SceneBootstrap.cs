using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds the entire demo scene from code so no .unity scene file is needed.
/// Attach this to an empty GameObject named "Bootstrap" in a blank scene,
/// OR use the provided SceneHierarchy guide to set up manually in Editor.
///
/// This exists so the project is runnable with minimal Unity Editor steps.
/// </summary>
public class SceneBootstrap : MonoBehaviour
{
    void Awake()
    {
        // ── Resolve shaders ─────────────────────────────────────
        // Capture the default shader from a primitive — guaranteed to exist
        // in whatever render pipeline the project uses, and never stripped.
        Shader litShader = ResolveLitShader();
        Shader unlitShader = ResolveUnlitShader();

        // ── 1. Floor ─────────────────────────────────────────────
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(10f, 1f, 10f);  // 100x100 m
        var floorMat = new Material(litShader);
        floorMat.color = new Color(0.25f, 0.25f, 0.25f);
        floor.GetComponent<Renderer>().material = floorMat;

        // ── 2. LED Box ───────────────────────────────────────────
        GameObject ledBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ledBox.name = "LEDBox";
        var ledMat = new Material(litShader);
        ledMat.color = new Color(0.1f, 0.1f, 0.15f);             // dark panel
        if (ledMat.HasProperty("_Smoothness"))
            ledMat.SetFloat("_Smoothness", 0.3f);
        ledBox.GetComponent<Renderer>().material = ledMat;

        // Front-face emissive highlight so you can tell which side is the screen
        GameObject ledFront = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ledFront.name = "LEDFrontFace";
        ledFront.transform.SetParent(ledBox.transform);
        ledFront.transform.localPosition = new Vector3(0f, 0f, -0.501f); // slightly in front
        ledFront.transform.localRotation = Quaternion.identity;
        ledFront.transform.localScale    = new Vector3(1f, 1f, 1f);
        var frontMat = new Material(unlitShader);
        frontMat.color = new Color(0.05f, 0.15f, 0.4f);          // dim blue "screen"
        ledFront.GetComponent<Renderer>().material = frontMat;
        // Remove collider from the quad — it's purely visual
        Destroy(ledFront.GetComponent<Collider>());

        // ── 3. Ruler (1 m reference) ─────────────────────────────
        GameObject ruler = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ruler.name = "Ruler_1m";
        var rulerMat = new Material(litShader);
        rulerMat.color = Color.yellow;
        ruler.GetComponent<Renderer>().material = rulerMat;

        // ── 4. Stage platform (visual only) ──────────────────────
        GameObject stage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stage.name = "StagePlatform";
        var stageMat = new Material(litShader);
        stageMat.color = new Color(0.35f, 0.22f, 0.1f);          // brown wood
        stage.GetComponent<Renderer>().material = stageMat;
        // Will be repositioned by UpdateStagePlatform() after Apply

        // ── 5. XR Origin ─────────────────────────────────────────
        // In a real build you would use the XR Interaction Toolkit
        // prefab.  Here we create a minimal stand-in so the script
        // compiles and runs in Editor as well.
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin == null)
        {
            xrOrigin = new GameObject("XR Origin (XR Rig)");
            GameObject cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOrigin.transform);
            cameraOffset.transform.localPosition = Vector3.zero;

            GameObject mainCam = Camera.main?.gameObject;
            if (mainCam == null)
            {
                mainCam = new GameObject("Main Camera");
                mainCam.AddComponent<Camera>();
                mainCam.tag = "MainCamera";
            }
            mainCam.transform.SetParent(cameraOffset.transform);
            mainCam.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            mainCam.transform.localRotation = Quaternion.identity;
        }

        // ── 6. Directional Light ─────────────────────────────────
        if (FindAnyObjectByType<Light>() == null)
        {
            GameObject lightGO = new GameObject("Directional Light");
            Light l = lightGO.AddComponent<Light>();
            l.type      = LightType.Directional;
            l.intensity = 1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // ── 7. World-Space UI Canvas ─────────────────────────────
        GameObject canvasGO = new GameObject("UICanvas");
        Canvas canvas       = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta     = new Vector2(900, 700);
        canvasRT.localScale    = Vector3.one * 0.002f;       // 1 px ≈ 2 mm
        // Place the UI panel to the left of the viewer
        canvasGO.transform.position = new Vector3(-2.5f, 1.5f, -1f);
        canvasGO.transform.rotation = Quaternion.Euler(0f, 30f, 0f);

        // ── 8. Build UI elements ─────────────────────────────────
        UIController ui = canvasGO.AddComponent<UIController>();
        BuildUI(canvasRT, ui);

        // ── 9. Screen Controller ─────────────────────────────────
        GameObject ctrlGO = new GameObject("ScreenController");
        ScreenController ctrl = ctrlGO.AddComponent<ScreenController>();
        ctrl.ledBox    = ledBox.transform;
        ctrl.xrOrigin  = xrOrigin.transform;
        ctrl.floorPlane = floor.transform;
        ctrl.ruler     = ruler.transform;

        ui.screenController = ctrl;

        // Store stage ref for later repositioning
        _stagePlatform = stage.transform;
        _screenCtrl    = ctrl;
        _uiCtrl        = ui;
    }

    Transform _stagePlatform;
    ScreenController _screenCtrl;
    UIController _uiCtrl;

    // After the first Apply (which happens in UIController.Start),
    // we reposition the stage platform to match.
    void LateUpdate()
    {
        if (_stagePlatform == null || _screenCtrl == null) return;
        // Keep stage platform under the LED box
        float stageH;
        if (float.TryParse(_uiCtrl.inputStageHeight?.text ?? "0.6",
                           System.Globalization.NumberStyles.Float,
                           System.Globalization.CultureInfo.InvariantCulture,
                           out stageH) && stageH > 0.01f)
        {
            float ledWm = _screenCtrl.lastWm > 0 ? _screenCtrl.lastWm : 4f;
            _stagePlatform.localScale = new Vector3(ledWm + 2f, stageH, 3f);
            _stagePlatform.position   = new Vector3(0f, stageH * 0.5f, 1f);
        }
    }

    // ══════════════════════════════════════════════════════════════
    // UI Builder  (programmatic – no prefab needed)
    // ══════════════════════════════════════════════════════════════
    void BuildUI(RectTransform root, UIController ui)
    {
        float y = 320f;   // start from top
        float labelW = 200f;
        float fieldW = 160f;
        float rowH   = 36f;
        float gap    = 6f;

        // Background panel
        Image bg = root.gameObject.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);

        // Title
        CreateLabel(root, "LED Screen VR Demo", 22, TextAlignmentOptions.Center,
                    new Vector2(0, y), new Vector2(860, 40));
        y -= 50;

        // ── Mode toggle row ──────────────────────────────────────
        ui.modeLabel = CreateLabel(root, "Mode B (panels)", 16,
                                   TextAlignmentOptions.Left,
                                   new Vector2(-200, y), new Vector2(300, rowH));
        ui.modeToggle = CreateToggle(root, new Vector2(230, y));
        CreateLabel(root, "A/B", 14, TextAlignmentOptions.Left,
                    new Vector2(270, y), new Vector2(60, rowH));
        y -= rowH + gap + 8;

        // ── Mode A group ─────────────────────────────────────────
        GameObject modeAGrp = CreateGroup(root, "ModeAGroup");
        ui.modeAGroup = modeAGrp;
        RectTransform aRT = modeAGrp.GetComponent<RectTransform>();
        aRT.anchoredPosition = new Vector2(0, y);
        aRT.sizeDelta = new Vector2(860, (rowH + gap) * 2);

        ui.inputWm = CreateFieldRow(aRT, "Wm (m)", "4.0",  0);
        ui.inputHm = CreateFieldRow(aRT, "Hm (m)", "2.0", -1);
        y -= (rowH + gap) * 2 + 8;

        // ── Mode B group ─────────────────────────────────────────
        GameObject modeBGrp = CreateGroup(root, "ModeBGroup");
        ui.modeBGroup = modeBGrp;
        RectTransform bRT = modeBGrp.GetComponent<RectTransform>();
        bRT.anchoredPosition = new Vector2(0, y + (rowH + gap) * 2 + 8); // same spot
        bRT.sizeDelta = new Vector2(860, (rowH + gap) * 2);

        ui.inputCols = CreateFieldRow(bRT, "Cols",  "8",  0);
        ui.inputRows = CreateFieldRow(bRT, "Rows",  "4", -1);

        // ── Common inputs ────────────────────────────────────────
        y -= 8;
        // Stage height with presets
        CreateLabel(root, "stageHeight", 14, TextAlignmentOptions.Left,
                    new Vector2(-420, y), new Vector2(labelW, rowH));
        ui.inputStageHeight = CreateInputField(root, "0.6",
                    new Vector2(-80, y), new Vector2(fieldW, rowH));

        ui.btnStage06 = CreateButton(root, "0.6", new Vector2(140, y), new Vector2(60, rowH));
        ui.btnStage10 = CreateButton(root, "1.0", new Vector2(210, y), new Vector2(60, rowH));
        ui.btnStage12 = CreateButton(root, "1.2", new Vector2(280, y), new Vector2(60, rowH));
        y -= rowH + gap;

        // B
        ui.inputB = CreateLabeledField(root, "B (LED bottom m)", "0.6", ref y, labelW, fieldW, rowH, gap);
        // D
        ui.inputD = CreateLabeledField(root, "D (distance m)", "10.0", ref y, labelW, fieldW, rowH, gap);
        // E
        ui.inputE = CreateLabeledField(root, "E (eye height m)", "1.6", ref y, labelW, fieldW, rowH, gap);

        y -= 10;

        // ── Apply button ─────────────────────────────────────────
        ui.btnApply = CreateButton(root, "APPLY", new Vector2(0, y), new Vector2(200, 44));
        // Make it stand out
        ui.btnApply.GetComponent<Image>().color = new Color(0.1f, 0.5f, 0.2f);
        y -= 54;

        // ── Warning text ─────────────────────────────────────────
        ui.warningText = CreateLabel(root, "", 14, TextAlignmentOptions.Left,
                                     new Vector2(0, y), new Vector2(800, 30));
        ui.warningText.color = new Color(1f, 0.3f, 0.3f);
        y -= 36;

        // ── Result text ──────────────────────────────────────────
        ui.resultText = CreateLabel(root, "", 16, TextAlignmentOptions.Left,
                                    new Vector2(0, y), new Vector2(800, 120));
        ui.resultText.color = Color.white;
    }

    // ── UI creation helpers ──────────────────────────────────────
    static GameObject CreateGroup(RectTransform parent, string name)
    {
        GameObject go = new GameObject(name);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        return go;
    }

    static TMP_Text CreateLabel(RectTransform parent, string text, int size,
                                TextAlignmentOptions align, Vector2 pos, Vector2 sz)
    {
        GameObject go = new GameObject("Label");
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sz;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.alignment = align;
        tmp.color     = Color.white;
        return tmp;
    }

    static TMP_InputField CreateInputField(RectTransform parent, string defaultVal,
                                            Vector2 pos, Vector2 sz)
    {
        // Background image
        GameObject go = new GameObject("InputField");
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sz;
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f);

        // Text area
        GameObject textArea = new GameObject("Text Area");
        RectTransform taRT = textArea.AddComponent<RectTransform>();
        taRT.SetParent(rt, false);
        taRT.anchorMin = Vector2.zero;
        taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(8, 2);
        taRT.offsetMax = new Vector2(-8, -2);
        textArea.AddComponent<RectMask2D>();

        // Placeholder
        GameObject phGO = new GameObject("Placeholder");
        RectTransform phRT = phGO.AddComponent<RectTransform>();
        phRT.SetParent(taRT, false);
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero;
        phRT.offsetMax = Vector2.zero;
        TextMeshProUGUI phTMP = phGO.AddComponent<TextMeshProUGUI>();
        phTMP.text      = "...";
        phTMP.fontSize  = 14;
        phTMP.color     = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        phTMP.alignment = TextAlignmentOptions.Left;

        // Display text
        GameObject txtGO = new GameObject("Text");
        RectTransform txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.SetParent(taRT, false);
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
        TextMeshProUGUI txtTMP = txtGO.AddComponent<TextMeshProUGUI>();
        txtTMP.fontSize  = 14;
        txtTMP.color     = Color.white;
        txtTMP.alignment = TextAlignmentOptions.Left;

        // TMP Input Field component
        TMP_InputField field = go.AddComponent<TMP_InputField>();
        field.textViewport  = taRT;
        field.textComponent = txtTMP;
        field.placeholder   = phTMP;
        field.text          = defaultVal;
        field.contentType   = TMP_InputField.ContentType.DecimalNumber;
        field.pointSize     = 14;

        return field;
    }

    static TMP_InputField CreateFieldRow(RectTransform parent, string label,
                                          string defaultVal, int rowIndex)
    {
        float rowH = 36f;
        float gap  = 6f;
        float yOff = rowIndex * (rowH + gap);
        CreateLabel(parent, label, 14, TextAlignmentOptions.Left,
                    new Vector2(-420, yOff), new Vector2(200, rowH));
        return CreateInputField(parent, defaultVal,
                    new Vector2(-80, yOff), new Vector2(160, rowH));
    }

    static TMP_InputField CreateLabeledField(RectTransform parent, string label,
            string defaultVal, ref float y, float labelW, float fieldW,
            float rowH, float gap)
    {
        CreateLabel(parent, label, 14, TextAlignmentOptions.Left,
                    new Vector2(-420, y), new Vector2(labelW, rowH));
        var f = CreateInputField(parent, defaultVal,
                    new Vector2(-80, y), new Vector2(fieldW, rowH));
        y -= rowH + gap;
        return f;
    }

    static Toggle CreateToggle(RectTransform parent, Vector2 pos)
    {
        GameObject go = new GameObject("Toggle");
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(30, 30);

        // Background
        GameObject bgGO = new GameObject("Background");
        RectTransform bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.SetParent(rt, false);
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // Checkmark
        GameObject ckGO = new GameObject("Checkmark");
        RectTransform ckRT = ckGO.AddComponent<RectTransform>();
        ckRT.SetParent(bgRT, false);
        ckRT.anchorMin = new Vector2(0.15f, 0.15f);
        ckRT.anchorMax = new Vector2(0.85f, 0.85f);
        ckRT.offsetMin = Vector2.zero; ckRT.offsetMax = Vector2.zero;
        Image ckImg = ckGO.AddComponent<Image>();
        ckImg.color = new Color(0.2f, 0.8f, 0.3f);

        Toggle toggle = go.AddComponent<Toggle>();
        toggle.targetGraphic = bgImg;
        toggle.graphic       = ckImg;
        return toggle;
    }

    static Button CreateButton(RectTransform parent, string label,
                                Vector2 pos, Vector2 sz)
    {
        GameObject go = new GameObject("Btn_" + label);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sz;
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // Label
        GameObject txtGO = new GameObject("Label");
        RectTransform txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.SetParent(rt, false);
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 14;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    // ══════════════════════════════════════════════════════════════
    // Shader resolution — works with URP, Built-in RP, or any pipeline
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a lit shader that is guaranteed to work in the current pipeline.
    /// Captures the default shader Unity assigns to primitives as final fallback,
    /// since that shader is always compiled and included in the build.
    /// </summary>
    static Shader ResolveLitShader()
    {
        // 1. Try URP
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s != null) return s;

        // 2. Try Built-in Standard
        s = Shader.Find("Standard");
        if (s != null) return s;

        // 3. Try mobile-friendly shaders (less likely to be stripped)
        s = Shader.Find("Mobile/Diffuse");
        if (s != null) return s;

        s = Shader.Find("Legacy Shaders/Diffuse");
        if (s != null) return s;

        // 4. Final fallback: grab whatever shader the engine uses for primitives
        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s = tmp.GetComponent<Renderer>().sharedMaterial.shader;
        Destroy(tmp);

        Debug.LogWarning("[SceneBootstrap] Using primitive default shader as fallback: " + s.name);
        return s;
    }

    /// <summary>
    /// Returns an unlit shader for the LED front face.
    /// </summary>
    static Shader ResolveUnlitShader()
    {
        Shader s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s != null) return s;

        s = Shader.Find("Unlit/Color");
        if (s != null) return s;

        s = Shader.Find("UI/Default");
        if (s != null) return s;

        // Fallback to lit shader — still better than pink
        return ResolveLitShader();
    }
}
