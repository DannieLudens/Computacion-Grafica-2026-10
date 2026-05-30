using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public static class DemoFixEffects
{
    [MenuItem("Demo Final/12 - Fix All Effects + PostProcessing")]
    public static void FixAll()
    {
        FixPostProcessing();
        FixLightning();
        FixFire();
        FixHoly();
        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log("[Fix Effects] All done ✓");
    }

    // ── 1. Enable Post-Processing on Main Camera ──────────────────────────────
    static void FixPostProcessing()
    {
        var camGO = GameObject.Find("Main Camera");
        if (camGO == null) { Debug.LogWarning("[Fix] Main Camera not found"); return; }

        // Enable Post Processing on the URP camera data
        var urpData = camGO.GetComponent<UniversalAdditionalCameraData>();
        if (urpData != null)
        {
            urpData.renderPostProcessing = true;
            EditorUtility.SetDirty(camGO);
            Debug.Log("[Fix] Post-Processing enabled on Main Camera ✓");
        }
        else
        {
            Debug.LogWarning("[Fix] UniversalAdditionalCameraData not found on Main Camera");
        }
    }

    // ── 2. Lightning: more visible on blade ───────────────────────────────────
    static void FixLightning()
    {
        var psGO = FindInactive("Particle System", "EGO_Effect_Lightning");
        if (psGO == null) { Debug.LogWarning("[Fix] Lightning PS not found"); return; }

        var ps       = psGO.GetComponent<ParticleSystem>();
        var renderer = psGO.GetComponent<ParticleSystemRenderer>();

        // Main: bigger, more visible particles
        var main = ps.main;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(1f, 4f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.maxParticles   = 200;

        // Much higher emission + bursts
        var emission = ps.emission;
        emission.rateOverTime = 80;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 30, 50, -1, 0.08f),
        });

        // Velocity: shoot outward from blade surface (all curves same mode = constant)
        var vel = ps.velocityOverLifetime;
        vel.enabled  = true;
        vel.space    = ParticleSystemSimulationSpace.Local;
        vel.x        = new ParticleSystem.MinMaxCurve(0f);
        vel.y        = new ParticleSystem.MinMaxCurve(0f);
        vel.z        = new ParticleSystem.MinMaxCurve(0f);
        vel.radial   = new ParticleSystem.MinMaxCurve(2f);

        // Renderer: StretchedBillboard = looks like electric arcs
        renderer.renderMode        = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale     = 0.3f;
        renderer.lengthScale       = 2f;
        renderer.maxParticleSize   = 0.5f;

        EditorUtility.SetDirty(psGO);

        // Add a blade glow child (thin stretched plane with lightning shader)
        AddBladeGlow(psGO.transform.parent, "BladeGlow_Lightning",
            new Color(0.3f, 0.7f, 2f), "VFX/Lightning");

        Debug.Log("[Fix] Lightning fixed ✓");
    }

    // ── 3. Fire: blade in flames ──────────────────────────────────────────────
    static void FixFire()
    {
        var psGO = FindInactive("EGO_Effect_Fire", null);
        if (psGO == null) { Debug.LogWarning("[Fix] EGO_Effect_Fire not found"); return; }

        var ps       = psGO.GetComponent<ParticleSystem>();
        var renderer = psGO.GetComponent<ParticleSystemRenderer>();

        // Main: fire particles rise upward, short lifetime near blade
        var main = ps.main;
        main.startLifetime      = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.startSpeed         = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        main.startSize          = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.gravityModifier    = -1.5f;   // strong upward
        main.maxParticles       = 300;
        main.startRotation      = new ParticleSystem.MinMaxCurve(0, 360f * Mathf.Deg2Rad);

        // High emission rate
        var emission = ps.emission;
        emission.rateOverTime = 150;

        // Velocity: outward + up (all same mode = constant)
        var vel = ps.velocityOverLifetime;
        vel.enabled  = true;
        vel.space    = ParticleSystemSimulationSpace.World;
        vel.x        = new ParticleSystem.MinMaxCurve(0f);
        vel.y        = new ParticleSystem.MinMaxCurve(2f);
        vel.z        = new ParticleSystem.MinMaxCurve(0f);
        vel.radial   = new ParticleSystem.MinMaxCurve(1.5f);

        // Renderer: Billboard for fire puffs
        renderer.renderMode      = ParticleSystemRenderMode.Billboard;
        renderer.maxParticleSize = 0.4f;

        EditorUtility.SetDirty(psGO);

        // Add blade glow (orange-red flame glow on blade mesh)
        AddBladeGlow(psGO.transform.parent, "BladeGlow_Fire",
            new Color(2f, 0.4f, 0f), "VFX/Fire");

        Debug.Log("[Fix] Fire fixed ✓");
    }

    // ── 4. Holy: sparkles not squares ─────────────────────────────────────────
    static void FixHoly()
    {
        var psGO = FindInactive("EGO_Effect_Holy", null);
        if (psGO == null) { Debug.LogWarning("[Fix] EGO_Effect_Holy not found"); return; }

        var ps       = psGO.GetComponent<ParticleSystem>();
        var renderer = psGO.GetComponent<ParticleSystemRenderer>();

        // Main: gentle floaty sparkles
        var main = ps.main;
        main.startLifetime      = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        main.startSpeed         = new ParticleSystem.MinMaxCurve(0.8f, 3f);
        main.startSize          = new ParticleSystem.MinMaxCurve(0.03f, 0.10f);
        main.gravityModifier    = -0.3f;
        main.maxParticles       = 200;
        main.startRotation      = new ParticleSystem.MinMaxCurve(0, 360f * Mathf.Deg2Rad);

        var emission = ps.emission;
        emission.rateOverTime = 80;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 20, 35, -1, 0.3f),
        });

        // Velocity: gentle outward float (all same mode = constant)
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(0f);
        vel.y       = new ParticleSystem.MinMaxCurve(1f);
        vel.z       = new ParticleSystem.MinMaxCurve(0f);
        vel.radial  = new ParticleSystem.MinMaxCurve(0.8f);

        // StretchedBillboard: looks like small light streaks/rays (no squares)
        renderer.renderMode      = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale   = 0.15f;
        renderer.lengthScale     = 3f;
        renderer.maxParticleSize = 0.3f;

        // Make sure material is assigned
        string matPath = "Assets/EjercicioFinal/Demo/Materials/M_VFX_Holy.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null)
            renderer.sharedMaterial = mat;
        else
            Debug.LogWarning("[Fix] M_VFX_Holy.mat not found at " + matPath);

        EditorUtility.SetDirty(psGO);

        // Add blade glow (white-gold)
        AddBladeGlow(psGO.transform.parent, "BladeGlow_Holy",
            new Color(2f, 1.8f, 0.5f), "VFX/Holy");

        Debug.Log("[Fix] Holy fixed ✓");
    }

    // ── Blade Glow Helper ─────────────────────────────────────────────────────
    /// Creates a glow overlay that uses the SAME MESH as the sword — perfect alignment.
    static void AddBladeGlow(Transform parent, string name, Color glowColor, string shaderName)
    {
        // Find the actual sword MeshRenderer
        var sword = FindSwordMR();
        if (sword == null) { Debug.LogWarning("[Fix] Sword MeshRenderer not found for BladeGlow"); return; }

        Transform glowParent = sword.transform; // parent to sword itself

        // Remove old glow if exists
        var old = glowParent.Find(name);
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // New glow GO parented to sword, identity transform
        var go = new GameObject(name);
        go.transform.SetParent(glowParent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = Vector3.one;

        // Add MeshFilter with same mesh as sword
        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = sword.GetComponent<MeshFilter>()?.sharedMesh;

        // Add MeshRenderer with glow material
        var mr = go.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows    = false;

        // Create or load material
        string matPath = $"Assets/EjercicioFinal/Demo/Materials/M_{name}.mat";
        Material mat;
        if (!System.IO.File.Exists(matPath))
        {
            var shader = Shader.Find(shaderName);
            mat = new Material(shader != null ? shader : Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }
        mr.sharedMaterial = mat;

        EditorUtility.SetDirty(go);
        Debug.Log($"[Fix] {name} → parented to sword mesh ✓");
    }

    static MeshRenderer FindSwordMR()
    {
        foreach (var mr in Resources.FindObjectsOfTypeAll<MeshRenderer>())
        {
            if (!mr.gameObject.scene.isLoaded) continue;
            string n = mr.gameObject.name;
            if (n.Contains("Big_Sword") || n.Contains("Weapon") || n.Contains("sword"))
                return mr;
        }
        return null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static GameObject FindInactive(string name, string parentName)
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name != name) continue;
            if (!t.gameObject.scene.isLoaded) continue;
            if (parentName != null && (t.parent == null || t.parent.name != parentName)) continue;
            return t.gameObject;
        }
        Debug.LogWarning("[Fix] Not found: " + name);
        return null;
    }
}
