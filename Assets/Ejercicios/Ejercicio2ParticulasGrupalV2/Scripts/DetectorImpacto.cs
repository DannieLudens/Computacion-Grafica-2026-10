using UnityEngine;

public class DetectorImpacto : MonoBehaviour
{
    public Animator animatorPersonaje;

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Proyectil"))
        {
            animatorPersonaje.SetTrigger("Hit");
        }
    }
}