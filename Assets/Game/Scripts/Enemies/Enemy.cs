using UnityEngine;
using System.Collections;
using DustOfWar.Combat;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Base enemy class with health and damage handling
    /// All enemy types inherit from this class
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour, IDamageable
    {
        [Header("Enemy Stats")]
        [SerializeField] protected float maxHealth = 50f;
        [SerializeField] protected float armor = 0f;
        [SerializeField] protected int scoreValue = 10;
        
        [Header("Drop Settings")]
        [SerializeField] protected bool dropResources = true;
        [SerializeField] protected GameObject[] resourceDropPrefabs;
        [SerializeField] protected GameObject rustyBoltPrefab; // Guaranteed bolt drop prefab
        [SerializeField] protected int minDropCount = 1;
        [SerializeField] protected int maxDropCount = 3;
        [SerializeField] protected float dropSpreadRadius = 1f;
        [SerializeField] protected bool alwaysDropBolt = true; // Always drop at least one bolt

        [Header("Visual Feedback")]
        [SerializeField] protected bool showDamageFlash = true;
        [SerializeField] protected Color damageFlashColor = Color.red;
        [SerializeField] protected float flashDuration = 0.1f;

        protected float currentHealth;
        protected SpriteRenderer spriteRenderer;
        protected Color originalColor;
        protected float flashTimer = 0f;
        protected bool isAlive = true;

        // Events
        public System.Action<float, float> OnHealthChanged;
        public System.Action<float> OnDamageTaken;
        public System.Action<Enemy> OnEnemyDeath;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            
            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            // Ensure enemy has proper tag
            if (!gameObject.CompareTag("Enemy"))
            {
                Debug.LogWarning($"Enemy {gameObject.name} doesn't have 'Enemy' tag!");
            }
        }

        protected virtual void Update()
        {
            // Check if health dropped below zero (safety check)
            if (isAlive && currentHealth <= 0f)
            {
                Debug.LogWarning($"Enemy {gameObject.name} HP dropped below zero in Update! HP: {currentHealth}");
                currentHealth = 0f;
                Die();
                return;
            }

            // Handle damage flash
            if (showDamageFlash && flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        /// <summary>
        /// Take damage (IDamageable interface)
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (!isAlive)
            {
                Debug.Log($"Enemy {gameObject.name} is already dead, ignoring damage");
                return;
            }

            Debug.Log($"Enemy {gameObject.name} taking {damage} damage");

            // Apply armor reduction
            float actualDamage = Mathf.Max(1f, damage - armor);
            
            currentHealth -= actualDamage;

            OnDamageTaken?.Invoke(actualDamage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Visual feedback
            if (showDamageFlash && spriteRenderer != null)
            {
                spriteRenderer.color = damageFlashColor;
                flashTimer = flashDuration;
            }

            // Check if enemy is dead (HP <= 0)
            if (currentHealth <= 0f)
            {
                currentHealth = 0f; // Ensure it's exactly 0
                
                // Notify stats manager
                if (DustOfWar.Gameplay.GameStatsManager.Instance != null)
                {
                    DustOfWar.Gameplay.GameStatsManager.Instance.RecordEnemyKill();
                }

                Die();
            }
        }

        /// <summary>
        /// Kill the enemy immediately
        /// </summary>
        public virtual void Die()
        {
            if (!isAlive)
            {
                Debug.LogWarning($"Enemy {gameObject.name} Die() called but already dead!");
                return;
            }

            Debug.Log($"Enemy {gameObject.name} is dying (HP: {currentHealth})");
            isAlive = false;
            
            // Drop resources first (before disabling anything)
            if (dropResources)
            {
                DropResources();
            }

            // Notify listeners
            OnEnemyDeath?.Invoke(this);
            
            // Use coroutine to disable components with delay to allow projectiles to process hits
            StartCoroutine(DisableAndDestroy());
        }

        protected virtual void DropResources()
        {
            // Always drop at least one bolt if enabled
            if (alwaysDropBolt && rustyBoltPrefab != null)
            {
                Vector2 randomOffset = Random.insideUnitCircle * dropSpreadRadius;
                Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
                Instantiate(rustyBoltPrefab, dropPosition, Quaternion.identity);
            }

            // Drop additional random resources
            if (resourceDropPrefabs != null && resourceDropPrefabs.Length > 0)
            {
                int dropCount = Random.Range(minDropCount, maxDropCount + 1);

                for (int i = 0; i < dropCount; i++)
                {
                    GameObject resourcePrefab = resourceDropPrefabs[Random.Range(0, resourceDropPrefabs.Length)];
                    if (resourcePrefab == null) continue;

                    Vector2 randomOffset = Random.insideUnitCircle * dropSpreadRadius;
                    Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                    Instantiate(resourcePrefab, dropPosition, Quaternion.identity);
                }
            }
        }

        /// <summary>
        /// Set maximum health (for difficulty scaling)
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 1f;
            maxHealth = Mathf.Max(1f, newMaxHealth);
            currentHealth = maxHealth * healthPercentage;
            
            // Ensure health doesn't go below 0
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                if (isAlive)
                {
                    Die();
                }
            }
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Set armor value
        /// </summary>
        public void SetArmor(float newArmor)
        {
            armor = Mathf.Max(0f, newArmor);
        }

        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public float GetHealthPercentage() => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        public bool IsAlive() => isAlive;
        public int GetScoreValue() => scoreValue;
        
        /// <summary>
        /// Coroutine to disable components and destroy enemy with delay
        /// </summary>
        private IEnumerator DisableAndDestroy()
        {
            // Wait a frame to allow projectiles to process hits
            yield return null;
            
            // Disable visual components
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            
            // Disable colliders (but keep them active for one more frame for collision processing)
            Collider2D[] colliders = GetComponents<Collider2D>();
            
            // Disable Rigidbody2D
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
            }
            
            // Wait one more frame for collision callbacks to complete
            yield return null;
            
            // Now disable colliders
            foreach (var collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
            
            // Hide gameObject
            gameObject.SetActive(false);
            
            // Destroy enemy after short delay
            Destroy(gameObject, 0.05f);
        }
    }
}

