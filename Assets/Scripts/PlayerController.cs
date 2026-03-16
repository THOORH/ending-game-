using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;

    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundcheck;
    [SerializeField] private float groundcheckY = 0.2f;
    [SerializeField] private float groundcheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    private bool canDash = true;

    [Header("Weapon Settings")]
    public GameObject macheteObject;
    [SerializeField] private float TimeBetweenAttack = 0.4f;
    private float timeSinceAttack;

    [Header("Attack Settings")]
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage = 2f;
    [SerializeField] GameObject AtaqueEfecto;

    [Header("Recoil Settings")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 20f;
    [SerializeField] float recoilYSpeed = 20f;
    int stepsXRecoiled, stepsYRecoiled;

    [Header("Health & Damage Recoil")]
    public int health;
    public int maxHealth;
    [SerializeField] private float damageRecoilForce = 25f; 
    private bool isHurt = false; 
    private bool isDead = false;

    [Header("UI Settings")]
    [SerializeField] private GameObject gameOverCanvas; // Referencia al Canvas de GameOver
    [SerializeField] private GameObject pausebutton;

    [HideInInspector] public PlayerStatesList pState;
    private Rigidbody2D rb;
    private float xAxis, yAxis;
    private float gravity;
    Animator anim;
    private bool dashed;
    private bool attackInput;

    public static PlayerController Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
        pState = GetComponent<PlayerStatesList>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (rb != null) gravity = rb.gravityScale;
        if(macheteObject != null) macheteObject.SetActive(false);
        health = maxHealth;
        
        // Nos aseguramos de que el menú esté oculto al iniciar
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        
        // Aseguramos que el tiempo corra normalmente
        Time.timeScale = 1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(SideAttackTransform != null) Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        if(UpAttackTransform != null) Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        if(DownAttackTransform != null) Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    void Update()
    {
        if (isDead) return;

        GetInputs();
        UpdateJumpVariables();
        
        if (pState != null && !pState.Dashing && !isHurt)
        {
            Flip();
            Move();
            Jump();
        }

        StartDash();
        Attack();
        Recoil();
    }

    void GetInputs()
    {
        xAxis = 0;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) xAxis = 1;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) xAxis = -1;
        yAxis = 0;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) yAxis = 1;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) yAxis = -1;
        attackInput = Keyboard.current.eKey.wasPressedThisFrame;
    }

    void Flip()
    {
        if (xAxis < 0) { transform.eulerAngles = new Vector2(0, 180); pState.lookingRight = false; }
        else if (xAxis > 0) { transform.eulerAngles = new Vector2(0, 0); pState.lookingRight = true; }
    }
    
    private void Move() { rb.linearVelocity = new Vector2(xAxis * walkSpeed, rb.linearVelocity.y); anim.SetBool("WWalking", rb.linearVelocity.x != 0 && Grounded()); }

    void StartDash()
    {
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && canDash && !dashed) { StartCoroutine(Dash()); dashed = true; }
        if (Grounded()) dashed = false;
    }

    IEnumerator Dash()
    {
        canDash = false; pState.Dashing = true; anim.SetTrigger("Wdash");
        rb.gravityScale = 0; 
        float dir = transform.eulerAngles.y == 180 ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity; pState.Dashing = false;
        yield return new WaitForSeconds(dashCooldown); canDash = true;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attackInput && timeSinceAttack >= TimeBetweenAttack)
        {
            timeSinceAttack = 0; anim.SetTrigger("Wattack");
            if(yAxis == 0 || (yAxis < 0 && Grounded())) 
            { 
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed); 
                AtaqueEfectoAngle(AtaqueEfecto, 0, SideAttackTransform);
            }
            else if (yAxis > 0) 
            { 
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed); 
                AtaqueEfectoAngle(AtaqueEfecto, 90, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded()) 
            { 
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed); 
                AtaqueEfectoAngle(AtaqueEfecto, -90, DownAttackTransform);
            }
        }
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
        List<Sirviente> hitenemies = new List<Sirviente>();
        if(objectsToHit.Length > 0) _recoilDir = true;
        for(int i = 0; i < objectsToHit.Length; i++)
        {
            Sirviente e = objectsToHit[i].GetComponent<Sirviente>();
            if(e != null && !hitenemies.Contains(e)) { e.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength); hitenemies.Add(e); }
        }
    }

    void Recoil()
    {
        if(pState.recoilingX) rb.linearVelocity = new Vector2(pState.lookingRight ? -recoilXSpeed : recoilXSpeed, 0);
        if(pState.recoilingY) { rb.gravityScale = 0; rb.linearVelocity = new Vector2(rb.linearVelocity.x, yAxis < 0 ? recoilYSpeed : -recoilYSpeed); }
        else if (!pState.Dashing && !isHurt) rb.gravityScale = gravity;

        // Se corrigieron los warnings CS0642 eliminando los puntos y coma incorrectos
        if(pState.recoilingX && stepsXRecoiled++ < recoilXSteps)
        {
            // Sigue aplicando recoil
        }
        else 
        {
            StopRecoilX();
        }

        if(pState.recoilingY && stepsYRecoiled++ < recoilYSteps)
        {
             // Sigue aplicando recoil
        }
        else 
        {
            StopRecoilY();
        }

        if(Grounded()) StopRecoilY();
    }

    void StopRecoilX() { stepsXRecoiled = 0; pState.recoilingX = false; }
    void StopRecoilY() { stepsYRecoiled = 0; pState.recoilingY = false; }

    public void TakeDamage(float _damage, Vector2 _enemyPosition)
    {
        if (!pState.invincible && !isDead)
        {
            isHurt = true;
            health -= Mathf.RoundToInt(_damage);
            health = Mathf.Clamp(health, 0, maxHealth);

            if (health <= 0)
            {
                Die();
            }
            else
            {
                Vector2 recoilDir = (transform.position - (Vector3)_enemyPosition).normalized;
                recoilDir += Vector2.up * 0.5f; 
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(recoilDir * damageRecoilForce, ForceMode2D.Impulse);
                StartCoroutine(StopTakingDamage());
            }
        }
    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        anim.SetTrigger("Wdash");
        yield return new WaitForSeconds(0.2f);
        isHurt = false;
        yield return new WaitForSeconds(0.8f);
        pState.invincible = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0; 
        Time.timeScale = 0f;       
        Debug.Log("El jugador ha muerto");

        // Activamos el menú de GameOver
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
            if(pausebutton != null) pausebutton.SetActive(false);
        }
    }

    void AtaqueEfectoAngle(GameObject _efectoPrefab, int _EffectAngle, Transform _attackTransform)
    {
        if(_efectoPrefab == null || _attackTransform == null) return;
        GameObject efecto = Instantiate(_efectoPrefab, _attackTransform.position, Quaternion.identity);
        efecto.transform.eulerAngles = new Vector3(0, 0, _EffectAngle);
        if (transform.eulerAngles.y == 180 && _EffectAngle == 0) efecto.transform.localScale = new Vector3(-1, 1, 1);
    }

    public bool Grounded() => Physics2D.Raycast(groundcheck.position, Vector2.down, groundcheckY, whatIsGround) || Physics2D.Raycast(groundcheck.position + new Vector3(groundcheckX, 0, 0), Vector2.down, groundcheckY, whatIsGround) || Physics2D.Raycast(groundcheck.position + new Vector3(-groundcheckX, 0, 0), Vector2.down, groundcheckY, whatIsGround);

    void Jump()
    {
        if (Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); coyoteTimeCounter = 0; }
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); pState.Jumping = true; jumpBufferCounter = 0; }
        anim.SetBool("WJumping", !Grounded() && rb.linearVelocity.y > 0.1f);
        anim.SetBool("WFalling", !Grounded() && rb.linearVelocity.y < -0.1f);
    } 

    void UpdateJumpVariables()
    {
        if (Grounded()) { pState.Jumping = false; coyoteTimeCounter = coyoteTime; }
        else coyoteTimeCounter -= Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;
    }

    public void EquipMachete() { if (macheteObject != null) macheteObject.SetActive(true); }
    public void UnequipMachete() { if (macheteObject != null) macheteObject.SetActive(false); }
}