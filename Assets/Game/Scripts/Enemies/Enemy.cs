using UnityEngine;
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
        [SerializeField] protected int minDropCount = 1;
        [SerializeField] protected int maxDropCount = 3;
        [SerializeField] protected float dropSpreadRadius = 1f;

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
            if (!isAlive) return;

            // Apply armor reduction
            float actualDamage = Mathf.Max(1f, damage - armor);
            
            currentHealth -= actualDamage;
            currentHealth = Mathf.Max(0f, currentHealth);

            OnDamageTaken?.Invoke(actualDamage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Visual feedback
            if (showDamageFlash && spriteRenderer != null)
            {
                spriteRenderer.color = damageFlashColor;
                flashTimer = flashDuration;
            }

            // Check if enemy is dead
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Kill the enemy immediately
        /// </summary>
        public virtual void Die()
        {
            if (!isAlive) return;

            isAlive = false;
            
            // Drop resources
            if (dropResources)
            {
                DropResources();
            }

            // Notify listeners
            OnEnemyDeath?.Invoke(this);

            // Destroy enemy
            Destroy(gameObject);
        }

        protected virtual void DropResources()
        {
            if (resourceDropPrefabs == null || resourceDropPrefabs.Length == 0) return;

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

        /// <summary>
        /// Set maximum health (for difficulty scaling)
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 1f;
            maxHealth = Mathf.Max(1f, newMaxHealth);
            currentHealth = maxHealth * healthPercentage;
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
    }
}

