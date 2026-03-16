using UnityEngine;

public class ParallaxLayerController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject cam;
    
    [Header("Configuración Parallax")]
    public float parallaxEffect; // 0 = estático, 1 = sigue a la cámara

    private Transform[] backgrounds;
    private float length;
    private float startPosX;
    private float camStartPosX;

    void Start()
    {
        if (cam == null) cam = Camera.main.gameObject;
        
        camStartPosX = cam.transform.position.x;
        startPosX = transform.position.x;

        // Obtener a los 3 hijos
        backgrounds = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            backgrounds[i] = transform.GetChild(i);
        }

        // Medir el ancho basándose en el primer hijo
        SpriteRenderer sr = backgrounds[0].GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            length = sr.bounds.size.x;
        }
    }

    void LateUpdate()
    {
        // 1. Cálculo de movimiento Parallax
        float relativeCamDist = cam.transform.position.x - camStartPosX;
        float dist = relativeCamDist * parallaxEffect;

        // Movemos el objeto padre (y con él, todos los hijos)
        transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);

        // 2. Lógica de Reposicionamiento de Hijos (El "Salto" Infinito)
        foreach (Transform bg in backgrounds)
        {
            // Calculamos la distancia relativa entre la cámara y cada fondo individual
            float relativeDist = cam.transform.position.x - bg.position.x;

            // Si el fondo queda muy atrás a la izquierda, lo movemos a la derecha
            if (relativeDist > length * 1.5f)
            {
                bg.position += new Vector3(length * 3, 0, 0);
            }
            // Si el fondo queda muy atrás a la derecha, lo movemos a la izquierda
            else if (relativeDist < -length * 1.5f)
            {
                bg.position -= new Vector3(length * 3, 0, 0);
            }
        }
    }
}