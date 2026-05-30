using UnityEngine;
using UnityEditor;

public static class DemoTweakParticles
{
    [MenuItem("Demo Final/15 - Tweak Particle Effects")]
    public static void TweakAll()
    {
        TweakLightning();
        TweakFire();
        TweakHoly();
        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log("[Tweak] All particle effects tweaked ✓");
    }

    // ── Lightning: chispas sobre la hoja, sin alejarse ───────────────────────
    static void TweakLightning()
    {
        var psGO = FindInactive("Particle System", "EGO_Effect_Lightning");
        if (psGO == null) { Debug.LogWarning("[Tweak] Lightning PS not found"); return; }

        var ps       = psGO.GetComponent<ParticleSystem>();
        var renderer = psGO.GetComponent<ParticleSystemRenderer>();

        // Main: lifetime media para que no titile tan rápido
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0f, 0.05f);    // casi sin velocidad inicial
        main.startSize     = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);

        // Emisión continua moderada — sin bursts (los bursts causan el titileo)
        var emission = ps.emission;
        emission.rateOverTime = 60;
        emission.SetBursts(new ParticleSystem.Burst[0]); // elimina bursts

        // Shape: normalOffset = 0 → spawn exactamente sobre la superficie
        var shape = ps.shape;
        shape.normalOffset = 0f;

        // Velocidad: todo cero → las partículas se quedan donde aparecen
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.Local;
        vel.x       = new ParticleSystem.MinMaxCurve(0f);
        vel.y       = new ParticleSystem.MinMaxCurve(0f);
        vel.z       = new ParticleSystem.MinMaxCurve(0f);
        vel.radial  = new ParticleSystem.MinMaxCurve(0f); // sin movimiento radial

        // Billboard simple — puntos eléctricos sobre la hoja
        renderer.renderMode      = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = 0.2f;

        EditorUtility.SetDirty(psGO);
        Debug.Log("[Tweak] Lightning: chispas sobre la hoja sin alejarse ✓");
    }

    // ── Fire: más concentradas al salir, suben libremente ─────────────────────
    static void TweakFire()
    {
        var psGO = FindInactive("EGO_Effect_Fire", null);
        if (psGO == null) { Debug.LogWarning("[Tweak] EGO_Effect_Fire not found"); return; }

        var ps       = psGO.GetComponent<ParticleSystem>();
        var renderer = psGO.GetComponent<ParticleSystemRenderer>();

        var main = ps.main;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.5f, 1.5f); // ← más lento al salir
        main.startSize       = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.gravityModifier = -1.2f; // sigue subiendo fuerte

        var emission = ps.emission;
        emission.rateOverTime = 120;

        // Shape: normalOffset pequeño = salen casi pegadas a la superficie
        var shape = ps.shape;
        shape.normalOffset = 0.005f; // ← era 0.03, ahora casi 0

        // Radial bajo + y fuerte = salen concentradas y suben
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(0f);
        vel.y       = new ParticleSystem.MinMaxCurve(1.5f); // sube rápido
        vel.z       = new ParticleSystem.MinMaxCurve(0f);
        vel.radial  = new ParticleSystem.MinMaxCurve(0.3f); // ← era 1.5, ahora 0.3

        renderer.renderMode    = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = 0.3f;

        EditorUtility.SetDirty(psGO);
        Debug.Log("[Tweak] Fire: partículas concentradas al salir ✓");
    }

    // ── Holy: puntos de luz / estrellas (no rectángulos) ─────────────────────
    static void TweakHoly()
    {
        var psGO = FindInactive("EGO_Effect_Holy", null);
        if (psGO == null) { Debug.LogWarning("[Tweak] EGO_Effect_Holy not found"); return; }

        var ps       = psGO.GetComponent<ParticleSystem>();
        var renderer = psGO.GetComponent<ParticleSystemRenderer>();

        var main = ps.main;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.02f, 0.06f); // ← muy pequeñas
        main.gravityModifier = -0.2f;

        var emission = ps.emission;
        emission.rateOverTime = 100;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 15, 25, -1, 0.25f),
        });

        // Radial bajo = se quedan cerca de la hoja
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(0f);
        vel.y       = new ParticleSystem.MinMaxCurve(0.8f);
        vel.z       = new ParticleSystem.MinMaxCurve(0f);
        vel.radial  = new ParticleSystem.MinMaxCurve(0.3f);

        // Billboard pequeño = puntos de luz. Con Bloom → estrellas doradas
        renderer.renderMode      = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = 0.15f;

        // Usar M_Holy_Particle (URP Particles/Unlit + textura estrella 4 puntas)
        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/EjercicioFinal/Demo/Materials/M_Holy_Particle.mat");
        if (mat != null)
            renderer.sharedMaterial = mat;
        else
            Debug.LogWarning("[Tweak] M_Holy_Particle.mat not found");

        // Size over lifetime: fade in rápido, fade out lento
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        var sc = new AnimationCurve();
        sc.AddKey(0f, 0f);
        sc.AddKey(0.15f, 1f);
        sc.AddKey(0.7f,  0.8f);
        sc.AddKey(1f,  0f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sc);

        EditorUtility.SetDirty(psGO);
        Debug.Log("[Tweak] Holy: puntos de luz pequeños con Bloom = estrellas ✓");
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
        Debug.LogWarning("[Tweak] Not found: " + name);
        return null;
    }
}
