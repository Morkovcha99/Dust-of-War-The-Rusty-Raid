using UnityEngine;
using DustOfWar.Combat;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Breakable pottery/vessel that can be destroyed by projectiles
    /// Drops resources when destroyed
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BreakablePottery : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float health = 5f;
        [SerializeField] private float maxHealth = 5f;

        [Header("Drop Settings")]
        [SerializeField] private bool dropResources = true;
        [SerializeField] private GameObject[] resourceDropPrefabs;
        [SerializeField] private int minDropCount = 0;
        [SerializeField] private int maxDropCount = 2;
        [SerializeField] private float dropSpreadRadius = 0.5f;

        [Header("Visual Effects")]
        [SerializeField] private GameObject breakEffect;
        [SerializeField] private bool showDamageFlash = true;
        [SerializeField] private Color damageFlashColor = Color.white;
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Audio")]
        [SerializeField] private AudioClip breakSound;
        [SerializeField] private float breakSoundVolume = 0.5f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private float flashTimer = 0f;
        private bool isDestroyed = false;
        private AudioSource audioSource;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }

            health = maxHealth;
        }

        private void Update()
        {
            // Handle damage flash
            if (showDamageFlash && flashTimer > 0f && spriteRenderer != null)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDestroyed) return;

            // Check for projectile hit
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null)
            {
                float damage = projectile.GetDamage();
                TakeDamage(damage);
            }
        }

        /// <summary>
        /// Take damage (IDamageable interface)
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDestroyed) return;

            health -= damage;
            health = Mathf.Max(0f, health);

            // Visual feedback
            if (showDamageFlash && spriteRenderer != null)
            {
                spriteRenderer.color = damageFlashColor;
                flashTimer = flashDuration;
            }

            // Check if destroyed
            if (health <= 0f)
            {
                Break();
            }
        }

        /// <summary>
        /// Break the pottery
        /// </summary>
        private void Break()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            // Play break sound
            if (breakSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(breakSound, breakSoundVolume);
            }

            // Spawn break effect
            if (breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }

            // Drop resources
            if (dropResources)
            {
                DropResources();
            }

            // Destroy the pottery
            Destroy(gameObject);
        }

        /// <summary>
        /// Drop resources when broken
        /// </summary>
        private void DropResources()
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
        /// Set health value
        /// </summary>
        public void SetHealth(float newHealth)
        {
            health = Mathf.Clamp(newHealth, 0f, maxHealth);
            if (health <= 0f && !isDestroyed)
            {
                Break();
            }
        }

        public float GetHealth() => health;
        public float GetMaxHealth() => maxHealth;
        public bool IsDestroyed() => isDestroyed;
    }
}




