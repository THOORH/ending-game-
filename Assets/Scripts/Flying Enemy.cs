using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 20f; 
    public float updateRate = 0.1f; 
    [SerializeField] private float stoppingDistance = 1.2f; 

    private Transform player;
    private NavMeshAgent agent;
    private Animator anim;
    private float nextUpdateTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (agent != null)
        {
            // IMPORTANTE para 2D:
            agent.updateUpAxis = false;
            agent.updateRotation = false;
            
            // Configuración de movimiento fluido
            agent.stoppingDistance = stoppingDistance;
            agent.acceleration = 30f; // Aceleración alta para respuesta rápida
            agent.speed = 5f; 
            agent.autoBraking = true;
            
            // Evitamos que el agente rote el sprite automáticamente en el eje Z
            agent.angularSpeed = 0;
        }

        if (PlayerController.Instance != null) player = PlayerController.Instance.transform;
    }

    void Update()
    {
        if (player == null)
        {
            if (PlayerController.Instance != null) player = PlayerController.Instance.transform;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        
        if (distance <= detectionRange)
        {
            // Solo actualizamos la ruta cada 'updateRate' para no saturar el procesador
            if (agent.isOnNavMesh && Time.time >= nextUpdateTime)
            {
                // Buscamos el punto más cercano en el NavMesh respecto al jugador
                NavMeshHit hit;
                if (NavMesh.SamplePosition(player.position, out hit, 2.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                nextUpdateTime = Time.time + updateRate;
            }
        }
        else
        {
            // Si el jugador se aleja, el enemigo se queda quieto
            if (agent.isOnNavMesh && agent.hasPath) agent.ResetPath();
        }

        // Animación
        if (anim != null)
        {
            // Usamos velocity.sqrMagnitude por ser más eficiente que magnitude
            bool moving = agent.velocity.sqrMagnitude > 0.01f;
            anim.SetBool("isFlying", moving);
        }

        // Orientación del sprite (Flip)
        if (player.position.x > transform.position.x) 
            transform.eulerAngles = Vector3.zero;
        else 
            transform.eulerAngles = new Vector3(0, 180, 0);
    }
}