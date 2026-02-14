using UnityEngine;

/// <summary>
/// Controls the LED box geometry, XR Origin position, and computes
/// viewing-angle feedback.  All mutations happen only via Apply().
///
/// ── Coordinate convention ──────────────────────────────────────
///   • LED front face sits at  z = 0   (audience side)
///   • LED centre is at        z = +PanelT / 2   (0.045 m)
///   • Audience (XR Origin) is at z = −D
///     → distance from eye to LED front = D  (exactly)
///   • Y-axis  = up;  floor at y = 0
///   • LED bottom edge at y = B;  LED centre at y = B + Hm/2
/// </summary>
public class ScreenController : MonoBehaviour
{
    // ── Scene references (assign in Inspector) ───────────────────
    [Header("Scene References")]
    [Tooltip("The Box (Cube) representing the LED screen")]
    public Transform ledBox;

    [Tooltip("XR Origin / Camera Offset root")]
    public Transform xrOrigin;

    [Tooltip("Floor plane")]
    public Transform floorPlane;

    [Tooltip("1 m reference ruler")]
    public Transform ruler;

    // ── Cached results (read by UIController for display) ────────
    [HideInInspector] public float lastWm;
    [HideInInspector] public float lastHm;
    [HideInInspector] public float lastThetaH;  // degrees
    [HideInInspector] public float lastThetaV;  // degrees
    [HideInInspector] public float lastPitch;   // degrees

    /// <summary>
    /// Apply a new configuration to the scene.
    /// Called once per user click — never per-frame.
    /// </summary>
    public void Apply(float Wm, float Hm, float B, float D, float E)
    {
        // ── 1. LED box scale & position ──────────────────────────
        //  Scale: width=Wm, height=Hm, depth=PanelT
        ledBox.localScale = new Vector3(Wm, Hm, ScreenConfig.PanelT);

        //  Position: centre the box so that its FRONT face is at z=0.
        //    front face z = centre_z − depth/2  →  centre_z = depth/2
        //  Bottom edge at y=B  → centre_y = B + Hm/2
        float centreY = B + Hm * 0.5f;
        float centreZ = ScreenConfig.PanelT * 0.5f;
        ledBox.position = new Vector3(0f, centreY, centreZ);

        // ── 2. XR Origin (audience) position ─────────────────────
        //  Eye should end up at (0, E, −D).
        //  XR Origin is the tracking-space root. The HMD adds the
        //  physical head offset on top. For a standing-reset pose
        //  the head is at local (0,E,0) relative to the origin,
        //  so we place the origin at (0, 0, −D).  The real-world
        //  eye height E is achieved by setting the origin Y so that
        //  floor-level tracking + user's real head height ≈ E.
        //  In practice on Quest 2 with "floor level" tracking the
        //  HMD Y already represents real height, so placing origin
        //  at (0, 0, −D) gives the correct result.
        //  We still expose E for the angle calculation & as a
        //  conceptual reference.
        xrOrigin.position = new Vector3(0f, 0f, -D);

        // ── 3. Floor ─────────────────────────────────────────────
        if (floorPlane != null)
        {
            floorPlane.position = Vector3.zero;
            // Default Unity plane is 10x10 m; scale to 100x100 m
            floorPlane.localScale = new Vector3(10f, 1f, 10f);
        }

        // ── 4. Ruler (1 m vertical stick at LED base) ────────────
        if (ruler != null)
        {
            // Thin cylinder: height 1 m, placed next to the LED
            ruler.localScale = new Vector3(0.02f, 0.5f, 0.02f);
            // Unity cylinder is 2 m tall at scale 1; scale.y=0.5 → 1 m
            // Place it to the right of the LED, base on the floor
            ruler.position = new Vector3(Wm * 0.5f + 0.3f, 0.5f, 0f);
        }

        // ── 5. Viewing-angle calculations ────────────────────────
        //  All formulas use D = front-face distance (already correct).
        //  Horizontal FOV  θH = 2 · atan( (Wm/2) / D )
        //  Vertical   FOV  θV = 2 · atan( (Hm/2) / D )
        //  Pitch angle      = atan( (ledCentreY − E) / D )
        lastWm     = Wm;
        lastHm     = Hm;
        lastThetaH = 2f * Mathf.Atan(Wm * 0.5f / D) * Mathf.Rad2Deg;
        lastThetaV = 2f * Mathf.Atan(Hm * 0.5f / D) * Mathf.Rad2Deg;

        float dy = centreY - E;
        lastPitch  = Mathf.Atan(dy / D) * Mathf.Rad2Deg;
    }
}
