using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static class DemoParticleSetup
{
    // ─────────────────────────────────────────────────────────────────────────
    // LIGHTNING
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Demo Final/6 - Setup Lightning Particle System")]
    public static void SetupLightning()
    {
        var go = FindInactive("Particle System", "EGO_Effect_Lightning");
        if (go == null) { Debug.LogError("[PS Setup] Particle System not found in EGO_Effect_Lightning"); return; }

        var ps = go.GetComponent<ParticleSystem>();
        if (ps == null) { Debug.LogError("[PS Setup] No ParticleSystem on Lightning child"); return; }

        // --- Main module ---
        var main = ps.main;
        main.duration           = 2f;
        main.loop               = true;
        main.startLifetime      = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startSpeed         = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize          = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor         = new ParticleSystem.MinMaxGradient(
            new Color(0.3f, 0.8f, 2.0f, 1f),   // cyan-blue
            new Color(0.6f, 0.9f, 2.0f, 1f));
        main.maxParticles       = 50;
        main.simulationSpace    = ParticleSystemSimulationSpace.World;
        main.gravityModifier    = 0f;

        // --- Emission: bursts only ---
        var emission = ps.emission;
        emission.enabled    = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 8, 12, -1, 0.5f),  // infinite bursts every 0.5s
        });

        // --- Shape: sphere (bolt appears around a point) ---
        var shape = ps.shape;
        shape.enabled       = true;
        shape.shapeType     = ParticleSystemShapeType.Sphere;
        shape.radius        = 0.05f;

        // --- Color over lifetime: bright → fade ---
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.5f, 0.9f, 2f), 0f),
                new GradientColorKey(new Color(1f, 1f, 2f), 0.3f),
                new GradientColorKey(new Color(0.3f, 0.6f, 1f), 1f),
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f),
            });
        col.color = new ParticleSystem.MinMaxGradient(gradient);

        // --- Size over lifetime: shrink ---
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.4f, 0.6f);
        sizeCurve.AddKey(1f, 0f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // --- Renderer ---
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = 0.3f;

        EditorUtility.SetDirty(go);
        Debug.Log("[PS Setup] Lightning Particle System configured ✓");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FIRE
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Demo Final/7 - Setup Fire Particle System")]
    public static void SetupFire()
    {
        var go = FindInactive("EGO_Effect_Fire", null);
        if (go == null) { Debug.LogError("[PS Setup] EGO_Effect_Fire not found"); return; }

        var ps = go.GetComponent<ParticleSystem>();
        if (ps == null) { Debug.LogError("[PS Setup] No ParticleSystem on EGO_Effect_Fire"); return; }

        // --- Main module ---
        var main = ps.main;
        main.duration           = 5f;
        main.loop               = true;
        main.startLifetime      = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
        main.startSpeed         = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startSize          = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startColor         = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.4f, 0f, 1f),
            new Color(1f, 0.7f, 0f, 1f));
        main.maxParticles       = 60;
        main.simulationSpace    = ParticleSystemSimulationSpace.World;
        main.gravityModifier    = -0.2f;   // particles rise

        // --- Emission: continuous ---
        var emission = ps.emission;
        emission.enabled      = true;
        emission.rateOverTime = 25;

        // --- Shape: cone ---
        var shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle     = 20f;
        shape.radius    = 0.15f;

        // --- Color over lifetime: orange → yellow → white → transparent ---
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.2f, 0f),   0f),
                new GradientColorKey(new Color(1f, 0.6f, 0f),   0.3f),
                new GradientColorKey(new Color(1f, 0.95f, 0.2f),0.65f),
                new GradientColorKey(new Color(1f, 1f, 0.9f),   0.9f),
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f,   1f),
            });
        col.color = new ParticleSystem.MinMaxGradient(gradient);

        // --- Size over lifetime: grow then shrink ---
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f,    0.2f);
        sizeCurve.AddKey(0.35f, 1f);
        sizeCurve.AddKey(1f,    0.1f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // --- Renderer ---
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode     = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = 0.8f;

        EditorUtility.SetDirty(go);
        Debug.Log("[PS Setup] Fire Particle System configured ✓");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper: find inactive GO by name (optionally filtered by parent name)
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject FindInactive(string name, string requiredParentName)
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name != name) continue;
            if (!t.gameObject.scene.isLoaded) continue;
            if (requiredParentName != null && (t.parent == null || t.parent.name != requiredParentName)) continue;
            return t.gameObject;
        }
        Debug.LogWarning("[PS Setup] Not found: " + name + (requiredParentName != null ? " (parent: " + requiredParentName + ")" : ""));
        return null;
    }
}
