using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public static class DemoFixUI
{
    [MenuItem("Demo Final/8 - Fix Input System + UI Layout")]
    public static void FixAll()
    {
        FixInputSystem();
        FixUILayout();
        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log("[Fix UI] All fixes applied and project saved ✓");
    }

    // ── 1. Fix EventSystem: swap StandaloneInputModule → InputSystemUIInputModule ──
    static void FixInputSystem()
    {
        var esGO = GameObject.Find("EventSystem");
        if (esGO == null) { Debug.LogWarning("[Fix UI] EventSystem not found"); return; }

        // Remove old module
        var old = esGO.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (old != null)
        {
            Object.DestroyImmediate(old);
            Debug.Log("[Fix UI] Removed StandaloneInputModule ✓");
        }

        // Add new module
        if (esGO.GetComponent<InputSystemUIInputModule>() == null)
        {
            esGO.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[Fix UI] Added InputSystemUIInputModule ✓");
        }

        EditorUtility.SetDirty(esGO);
    }

    // ── 2. Fix UI Layout: size & position the panel and buttons ──────────────
    static void FixUILayout()
    {
        // Canvas — Screen Space Overlay, reference res 1920x1080
        var canvasGO = GameObject.Find("Canvas_Demo");
        if (canvasGO == null) { Debug.LogWarning("[Fix UI] Canvas_Demo not found"); return; }

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
        }

        // Panel — bottom strip, full width, 160px tall
        var panelRT = GetRT("Canvas_Demo/Panel_Controls");
        if (panelRT != null)
        {
            panelRT.anchorMin        = new Vector2(0, 0);
            panelRT.anchorMax        = new Vector2(1, 0);
            panelRT.pivot            = new Vector2(0.5f, 0);
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta        = new Vector2(0, 160);
        }

        // Effect name label — top of panel
        var lblRT = GetRT("Canvas_Demo/Panel_Controls/TXT_EffectName");
        if (lblRT != null)
        {
            lblRT.anchorMin        = new Vector2(0, 1);
            lblRT.anchorMax        = new Vector2(1, 1);
            lblRT.pivot            = new Vector2(0.5f, 1);
            lblRT.anchoredPosition = new Vector2(0, -8);
            lblRT.sizeDelta        = new Vector2(-20, 36);
            var txt = lblRT.GetComponent<Text>();
            if (txt != null) { txt.alignment = TextAnchor.MiddleCenter; txt.fontSize = 22; }
        }

        // 3 effect buttons side by side
        string[] btnNames = { "BTN_Lightning", "BTN_Fire", "BTN_Portal" };
        float btnW = 220f, btnH = 60f, spacing = 20f;
        float totalW = btnNames.Length * btnW + (btnNames.Length - 1) * spacing;
        float startX = -totalW / 2f + btnW / 2f;

        for (int i = 0; i < btnNames.Length; i++)
        {
            var rt = GetRT("Canvas_Demo/Panel_Controls/" + btnNames[i]);
            if (rt == null) continue;
            rt.anchorMin        = new Vector2(0.5f, 0);
            rt.anchorMax        = new Vector2(0.5f, 0);
            rt.pivot            = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(startX + i * (btnW + spacing), 55);
            rt.sizeDelta        = new Vector2(btnW, btnH);

            // Center label text
            var lblChild = rt.Find("Label");
            if (lblChild != null)
            {
                var lrt = lblChild.GetComponent<RectTransform>();
                if (lrt != null) { lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero; lrt.anchoredPosition = Vector2.zero; }
                var t = lblChild.GetComponent<Text>();
                if (t != null) t.alignment = TextAnchor.MiddleCenter;
            }
        }

        // Play/Pause button — right side
        var ppRT = GetRT("Canvas_Demo/Panel_Controls/BTN_PlayPause");
        if (ppRT != null)
        {
            ppRT.anchorMin        = new Vector2(1, 0);
            ppRT.anchorMax        = new Vector2(1, 0);
            ppRT.pivot            = new Vector2(1, 0);
            ppRT.anchoredPosition = new Vector2(-30, 55);
            ppRT.sizeDelta        = new Vector2(180, 60);

            var lblChild = ppRT.Find("Label");
            if (lblChild != null)
            {
                var lrt = lblChild.GetComponent<RectTransform>();
                if (lrt != null) { lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero; lrt.anchoredPosition = Vector2.zero; }
                var t = lblChild.GetComponent<Text>();
                if (t != null) t.alignment = TextAnchor.MiddleCenter;
            }
        }

        EditorUtility.SetDirty(canvasGO);
        Debug.Log("[Fix UI] UI Layout fixed ✓");
    }

    static RectTransform GetRT(string path)
    {
        // Search including inactive objects
        foreach (var t in Resources.FindObjectsOfTypeAll<RectTransform>())
        {
            if (!t.gameObject.scene.isLoaded) continue;
            if (GetPath(t) == path) return t;
        }
        // Fallback: try active only
        var go = GameObject.Find(path);
        return go != null ? go.GetComponent<RectTransform>() : null;
    }

    static string GetPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetPath(t.parent) + "/" + t.name;
    }
}
