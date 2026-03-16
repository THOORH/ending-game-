using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sirviente : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected float Health = 10;
    [SerializeField] protected float recoilLength = 0.2f;
    [SerializeField] protected float recoilFactor = 0.1f; 
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected float speed;
    [SerializeField] protected float damage = 1; 

    protected float recoilTimer;
    protected Rigidbody2D rb;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void Update()
    {
        if (Health <= 0) { Destroy(gameObject); }

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength) recoilTimer += Time.deltaTime;
            else { isRecoiling = false; recoilTimer = 0; rb.linearVelocity = Vector2.zero; }
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        Health -= _damageDone;
        if (!isRecoiling)
        {
            isRecoiling = true;
            rb.linearVelocity = -_hitDirection * _hitForce * recoilFactor;
        }
    }

    // DETECCIÓN FÍSICA (Si ambos colliders son sólidos)
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Colisión detectada con: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            Attack();
        }

        if (isRecoiling) { isRecoiling = false; recoilTimer = 0; rb.linearVelocity = Vector2.zero; }
    }

    // DETECCIÓN TRIGGER (Si uno de los colliders tiene 'Is Trigger' activado)
    protected virtual void OnTriggerEnter2D(Collider2D _other)
    {
        Debug.Log("Trigger detectado con: " + _other.gameObject.name);
        if (_other.CompareTag("Player"))
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        if (PlayerController.Instance != null)
        {
            // Pasamos daño y posición para el empuje
            PlayerController.Instance.TakeDamage(damage, transform.position);
        }
    }
}