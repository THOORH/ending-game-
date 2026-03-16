using UnityEngine;
using UnityEngine.InputSystem;

public class Jugador : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed = 5f;

    [Header("Salto de Precisión")]
    public float jumpHeight = 3f; // Altura MÁXIMA (en cuadros/metros)
    public float jumpCutMultiplier = 0.5f; // Cuánto se frena al soltar (0.5 = frena al 50%)
    public float coyoteTime = 0.1f; // Tiempo extra para saltar al caer de una plataforma
    
    private float coyoteCounter;

    [Header("Detección")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Animator animator;

    // Variable para controlar hacia dónde mira el personaje
    private bool mirandoDerecha = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. INPUT DE MOVIMIENTO
        float moveInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;

        // 2. LÓGICA DE COYOTE TIME
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // 3. SALTO (LÓGICA HOLLOW KNIGHT)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && coyoteCounter > 0)
        {
            float jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * rb.gravityScale));
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteCounter = 0;
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // 4. APLICAR MOVIMIENTO HORIZONTAL
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // 5. LÓGICA DE GIRO (FLIP)
        if (moveInput > 0 && !mirandoDerecha)
        {
            Girar();
        }
        else if (moveInput < 0 && mirandoDerecha)
        {
            Girar();
        }

        SetAnimation(moveInput);
    }

    private void Girar()
    {
        // Cambiamos el estado de la dirección
        mirandoDerecha = !mirandoDerecha;

        // Giramos el objeto 180 grados en el eje Y
        Vector3 rotacion = transform.eulerAngles;
        rotacion.y += 180;
        transform.eulerAngles = rotacion;
    }

    private void FixedUpdate()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    private void SetAnimation(float moveinput)
    {
        if (isGrounded)
        {
           if(moveinput == 0) animator.Play("Waraidle");
           else animator.Play("Wwalk");
        }
        else
        {
            if(rb.linearVelocity.y > 0) animator.Play("Jump");
            else animator.Play("Wfall");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}