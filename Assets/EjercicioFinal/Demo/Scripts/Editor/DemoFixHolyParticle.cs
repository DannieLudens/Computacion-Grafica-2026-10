using UnityEngine;
using UnityEditor;

public static class DemoFixHolyParticle
{
    [MenuItem("Demo Final/16 - Fix Holy Particle Material (Additive Star)")]
    public static void Fix()
    {
        string matPath = "Assets/EjercicioFinal/Demo/Materials/M_Holy_Particle.mat";
        string texPath = "Assets/Entregas/Daniel Esteban Ardila Alzate/1-ParticleSystem_101/Textures/TX_VFX_4PointStar_DEAA.png";

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) { Debug.LogError("[HolyFix] URP Particles/Unlit shader not found"); return; }

        Material mat;
        if (System.IO.File.Exists(matPath))
            mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        else
            mat = new Material(shader);

        mat.shader = shader;

        // ── Textura estrella ──────────────────────────────────────────────────
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (tex != null)
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetTexture("_MainTex", tex);
        }

        // ── Color dorado ──────────────────────────────────────────────────────
        var gold = new Color(1f, 0.92f, 0.4f, 1f);
        mat.SetColor("_BaseColor", gold);
        mat.SetColor("_Color",     gold);

        // ── Blend: Additive (fondo negro = invisible, estrella = visible) ───────
        mat.SetFloat("_Surface",  1f);   // 1 = Transparent
        mat.SetFloat("_Blend",    2f);   // 2 = Additive en URP Particles/Unlit
        mat.SetFloat("_SrcBlend", 1f);   // One
        mat.SetFloat("_DstBlend", 1f);   // One
        mat.SetFloat("_ZWrite",   0f);

        // ── Keywords ──────────────────────────────────────────────────────────
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_BLENDMODE_ADD");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        if (!System.IO.File.Exists(matPath))
            AssetDatabase.CreateAsset(mat, matPath);
        else
            EditorUtility.SetDirty(mat);

        AssetDatabase.SaveAssets();

        // ── Asignar al renderer del Holy ──────────────────────────────────────
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name != "EGO_Effect_Holy" || !t.gameObject.scene.isLoaded) continue;
            var r = t.GetComponent<ParticleSystemRenderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
                EditorUtility.SetDirty(t.gameObject);
                Debug.Log("[HolyFix] Material asignado al renderer de EGO_Effect_Holy ✓");
            }
        }

        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log($"[HolyFix] M_Holy_Particle configurado como Additive + estrella ✓  (tex: {(tex ? tex.name : "no encontrada")})");
    }
}
