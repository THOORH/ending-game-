using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre exacto de la escena a la que quieres ir")]
    [SerializeField] private string sceneToLoad;

    // Esta función se activa automáticamente cuando algo entra en el Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si lo que entró es el jugador usando su Tag
        if (collision.CompareTag("Player"))
        {
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Opcional: Si tienes un sistema de música o estados, puedes guardarlos aquí
            Debug.Log("Cargando escena: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No has asignado un nombre de escena en " + gameObject.name);
        }
    }
}
