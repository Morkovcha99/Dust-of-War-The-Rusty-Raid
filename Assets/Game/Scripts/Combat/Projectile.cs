using UnityEngine;

namespace DustOfWar.Combat
{
    /// <summary>
    /// Projectile behavior for weapons
    /// Handles movement, collision, and damage dealing
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask targetLayers = -1;
        [SerializeField] private bool destroyOnHit = true;

        [Header("Visual Effects")]
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private GameObject trailEffect;

        private Rigidbody2D rb;
        private Vector2 velocity;
        private bool isInitialized = false;
        private float spawnTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            spawnTime = Time.time;
        }

        private void Update()
        {
            // Destroy after lifetime
            if (Time.time - spawnTime >= lifetime)
            {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// Initialize projectile with velocity, damage, and lifetime
        /// </summary>
        public void Initialize(Vector2 velocity, float damage, float lifetime)
        {
            this.velocity = velocity;
            this.damage = damage;
            this.lifetime = lifetime;
            
            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
            
            // Set rotation to face movement direction
            if (velocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            
            isInitialized = true;
            spawnTime = Time.time;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInitialized) return;

            // Check if hit target layer
            if (targetLayers != -1 && ((1 << other.gameObject.layer) & targetLayers) == 0)
            {
                return;
            }

            // Try to deal damage to target
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Spawn hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // Destroy projectile
            if (destroyOnHit)
            {
                DestroyProjectile();
            }
        }

        private void DestroyProjectile()
        {
            // Spawn trail effect or cleanup
            if (trailEffect != null)
            {
                Instantiate(trailEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Set projectile damage
        /// </summary>
        public void SetDamage(float newDamage)
        {
            damage = newDamage;
        }

        /// <summary>
        /// Get projectile damage
        /// </summary>
        public float GetDamage()
        {
            return damage;
        }
    }

    /// <summary>
    /// Interface for objects that can take damage
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}

