using UnityEngine;
using DustOfWar.Enemies;
using DustOfWar.Gameplay;

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
        [SerializeField] private string[] targetTags = new string[] { "Player", "Enemy" }; // Tags that can be hit
        [SerializeField] private bool ignoreOwner = true; // Ignore the object that fired this projectile

        [Header("Visual Effects")]
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private GameObject trailEffect;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableHitDebug = true;
        [SerializeField] private bool showHitVisualDebug = true;
        [SerializeField] private Color hitDebugColor = Color.yellow;
        [SerializeField] private float hitDebugDuration = 0.5f;

        private Rigidbody2D rb;
        private Vector2 velocity;
        private bool isInitialized = false;
        private float spawnTime;
        private GameObject owner; // The object that fired this projectile

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
        public void Initialize(Vector2 velocity, float damage, float lifetime, GameObject owner = null)
        {
            this.velocity = velocity;
            this.damage = damage;
            this.lifetime = lifetime;
            this.owner = owner;
            
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

            // Ignore owner if set
            if (ignoreOwner && owner != null && other.gameObject == owner)
            {
                return;
            }

            // Check if hit target layer
            if (targetLayers != -1 && ((1 << other.gameObject.layer) & targetLayers) == 0)
            {
                return;
            }

            // Check if object has IDamageable component first
            IDamageable damageable = other.GetComponent<IDamageable>();
            
            // If no IDamageable, check tags
            if (damageable == null)
            {
                // Check if hit valid tag
                bool validTag = false;
                if (targetTags != null && targetTags.Length > 0)
                {
                    foreach (string tag in targetTags)
                    {
                        if (other.CompareTag(tag))
                        {
                            validTag = true;
                            break;
                        }
                    }
                }
                else
                {
                    // If no tags specified, allow all
                    validTag = true;
                }

                if (!validTag)
                {
                    return;
                }
            }
            // If object has IDamageable, allow hit regardless of tag (for breakable objects like pottery)

            // Try to deal damage to target
            if (damageable != null)
            {
                // Get target info before damage for debug
                Enemy enemy = other.GetComponent<Enemy>();
                BreakablePottery pottery = other.GetComponent<BreakablePottery>();
                float healthBefore = 0f;
                float maxHealth = 0f;
                
                if (enemy != null)
                {
                    healthBefore = enemy.GetCurrentHealth();
                    maxHealth = enemy.GetMaxHealth();
                }
                else if (pottery != null)
                {
                    healthBefore = pottery.GetHealth();
                    maxHealth = pottery.GetMaxHealth();
                }
                
                // Deal damage
                damageable.TakeDamage(damage);
                
                // Debug hit information
                if (enableHitDebug)
                {
                    string ownerName = owner != null ? owner.name : "Unknown";
                    string targetName = other.name;
                    string targetTag = other.tag;
                    Vector3 hitPosition = transform.position;
                    string targetType = enemy != null ? "Enemy" : (pottery != null ? "Pottery" : "Damageable");
                    
                    float healthAfter = 0f;
                    if (enemy != null)
                    {
                        healthAfter = enemy.GetCurrentHealth();
                    }
                    else if (pottery != null)
                    {
                        healthAfter = pottery.GetHealth();
                    }
                    
                    Debug.Log($"[PROJECTILE HIT] " +
                        $"Owner: {ownerName} → Target: {targetName} ({targetTag}, {targetType})\n" +
                        $"Position: ({hitPosition.x:F2}, {hitPosition.y:F2})\n" +
                        $"Damage: {damage}\n" +
                        $"Target Health: {healthBefore:F1}/{maxHealth:F1} → {healthAfter:F1}/{maxHealth:F1}");
                }
                
                // Visual debug indicator
                if (showHitVisualDebug)
                {
                    DrawHitDebugIndicator(transform.position);
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
            else
            {
                if (enableHitDebug)
                {
                    Debug.LogWarning($"[PROJECTILE MISS] Hit {other.name} ({other.tag}) but no IDamageable component found!");
                }
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
        
        /// <summary>
        /// Draw visual debug indicator at hit position
        /// </summary>
        private void DrawHitDebugIndicator(Vector3 position)
        {
            // Draw debug line from projectile to hit point
            if (owner != null)
            {
                Debug.DrawLine(owner.transform.position, position, hitDebugColor, hitDebugDuration);
            }
            
            // Draw debug sphere at hit point
            Debug.DrawRay(position, Vector3.up * 0.5f, hitDebugColor, hitDebugDuration);
            Debug.DrawRay(position, Vector3.down * 0.5f, hitDebugColor, hitDebugDuration);
            Debug.DrawRay(position, Vector3.left * 0.5f, hitDebugColor, hitDebugDuration);
            Debug.DrawRay(position, Vector3.right * 0.5f, hitDebugColor, hitDebugDuration);
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

