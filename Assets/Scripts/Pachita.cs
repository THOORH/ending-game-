using UnityEngine;

public class Pachita : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Detection & Jump Sensors")]
    [SerializeField] private Transform frontCheck; 
    [SerializeField] private float wallDetectionDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.2f);

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        if (rb != null) 
        {
            rb.gravityScale = 3f; 
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearDamping = 1f;
        }

        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Detección de suelo
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);

        // CONTROL DE ANIMACIONES (Corregido para forzar salida de aire)
        if (anim != null)
        {
            float horizontalVel = Mathf.Abs(rb.linearVelocity.x);
            bool isMoving = horizontalVel > 0.1f;

            if (isGrounded)
            {
                // Si pisa el suelo, APAGAMOS aire inmediatamente
                anim.SetBool("Pjump", false);
                anim.SetBool("Pfall", false);
                
                // Solo después evaluamos si camina o está quieto
                anim.SetBool("Pwalk", isMoving);
            }
            else
            {
                // Si está en el aire, no puede estar caminando
                anim.SetBool("Pwalk", false);
                
                // Evaluamos si sube o cae
                anim.SetBool("Pjump", rb.linearVelocity.y > 0.1f);
                anim.SetBool("Pfall", rb.linearVelocity.y < -0.1f);
            }
        }

        // Lógica de Salto automática
        bool obstacleInFront = Physics2D.Raycast(frontCheck.position, transform.right, wallDetectionDistance, groundLayer);
        if (obstacleInFront && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        if (distanceX > stoppingDistance)
        {
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
            
            if (direction > 0) transform.eulerAngles = Vector3.zero;
            else transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void OnDrawGizmos()
    {
        if (frontCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(frontCheck.position, transform.right * wallDetectionDistance);
        }
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}