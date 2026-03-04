using UnityEngine;

[ExecuteInEditMode]
public class VertexAbsorptionController : MonoBehaviour
{
    [SerializeField] private Material target;
    [SerializeField] private float radius;

    void Update()
    {
        target.SetVector("_Center", transform.position);
        target.SetFloat("_Radius", radius);
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    #endif
}