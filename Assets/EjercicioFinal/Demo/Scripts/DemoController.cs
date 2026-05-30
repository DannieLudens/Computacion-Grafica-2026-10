using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class DemoController : MonoBehaviour
{
    [Header("Effects (assign in Inspector)")]
    public GameObject[] effects = new GameObject[3];

    [Header("Post-Processing Volumes")]
    public Volume[] effectVolumes = new Volume[3];

    [Header("Character")]
    public Animator characterAnimator;

    [Header("UI")]
    public Button[] effectButtons = new Button[3];
    public Button   playPauseButton;
    public Text     effectNameLabel;
    public Text     playPauseLabel;

    [Header("Effect Names")]
    public string[] effectNames = { "⚡  Rayo", "🔥  Fuego", "✨  Sagrado" };

    [Header("Animations")]
    public string attackAnimTrigger = "Attack";
    public string idleAnimState    = "Idle";

    [Header("Impact Post-Processing (flash al espadazo)")]
    public Volume[] impactVolumes = new Volume[3];
    [Tooltip("Tiempos en segundos de cada golpe del combo (desde que se presiona el botón)")]
    public float[] hitTimes = { 2.37f, 3.33f, 4.50f };
    [Tooltip("Tiempo en subir el weight de 0 a 1 (segundos) — más corto = flash más seco")]
    public float flashRiseTime = 0.05f;
    [Tooltip("Tiempo en bajar el weight de 1 a 0 (segundos) — más largo = tail visible")]
    public float flashFallTime = 0.12f;

    [Header("Camera")]
    public CameraFollow cameraFollow;
    public Vector3[] cameraOffsets = {
        new Vector3(0f, 1.8f, -4f),
        new Vector3(0f, 1.8f, -4f),
        new Vector3(0f, 1.8f, -4f),
    };

    [Header("Sword Material Swap")]
    // Materials to apply to the sword for each effect (assign in Inspector)
    public Material[] swordMaterials = new Material[3];

    // Runtime
    int      _activeIndex       = -1;
    bool     _isPlaying         = false;
    bool     _wasAttacking      = false;
    Renderer _swordRenderer;
    Material _originalSwordMaterial;

    void Start()
    {
        // Auto-find sword MeshRenderer by name
        _swordRenderer = FindRendererByName("Modeling_Weapon_Big_Sword");
        if (_swordRenderer != null)
            _originalSwordMaterial = _swordRenderer.sharedMaterial;

        for (int i = 0; i < effectButtons.Length; i++)
        {
            int idx = i;
            effectButtons[i].onClick.AddListener(() => SelectEffect(idx));
        }
        playPauseButton.onClick.AddListener(TogglePlayPause);

        SetAllEffectsActive(false);
        SetAllVolumesActive(false);
        UpdateUI();
    }

    Renderer FindRendererByName(string n)
    {
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (r.name == n) return r;
        return null;
    }

    [Header("Lightning Volume Flicker")]
    [Tooltip("Velocidad del parpadeo eléctrico (cycles/sec). ~4 = rápido pero no excesivo.")]
    public float lightningFlickerSpeed = 4f;

    void Update()
    {
        if (_activeIndex < 0 || !_isPlaying || characterAnimator == null) return;

        // Parpadeo eléctrico en ImpactVolume_Lightning (Chromatic Aberration oscilante)
        if (_activeIndex == 0 && impactVolumes != null && impactVolumes.Length > 0 && impactVolumes[0] != null)
        {
            float t = (Mathf.Sin(Time.time * lightningFlickerSpeed * Mathf.PI * 2f) + 1f) / 2f;
            impactVolumes[0].weight = Mathf.Lerp(0.4f, 1f, t);
        }
        else if (impactVolumes != null && impactVolumes.Length > 0 && impactVolumes[0] != null)
        {
            // Asegura que vuelva a 0 cuando cambia de espada
            impactVolumes[0].weight = 0f;
        }

        bool attacking = characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        if (attacking && !_wasAttacking)
            StartCoroutine(FlashImpactEffect(_activeIndex));
        _wasAttacking = attacking;
    }

    System.Collections.IEnumerator FlashImpactEffect(int index)
    {
        if (impactVolumes == null || index >= impactVolumes.Length) yield break;
        if (impactVolumes[index] == null) yield break;

        // Lanzar los 3 flashes en paralelo — cada uno espera su propio delay
        for (int hit = 0; hit < hitTimes.Length; hit++)
            StartCoroutine(SingleFlash(index, hitTimes[hit]));
    }

    System.Collections.IEnumerator SingleFlash(int index, float delay)
    {
        yield return new WaitForSeconds(delay);

        var vol = impactVolumes[index];
        if (vol == null) yield break;

        // Subida instantánea
        vol.weight = 1f;

        // Caída simple — sin counter, sin bloqueos
        for (float t = flashFallTime; t > 0f; t -= Time.deltaTime)
        {
            if (vol != null) vol.weight = t / flashFallTime;
            yield return null;
        }

        if (vol != null) vol.weight = 0f;
    }

    public void SelectEffect(int index)
    {
        if (index < 0 || index >= effects.Length) return;

        if (_activeIndex == index && _isPlaying)
        {
            TogglePlayPause();
            return;
        }

        if (_activeIndex >= 0)
        {
            SetEffectActive(_activeIndex, false);
            SetVolumeActive(_activeIndex, false);
        }

        _activeIndex = index;
        _isPlaying   = true;

        SetEffectActive(_activeIndex, true);
        SetVolumeActive(_activeIndex, true);
        SwapSwordMaterial(_activeIndex);
        TriggerCharacterAnim(_activeIndex);
        UpdateCameraOffset(_activeIndex);
        UpdateUI();
    }

    public void TogglePlayPause()
    {
        if (_activeIndex < 0) return;

        _isPlaying = !_isPlaying;

        if (characterAnimator != null)
            characterAnimator.speed = _isPlaying ? 1f : 0f;

        if (effects[_activeIndex] != null)
        {
            var particles = effects[_activeIndex].GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                if (_isPlaying) ps.Play();
                else            ps.Pause();
            }
        }

        UpdateUI();
    }

    void TriggerCharacterAnim(int index)
    {
        if (characterAnimator == null) return;
        characterAnimator.speed = 1f;
        characterAnimator.SetTrigger(attackAnimTrigger);
    }

    void SwapSwordMaterial(int index)
    {
        if (_swordRenderer == null) return;
        if (index >= 0 && index < swordMaterials.Length && swordMaterials[index] != null)
            _swordRenderer.material = swordMaterials[index];
        else if (_originalSwordMaterial != null)
            _swordRenderer.material = _originalSwordMaterial;
    }

    void UpdateCameraOffset(int index)
    {
        if (cameraFollow == null) return;
        if (index >= 0 && index < cameraOffsets.Length)
            cameraFollow.SetOffset(cameraOffsets[index]);
    }

    void SetAllEffectsActive(bool active)
    {
        foreach (var e in effects)
            if (e != null) e.SetActive(active);
    }

    void SetEffectActive(int index, bool active)
    {
        if (effects[index] != null) effects[index].SetActive(active);
    }

    void SetAllVolumesActive(bool active)
    {
        foreach (var v in effectVolumes)
            if (v != null) v.gameObject.SetActive(active);
    }

    void SetVolumeActive(int index, bool active)
    {
        if (effectVolumes[index] != null) effectVolumes[index].gameObject.SetActive(active);
    }

    void UpdateUI()
    {
        if (effectNameLabel != null)
            effectNameLabel.text = (_activeIndex >= 0) ? effectNames[_activeIndex] : "Selecciona un efecto";

        if (playPauseLabel != null)
            playPauseLabel.text = _isPlaying ? "⏸  Pausar" : "▶  Reanudar";

        for (int i = 0; i < effectButtons.Length; i++)
        {
            var img = effectButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == _activeIndex) ? new Color(0.2f, 0.6f, 1f, 0.9f)
                                                 : new Color(0.15f, 0.15f, 0.15f, 0.8f);
        }
    }
}
