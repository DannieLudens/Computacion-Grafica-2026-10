using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static class DemoSetupImpactVolumes
{
    [MenuItem("Demo Final/17 - Setup Impact Volumes (PP al espadazo)")]
    public static void Setup()
    {
        CreateImpactVolume("ImpactVolume_Lightning", "Assets/EjercicioFinal/Demo/VolumeProfiles/VP_Impact_Lightning.asset");
        CreateImpactVolume("ImpactVolume_Fire",      "Assets/EjercicioFinal/Demo/VolumeProfiles/VP_Impact_Fire.asset");
        CreateImpactVolume("ImpactVolume_Holy",      "Assets/EjercicioFinal/Demo/VolumeProfiles/VP_Impact_Holy.asset");

        WireToController();

        AssetDatabase.SaveAssets();
        EditorApplication.ExecuteMenuItem("File/Save Project");
        Debug.Log("[Impact Volumes] Setup completo ✓");
        Debug.Log("[Impact Volumes] Recuerda agregar overrides en el Inspector:");
        Debug.Log("  VP_Impact_Lightning → Chromatic Aberration (Intensity 0→0.9)");
        Debug.Log("  VP_Impact_Fire      → Vignette (Color rojo, Intensity 0→0.5) + Color Adjustments (Temp +50)");
        Debug.Log("  VP_Impact_Holy      → Color Adjustments (Exposure 0→1.5)");
    }

    static void CreateImpactVolume(string goName, string profilePath)
    {
        // Remove old if exists
        var existing = GameObject.Find(goName);
        if (existing != null) Object.DestroyImmediate(existing);

        var go = new GameObject(goName);
        var vol = go.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 20;   // mayor que los effect volumes (10)
        vol.weight   = 0f;   // empieza invisible

        // Create profile
        VolumeProfile profile;
        if (System.IO.File.Exists(profilePath))
            profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        else
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        vol.sharedProfile = profile;

        EditorUtility.SetDirty(go);
        Debug.Log($"[Impact Volumes] {goName} creado ✓  (profile: {profilePath})");
    }

    static void WireToController()
    {
        var dcGO = GameObject.Find("DemoController");
        if (dcGO == null) { Debug.LogWarning("[Impact Volumes] DemoController not found"); return; }

        var dc = dcGO.GetComponent<DemoController>();
        if (dc == null) { Debug.LogWarning("[Impact Volumes] DemoController script not found"); return; }

        dc.impactVolumes = new Volume[3];
        dc.impactVolumes[0] = GameObject.Find("ImpactVolume_Lightning")?.GetComponent<Volume>();
        dc.impactVolumes[1] = GameObject.Find("ImpactVolume_Fire")?.GetComponent<Volume>();
        dc.impactVolumes[2] = GameObject.Find("ImpactVolume_Holy")?.GetComponent<Volume>();

        EditorUtility.SetDirty(dcGO);
        Debug.Log($"[Impact Volumes] Wired: L={dc.impactVolumes[0]?.name}, F={dc.impactVolumes[1]?.name}, H={dc.impactVolumes[2]?.name}");
    }
}
