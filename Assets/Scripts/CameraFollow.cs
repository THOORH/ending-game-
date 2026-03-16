using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [SerializeField] private float followSpeed = 0.1f;
    
    // Es vital que el Z del offset sea -10 para que la cámara no quede sobre el jugador
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    // Se usa LateUpdate para que la cámara se mueva DESPUÉS de que el jugador 
    // haya completado su movimiento en el Update/FixedUpdate.
    void LateUpdate()
    {
        // Verificamos que exista la instancia del jugador para evitar errores de referencia
        if (PlayerController.Instance != null)
        {
            // Calculamos la posición objetivo sumando el offset a la posición del jugador
            Vector3 targetPosition = PlayerController.Instance.transform.position + offset;
            
            // Aplicamos el movimiento suave con Lerp
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed);
        }
    }
}