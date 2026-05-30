using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public static class DemoSetupEditor
{
    [MenuItem("Demo Final/1 - Setup Animator Controller")]
    public static void CreateAnimatorController()
    {
        string controllerPath = "Assets/EjercicioFinal/Demo/Scripts/AC_Demo_Character.controller";

        // Create the controller asset
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Get the root state machine
        var rootSM = controller.layers[0].stateMachine;

        // Load animation clips from FBX files
        AnimationClip idleClip   = LoadClipFromFBX("Assets/Prefabs/GreatSwordAttacks/M_Big_Sword@Idle 1.FBX", "Idle 1");
        AnimationClip attackClip = LoadClipFromFBX("Assets/Prefabs/GreatSwordAttacks/M_Big_Sword@Attack_3Combo_ALL 1.FBX", "Attack_3Combo_ALL 1");

        // Add states
        AnimatorState idleState   = rootSM.AddState("Idle");
        AnimatorState attackState = rootSM.AddState("Attack");

        if (idleClip   != null) idleState.motion   = idleClip;
        if (attackClip != null) attackState.motion  = attackClip;

        // Default state is Idle
        rootSM.defaultState = idleState;

        // Add trigger parameter for attack
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        // Idle → Attack (on trigger)
        var toAttack = idleState.AddTransition(attackState);
        toAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        toAttack.hasExitTime    = false;
        toAttack.duration       = 0.1f;

        // Attack → Idle (on exit time)
        var toIdle = attackState.AddTransition(idleState);
        toIdle.hasExitTime  = true;
        toIdle.exitTime     = 0.95f;
        toIdle.duration     = 0.1f;

        AssetDatabase.SaveAssets();
        Debug.Log("[Demo Setup] AnimatorController created at: " + controllerPath);

        // Assign to character in scene
        var character = GameObject.Find("AttackCharacter_GreatSword");
        if (character != null)
        {
            var animator = character.GetComponent<Animator>();
            if (animator == null) animator = character.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            Debug.Log("[Demo Setup] Controller assigned to AttackCharacter_GreatSword");
        }
        else
        {
            Debug.LogWarning("[Demo Setup] AttackCharacter_GreatSword not found in scene. Assign the controller manually.");
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Demo Final/2 - Setup Portal Sphere Mesh")]
    public static void SetupPortalMesh()
    {
        // Find inactive objects too using FindObjectsOfTypeAll
        GameObject portalSphere = null;
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name == "PortalSphere" && t.parent != null && t.parent.name == "EGO_Effect_Portal")
            {
                portalSphere = t.gameObject;
                break;
            }
        }

        if (portalSphere == null)
        {
            Debug.LogError("[Demo Setup] PortalSphere not found!");
            return;
        }

        var mf = portalSphere.GetComponent<MeshFilter>();
        if (mf != null)
        {
            // Assign Unity's built-in sphere mesh
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mf.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(go);
            Debug.Log("[Demo Setup] Sphere mesh assigned to PortalSphere");
        }
    }

    [MenuItem("Demo Final/3 - Create Volume Profiles")]
    public static void CreateVolumeProfiles()
    {
        string folder = "Assets/EjercicioFinal/Demo/VolumeProfiles";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        CreateProfile(folder + "/VP_Lightning.asset");
        CreateProfile(folder + "/VP_Fire.asset");
        CreateProfile(folder + "/VP_Portal.asset");

        AssetDatabase.Refresh();
        Debug.Log("[Demo Setup] Volume Profiles created in " + folder);
        Debug.Log("[Demo Setup] Manually add Bloom/Color Adjustments/Lens Distortion overrides in the Inspector.");
    }

    [MenuItem("Demo Final/4 - Assign Volume Profiles to Volume GameObjects")]
    public static void AssignVolumeProfiles()
    {
        AssignProfile("Volume_Lightning", "Assets/EjercicioFinal/Demo/VolumeProfiles/VP_Lightning.asset");
        AssignProfile("Volume_Fire",      "Assets/EjercicioFinal/Demo/VolumeProfiles/VP_Fire.asset");
        AssignProfile("Volume_Portal",    "Assets/EjercicioFinal/Demo/VolumeProfiles/VP_Portal.asset");
        Debug.Log("[Demo Setup] Volume Profiles assigned!");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    static AnimationClip LoadClipFromFBX(string fbxPath, string clipName)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var a in assets)
            if (a is AnimationClip clip && clip.name == clipName)
                return clip;

        // Fallback: return first clip found
        foreach (var a in assets)
            if (a is AnimationClip clip && !clip.name.Contains("__preview__"))
                return clip;

        Debug.LogWarning("[Demo Setup] Clip not found in: " + fbxPath);
        return null;
    }

    static void CreateProfile(string path)
    {
        if (!File.Exists(path))
        {
            var profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
        }
    }

    static void AssignProfile(string goName, string profilePath)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning("[Demo Setup] Not found: " + goName); return; }

        var vol = go.GetComponent<UnityEngine.Rendering.Volume>();
        if (vol == null) { Debug.LogWarning("[Demo Setup] No Volume on: " + goName); return; }

        var profile = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(profilePath);
        if (profile == null) { Debug.LogWarning("[Demo Setup] Profile not found: " + profilePath); return; }

        vol.sharedProfile = profile;
    }
}
