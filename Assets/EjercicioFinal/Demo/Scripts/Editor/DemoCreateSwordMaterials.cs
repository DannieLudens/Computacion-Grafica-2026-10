using UnityEngine;
using UnityEditor;

public static class DemoCreateSwordMaterials
{
    [MenuItem("Demo Final/14 - Create Glowing Sword Materials (Lit + Emission)")]
    public static void CreateMaterials()
    {
        // Lightning: silver blade with bright electric-blue emission
        CreateLitMat("M_Sword_Lightning",
            baseColor:  new Color(0.55f, 0.65f, 0.80f),   // cool steel-blue tint
            emitColor:  new Color(0.1f,  0.5f,  4.0f),    // HDR blue glow
            smoothness: 0.85f, metallic: 0.9f);

        // Fire: warm blade with bright orange-red emission
        CreateLitMat("M_Sword_Fire",
            baseColor:  new Color(0.6f,  0.25f, 0.05f),   // dark orange-red tint
            emitColor:  new Color(4.0f,  0.8f,  0.0f),    // HDR fire orange
            smoothness: 0.5f, metallic: 0.6f);

        // Holy: bright gold blade with golden emission
        CreateLitMat("M_Sword_Holy",
            baseColor:  new Color(0.9f,  0.85f, 0.45f),   // gold tint
            emitColor:  new Color(3.5f,  2.8f,  0.4f),    // HDR golden glow
            smoothness: 0.9f, metallic: 0.85f);

        // Re-assign to DemoController
        AssignToController();

        AssetDatabase.Refresh();
        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log("[Sword Mats] Glowing sword materials created and assigned ✓");
    }

    static void CreateLitMat(string name, Color baseColor, Color emitColor,
                              float smoothness, float metallic)
    {
        string path = $"Assets/EjercicioFinal/Demo/Materials/{name}.mat";

        // Use URP Lit shader
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat;
        if (System.IO.File.Exists(path))
            mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        else
            mat = new Material(shader);

        mat.shader = shader;

        // Base color
        if (mat.HasProperty("_BaseColor"))   mat.SetColor("_BaseColor", baseColor);
        if (mat.HasProperty("_Color"))       mat.SetColor("_Color",     baseColor);

        // Metallic / Smoothness
        if (mat.HasProperty("_Metallic"))    mat.SetFloat("_Metallic",    metallic);
        if (mat.HasProperty("_Smoothness"))  mat.SetFloat("_Smoothness",  smoothness);
        if (mat.HasProperty("_Glossiness"))  mat.SetFloat("_Glossiness",  smoothness);

        // Emission — this is what makes it glow with Bloom
        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", emitColor);

        if (!System.IO.File.Exists(path))
            AssetDatabase.CreateAsset(mat, path);
        else
            EditorUtility.SetDirty(mat);

        AssetDatabase.SaveAssets();
        Debug.Log($"[Sword Mats] {name} created ✓  (emission: {emitColor})");
    }

    static void AssignToController()
    {
        var dcGO = GameObject.Find("DemoController");
        if (dcGO == null) return;
        var dc = dcGO.GetComponent<DemoController>();
        if (dc == null) return;

        dc.swordMaterials    = new Material[3];
        dc.swordMaterials[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/EjercicioFinal/Demo/Materials/M_Sword_Lightning.mat");
        dc.swordMaterials[1] = AssetDatabase.LoadAssetAtPath<Material>("Assets/EjercicioFinal/Demo/Materials/M_Sword_Fire.mat");
        dc.swordMaterials[2] = AssetDatabase.LoadAssetAtPath<Material>("Assets/EjercicioFinal/Demo/Materials/M_Sword_Holy.mat");

        EditorUtility.SetDirty(dcGO);
    }
}
