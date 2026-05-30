using UnityEngine;
using UnityEditor;

public static class DemoReparentEffects
{
    // ── 9. Reparent Lightning + Fire to hand_r bone ───────────────────────────
    [MenuItem("Demo Final/9 - Reparent Lightning+Fire to Sword Bone")]
    public static void ReparentToSword()
    {
        // Find hand_r recursively inside the character
        var character = FindInactive("AttackCharacter_GreatSword", null);
        if (character == null) { Debug.LogError("[Reparent] AttackCharacter_GreatSword not found"); return; }

        var handR = FindChildRecursive(character.transform, "hand_r");
        if (handR == null) { Debug.LogError("[Reparent] hand_r bone not found in character"); return; }

        Debug.Log($"[Reparent] Found hand_r: {GetPath(handR)}");

        // Lightning → tip of sword (forward offset in local space)
        var lightning = FindInactive("EGO_Effect_Lightning", null);
        if (lightning != null)
        {
            lightning.transform.SetParent(handR, false);
            lightning.transform.localPosition = new Vector3(0f, 0.1f, 0.8f);
            lightning.transform.localRotation = Quaternion.identity;
            lightning.transform.localScale    = Vector3.one;
            EditorUtility.SetDirty(lightning);
            Debug.Log("[Reparent] EGO_Effect_Lightning → hand_r ✓");
        }
        else Debug.LogWarning("[Reparent] EGO_Effect_Lightning not found");

        // Fire → same bone, slightly different offset
        var fire = FindInactive("EGO_Effect_Fire", null);
        if (fire != null)
        {
            fire.transform.SetParent(handR, false);
            fire.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            fire.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // cone points upward (along blade)
            fire.transform.localScale    = Vector3.one;
            EditorUtility.SetDirty(fire);
            Debug.Log("[Reparent] EGO_Effect_Fire → hand_r ✓");
        }
        else Debug.LogWarning("[Reparent] EGO_Effect_Fire not found");

        // Portal stays in world space — move it 2m behind character's start position
        var portal = FindInactive("EGO_Effect_Portal", null);
        if (portal != null)
        {
            portal.transform.SetParent(null);   // ensure world root
            portal.transform.position = new Vector3(0f, 1f, -2f);
            portal.transform.rotation = Quaternion.identity;
            portal.transform.localScale = new Vector3(1f, 1f, 0.05f); // flat disc
            EditorUtility.SetDirty(portal);
            Debug.Log("[Reparent] EGO_Effect_Portal positioned at world (0,1,-2) ✓");
        }

        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log("[Reparent] Done ✓");
    }

    // ── 10. Setup Third-Person Camera ─────────────────────────────────────────
    [MenuItem("Demo Final/10 - Setup Third Person Camera")]
    public static void SetupCamera()
    {
        var camGO = GameObject.Find("Main Camera");
        if (camGO == null) { Debug.LogError("[Camera] Main Camera not found"); return; }

        // Add or get CameraFollow script
        var follow = camGO.GetComponent<CameraFollow>();
        if (follow == null) follow = camGO.AddComponent<CameraFollow>();

        // Assign target to character
        var character = FindInactive("AttackCharacter_GreatSword", null);
        if (character != null)
        {
            follow.target             = character.transform;
            follow.distance           = 4f;
            follow.positionSmoothTime = 0.2f;
            follow.lookAtHeightOffset = 1.2f;
            follow.mouseSensitivity   = 3f;
            follow.scrollSensitivity  = 3f;
            Debug.Log("[Camera] CameraFollow assigned to Main Camera, target = AttackCharacter_GreatSword ✓");
        }
        else
        {
            Debug.LogWarning("[Camera] AttackCharacter_GreatSword not found — assign Target manually");
        }

        EditorUtility.SetDirty(camGO);
        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log("[Camera] Third-person camera setup done ✓");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    static Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    static GameObject FindInactive(string name, string parentName)
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name != name) continue;
            if (!t.gameObject.scene.isLoaded) continue;
            if (parentName != null && (t.parent == null || t.parent.name != parentName)) continue;
            return t.gameObject;
        }
        Debug.LogWarning("[Reparent] Not found: " + name);
        return null;
    }

    static string GetPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetPath(t.parent) + "/" + t.name;
    }
}
