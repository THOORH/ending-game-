using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar o reiniciar escenas

public class menus : MonoBehaviour

{
    public GameObject pauseMenu;
    public void StartGame()
    {
        // Restablecemos el tiempo por si acaso
        Time.timeScale = 1f;

        // Cargamos la escena del nivel
        // Puedes escribir el nombre directamente o usar la variable
        SceneManager.LoadScene("Intro");
        
        Debug.Log("Iniciando Juego: Cargando Intro");
    }
    // Función para reiniciar la partida
    public void RestartGame()
    {
        // 1. Aseguramos que el tiempo vuelva a la normalidad 
        // (Por si pausaste el juego al morir o abrir el menú)
        Time.timeScale = 1f;

        // 2. Cargamos la escena actual nuevamente
        // GetActiveScene().name obtiene el nombre del nivel en el que estás ahora
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        Debug.Log("Reiniciando nivel...");
    }

    // Función para ir al Menú Principal (opcional)
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    // Función para salir del juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        Debug.Log("Juego en pausa");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        Debug.Log("Reanudando juego");
    }
}