using UnityEngine;
using UnityEditor;

public static class DemoAssignSwordMats
{
    [MenuItem("Demo Final/13 - Assign Sword Materials to DemoController")]
    public static void AssignMaterials()
    {
        var dcGO = GameObject.Find("DemoController");
        if (dcGO == null) { Debug.LogError("[Sword Mats] DemoController not found"); return; }

        var dc = dcGO.GetComponent<DemoController>();
        if (dc == null) { Debug.LogError("[Sword Mats] DemoController script not found"); return; }

        dc.swordMaterials = new Material[3];
        dc.swordMaterials[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/EjercicioFinal/Demo/Materials/M_VFX_Lightning.mat");
        dc.swordMaterials[1] = AssetDatabase.LoadAssetAtPath<Material>("Assets/EjercicioFinal/Demo/Materials/M_VFX_Fire.mat");
        dc.swordMaterials[2] = AssetDatabase.LoadAssetAtPath<Material>("Assets/EjercicioFinal/Demo/Materials/M_VFX_Holy.mat");

        EditorUtility.SetDirty(dcGO);
        EditorApplication.ExecuteMenuItem("File/Save Project");

        Debug.Log($"[Sword Mats] Assigned: {(dc.swordMaterials[0] ? dc.swordMaterials[0].name : "NULL")}, " +
                  $"{(dc.swordMaterials[1] ? dc.swordMaterials[1].name : "NULL")}, " +
                  $"{(dc.swordMaterials[2] ? dc.swordMaterials[2].name : "NULL")} ✓");
    }
}
