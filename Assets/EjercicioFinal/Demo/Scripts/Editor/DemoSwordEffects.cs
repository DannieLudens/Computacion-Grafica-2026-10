using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static class DemoSwordEffects
{
    // ── 11. Setup ALL 3 sword effects with MeshRenderer shape ─────────────────
    [MenuItem("Demo Final/11 - Setup Sword Effects (Blade Shape + Holy)")]
    public static void SetupAllSwordEffects()
    {
        // Find sword MeshRenderer
        var sword = FindSword();
        if (sword == null) { Debug.LogError("[Sword FX] Sword MeshRenderer not found!"); return; }
        Debug.Log($"[Sword FX] Sword found: {GetPath(sword.transform)}");

        SetupLightningOnBlade(sword);
        SetupFireOnBlade(sword);
        SetupHolyEffect(sword);
        RemovePortal();

        AssetDatabase.SaveAssets();
        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log("[Sword FX] All sword effects configured ✓");
    }

    // ── Lightning ─────────────────────────────────────────────────────────────
    static void SetupLightningOnBlade(MeshRenderer sword)
    {
        var psGO = FindInactive("Particle System", "EGO_Effect_Lightning");
        if (psGO == null) { Debug.LogWarning("[Sword FX] Lightning PS not found"); return; }

        var ps = psGO.GetComponent<ParticleSystem>();

        // Main
        var main = ps.main;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(0.5f, 2.5f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.maxParticles   = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Burst emission for lightning feel
        var emission = ps.emission;
        emission.rateOverTime = 30;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 15, 25, -1, 0.15f),
        });

        // Shape: MeshRenderer (entire blade)
        var shape = ps.shape;
        shape.enabled        = true;
        shape.shapeType      = ParticleSystemShapeType.MeshRenderer;
        shape.meshRenderer   = sword;
        shape.normalOffset   = 0.02f;

        // Color: electric blue flicker
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.5f, 0.9f, 2f),  0f),
                new GradientColorKey(new Color(1f,   1f,   2f),  0.2f),
                new GradientColorKey(new Color(0.2f, 0.5f, 1f),  1f),
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.4f),
                new GradientAlphaKey(0f, 1f),
            });
        col.color = new ParticleSystem.MinMaxGradient(g);

        EditorUtility.SetDirty(psGO);
        Debug.Log("[Sword FX] Lightning → blade shape ✓");
    }

    // ── Fire ──────────────────────────────────────────────────────────────────
    static void SetupFireOnBlade(MeshRenderer sword)
    {
        var psGO = FindInactive("EGO_Effect_Fire", null);
        if (psGO == null) { Debug.LogWarning("[Sword FX] EGO_Effect_Fire not found"); return; }

        var ps = psGO.GetComponent<ParticleSystem>();

        // Main
        var main = ps.main;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(1.0f, 2.5f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
        main.gravityModifier = -0.4f;
        main.maxParticles   = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Shape: MeshRenderer (entire blade)
        var shape = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.MeshRenderer;
        shape.meshRenderer = sword;
        shape.normalOffset = 0.03f;

        // Emission continuous
        var emission = ps.emission;
        emission.rateOverTime = 60;

        // Color: fire gradient
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.15f, 0f),   0f),
                new GradientColorKey(new Color(1f, 0.6f,  0f),   0.35f),
                new GradientColorKey(new Color(1f, 0.95f, 0.2f), 0.7f),
                new GradientColorKey(new Color(1f, 1f,    0.9f), 0.9f),
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f,   0f),
                new GradientAlphaKey(0.9f, 0.4f),
                new GradientAlphaKey(0f,   1f),
            });
        col.color = new ParticleSystem.MinMaxGradient(g);

        // Size: grow then shrink
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        var sc = new AnimationCurve();
        sc.AddKey(0f, 0.3f); sc.AddKey(0.4f, 1f); sc.AddKey(1f, 0.05f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sc);

        EditorUtility.SetDirty(psGO);
        Debug.Log("[Sword FX] Fire → blade shape ✓");
    }

    // ── Holy ──────────────────────────────────────────────────────────────────
    static void SetupHolyEffect(MeshRenderer sword)
    {
        // Rename Portal → Holy (or create fresh if missing)
        var portalGO = FindInactive("EGO_Effect_Portal", null);
        GameObject holyGO;

        if (portalGO != null)
        {
            // Re-use the empty parent, just rename it
            portalGO.name = "EGO_Effect_Holy";
            holyGO = portalGO;

            // Destroy PortalSphere child (no longer needed)
            var sphere = holyGO.transform.Find("PortalSphere");
            if (sphere != null) Object.DestroyImmediate(sphere.gameObject);
        }
        else
        {
            holyGO = new GameObject("EGO_Effect_Holy");
        }

        // Parent to hand_r like the others
        var handR = FindChildRecursive(
            FindInactive("AttackCharacter_GreatSword", null).transform, "hand_r");
        if (handR != null)
        {
            holyGO.transform.SetParent(handR, false);
            holyGO.transform.localPosition = Vector3.zero;
            holyGO.transform.localRotation = Quaternion.identity;
            holyGO.transform.localScale    = Vector3.one;
        }

        // Add ParticleSystem if not present
        var ps = holyGO.GetComponent<ParticleSystem>();
        if (ps == null) ps = holyGO.AddComponent<ParticleSystem>();

        // Main
        var main = ps.main;
        main.duration           = 5f;
        main.loop               = true;
        main.startLifetime      = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed         = new ParticleSystem.MinMaxCurve(0.8f, 2.0f);
        main.startSize          = new ParticleSystem.MinMaxCurve(0.04f, 0.14f);
        main.startColor         = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.98f, 0.6f, 1f),
            new Color(1f, 1f,    1f,   1f));
        main.gravityModifier    = -0.15f;
        main.maxParticles       = 120;
        main.simulationSpace    = ParticleSystemSimulationSpace.World;

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = 50;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 20, 30, -1, 0.5f),
        });

        // Shape: MeshRenderer (entire blade)
        var shape = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.MeshRenderer;
        shape.meshRenderer = sword;
        shape.normalOffset = 0.04f;

        // Color over lifetime: gold → white → fade
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.95f, 0.4f), 0f),
                new GradientColorKey(new Color(1f, 1f,    0.8f), 0.4f),
                new GradientColorKey(new Color(1f, 1f,    1f),   0.8f),
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f,   0f),
                new GradientAlphaKey(0.9f, 0.5f),
                new GradientAlphaKey(0f,   1f),
            });
        col.color = new ParticleSystem.MinMaxGradient(g);

        // Size over lifetime
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        var sc = new AnimationCurve();
        sc.AddKey(0f, 0.5f); sc.AddKey(0.3f, 1f); sc.AddKey(1f, 0f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sc);

        // Create and assign material
        string matPath = "Assets/EjercicioFinal/Demo/Materials/M_VFX_Holy.mat";
        if (!System.IO.File.Exists(matPath))
        {
            var mat = new Material(Shader.Find("VFX/Holy"));
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();
        }
        var renderer = holyGO.GetComponent<ParticleSystemRenderer>();
        if (renderer == null) renderer = holyGO.AddComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        holyGO.SetActive(false);
        EditorUtility.SetDirty(holyGO);
        Debug.Log("[Sword FX] Holy effect created on blade ✓");
    }

    // ── Remove Portal ─────────────────────────────────────────────────────────
    static void RemovePortal()
    {
        // Volume_Portal → rename to Volume_Holy
        var vp = FindInactive("Volume_Portal", null);
        if (vp != null)
        {
            vp.name = "Volume_Holy";
            EditorUtility.SetDirty(vp);

            // Swap profile
            string newProfilePath = "Assets/EjercicioFinal/Demo/VolumeProfiles/VP_Holy.asset";
            var oldProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                "Assets/EjercicioFinal/Demo/VolumeProfiles/VP_Portal.asset");

            if (!System.IO.File.Exists(newProfilePath))
            {
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, newProfilePath);
                AssetDatabase.SaveAssets();
            }
            var vol = vp.GetComponent<Volume>();
            if (vol != null)
                vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(newProfilePath);

            Debug.Log("[Sword FX] Volume_Portal → Volume_Holy ✓");
        }

        AssetDatabase.Refresh();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static MeshRenderer FindSword()
    {
        foreach (var mr in Resources.FindObjectsOfTypeAll<MeshRenderer>())
        {
            if (!mr.gameObject.scene.isLoaded) continue;
            string n = mr.gameObject.name;
            if (n.Contains("Big_Sword") || n.Contains("Sword") || n.Contains("Weapon") || n.Contains("sword"))
                return mr;
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
        return null;
    }

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

    static string GetPath(Transform t) =>
        t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
}
