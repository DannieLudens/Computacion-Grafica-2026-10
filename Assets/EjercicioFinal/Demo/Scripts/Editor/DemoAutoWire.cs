using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// Auto-wires all DemoController serialized references by finding objects in the scene.
public static class DemoAutoWire
{
    [MenuItem("Demo Final/5 - Auto-Wire DemoController References")]
    public static void AutoWire()
    {
        // Find DemoController
        var dcGO = GameObject.Find("DemoController");
        if (dcGO == null) { Debug.LogError("[AutoWire] DemoController GameObject not found!"); return; }

        var dc = dcGO.GetComponent<DemoController>();
        if (dc == null) { Debug.LogError("[AutoWire] DemoController script not found on DemoController!"); return; }

        // --- Effects (allow inactive) ---
        dc.effects = new GameObject[3];
        dc.effects[0] = FindInactive("EGO_Effect_Lightning");
        dc.effects[1] = FindInactive("EGO_Effect_Fire");
        dc.effects[2] = FindInactive("EGO_Effect_Portal");

        // --- Effect Volumes (allow inactive) ---
        dc.effectVolumes = new UnityEngine.Rendering.Volume[3];
        dc.effectVolumes[0] = FindVolumeInactive("Volume_Lightning");
        dc.effectVolumes[1] = FindVolumeInactive("Volume_Fire");
        dc.effectVolumes[2] = FindVolumeInactive("Volume_Portal");

        // --- Character Animator ---
        var charGO = GameObject.Find("AttackCharacter_GreatSword");
        dc.characterAnimator = charGO != null ? charGO.GetComponent<Animator>() : null;

        // --- UI Buttons ---
        dc.effectButtons = new Button[3];
        dc.effectButtons[0] = FindButton("BTN_Lightning");
        dc.effectButtons[1] = FindButton("BTN_Fire");
        dc.effectButtons[2] = FindButton("BTN_Portal");
        dc.playPauseButton  = FindButton("BTN_PlayPause");

        // --- Labels ---
        dc.effectNameLabel = FindText("TXT_EffectName");
        dc.playPauseLabel  = FindTextInChild("BTN_PlayPause", "Label");

        // --- CameraFollow ---
        var camGO = GameObject.Find("Main Camera");
        dc.cameraFollow = camGO != null ? camGO.GetComponent<CameraFollow>() : null;
        Debug.Log($"  cameraFollow           = {(dc.cameraFollow ? "OK" : "NULL")}");

        // --- Mark dirty & save ---
        EditorUtility.SetDirty(dcGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            dcGO.scene);

        // Report
        Debug.Log("[AutoWire] Done! Results:");
        Debug.Log($"  effects[0] (Lightning) = {(dc.effects[0] ? dc.effects[0].name : "NULL")}");
        Debug.Log($"  effects[1] (Fire)      = {(dc.effects[1] ? dc.effects[1].name : "NULL")}");
        Debug.Log($"  effects[2] (Portal)    = {(dc.effects[2] ? dc.effects[2].name : "NULL")}");
        Debug.Log($"  volumes[0]             = {(dc.effectVolumes[0] ? dc.effectVolumes[0].name : "NULL")}");
        Debug.Log($"  volumes[1]             = {(dc.effectVolumes[1] ? dc.effectVolumes[1].name : "NULL")}");
        Debug.Log($"  volumes[2]             = {(dc.effectVolumes[2] ? dc.effectVolumes[2].name : "NULL")}");
        Debug.Log($"  characterAnimator      = {(dc.characterAnimator ? dc.characterAnimator.gameObject.name : "NULL")}");
        Debug.Log($"  effectButtons[0..2]    = {(dc.effectButtons[0] ? "OK" : "NULL")}, {(dc.effectButtons[1] ? "OK" : "NULL")}, {(dc.effectButtons[2] ? "OK" : "NULL")}");
        Debug.Log($"  playPauseButton        = {(dc.playPauseButton ? "OK" : "NULL")}");
        Debug.Log($"  effectNameLabel        = {(dc.effectNameLabel ? "OK" : "NULL")}");
        Debug.Log($"  playPauseLabel         = {(dc.playPauseLabel ? "OK" : "NULL")}");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    static GameObject FindInactive(string name)
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            if (t.name == name && t.gameObject.scene.isLoaded)
                return t.gameObject;
        Debug.LogWarning("[AutoWire] Not found: " + name);
        return null;
    }

    static UnityEngine.Rendering.Volume FindVolumeInactive(string name)
    {
        var go = FindInactive(name);
        return go != null ? go.GetComponent<UnityEngine.Rendering.Volume>() : null;
    }

    static Button FindButton(string name)
    {
        // Search active + inactive
        foreach (var b in Resources.FindObjectsOfTypeAll<Button>())
            if (b.gameObject.name == name && b.gameObject.scene.isLoaded)
                return b;
        // Fallback: find GO and get component
        var go = FindInactive(name);
        return go != null ? go.GetComponent<Button>() : null;
    }

    static Text FindText(string name)
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Text>())
            if (t.gameObject.name == name && t.gameObject.scene.isLoaded)
                return t;
        return null;
    }

    static Text FindTextInChild(string parentName, string childName)
    {
        var parent = FindInactive(parentName);
        if (parent == null) return null;
        var child = parent.transform.Find(childName);
        return child != null ? child.GetComponent<Text>() : null;
    }
}
