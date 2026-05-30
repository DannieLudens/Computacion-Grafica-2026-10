using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Smooth third-person camera with mouse-drag orbit.
/// - Follows target with SmoothDamp
/// - Hold LEFT mouse button + drag to orbit (yaw & pitch)
/// - Scroll wheel to zoom in/out
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Distance")]
    public float distance           = 4f;
    public float minDistance        = 1.5f;
    public float maxDistance        = 10f;

    [Header("Orbit sensitivity")]
    public float mouseSensitivity   = 3f;
    public float scrollSensitivity  = 3f;

    [Header("Pitch limits (degrees)")]
    public float minPitch = -15f;
    public float maxPitch =  60f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.15f;

    [Header("Look At height offset")]
    public float lookAtHeightOffset = 1.2f;

    // Orbit state
    float _yaw;
    float _pitch = 15f;

    // SmoothDamp
    Vector3 _velocity = Vector3.zero;

    void Start()
    {
        if (target == null) return;
        _yaw = target.eulerAngles.y;
        // Snap to position immediately
        transform.position = ComputeDesiredPosition();
        transform.LookAt(GetLookTarget());
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();

        // Smooth position follow
        Vector3 desired = ComputeDesiredPosition();
        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref _velocity, positionSmoothTime);

        // Always look at character chest
        transform.LookAt(GetLookTarget());
    }

    // ── Input (New Input System) ───────────────────────────────────────────────

    void HandleInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Left mouse button drag → orbit
        if (mouse.leftButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            // delta is in raw pixels — scale down to degrees
            float mouseX =  delta.x * 0.1f;
            float mouseY =  delta.y * 0.1f;

            _yaw   += mouseX * mouseSensitivity;
            _pitch -= mouseY * mouseSensitivity;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        // Scroll wheel → zoom (new IS gives ~120 per notch, old gave ~0.1)
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.1f)
        {
            distance -= scroll * scrollSensitivity * 0.005f;
            distance  = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    // ── Position calculation ───────────────────────────────────────────────────

    Vector3 ComputeDesiredPosition()
    {
        // Spherical to Cartesian from target + orbit angles
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
        return target.position + Vector3.up * (lookAtHeightOffset * 0.5f) + offset;
    }

    Vector3 GetLookTarget()
    {
        return target.position + Vector3.up * lookAtHeightOffset;
    }

    // ── Public API (called from DemoController) ────────────────────────────────

    /// <summary>Change the base distance and optionally reset yaw to match target.</summary>
    public void SetOffset(Vector3 newOffset)
    {
        // newOffset.z = -distance convention
        distance = Mathf.Abs(newOffset.z) > 0.1f ? Mathf.Abs(newOffset.z) : distance;
        // x offset → slight yaw correction
        if (Mathf.Abs(newOffset.x) > 0.1f)
            _yaw = (target != null ? target.eulerAngles.y : 0f) + Mathf.Atan2(newOffset.x, -newOffset.z) * Mathf.Rad2Deg;
    }

    /// <summary>Snap yaw to face the target's forward direction.</summary>
    public void ResetYaw()
    {
        if (target != null) _yaw = target.eulerAngles.y + 180f;
    }
}
