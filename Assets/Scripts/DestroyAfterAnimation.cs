using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    [SerializeField] private float delay = 0.1f; // Tiempo que dura tu animación de slash

    void Start()
    {
        // Destruye este objeto después del tiempo asignado
        Destroy(gameObject, delay);
    }
}