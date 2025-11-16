using UnityEngine;
using DustOfWar.Combat;

namespace DustOfWar.Player
{
    /// <summary>
    /// Vehicle stats and health system
    /// Manages vehicle health, damage, and stat modifications
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerVehicle : MonoBehaviour, IDamageable
    {
        [Header("Vehicle Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float armor = 0f;
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float baseDamageMultiplier = 1f;

        [Header("Damage Settings")]
        [SerializeField] private float invulnerabilityDuration = 0.5f;
        [SerializeField] private bool showDamageIndicators = true;

        private PlayerController playerController;
        private float currentHealth;
        private float damageMultiplier = 1f;
        private float speedMultiplier = 1f;
        private float armorBonus = 0f;
        private bool isInvulnerable = false;
        private float invulnerabilityTimer = 0f;
        private float shieldHealth = 0f; // Temporary shield

        // Events
        public System.Action<float, float> OnHealthChanged; // currentHealth, maxHealth
        public System.Action<float> OnDamageTaken;
        public System.Action OnVehicleDestroyed;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            currentHealth = maxHealth;
        }

        private void Start()
        {
            // Initialize base speed
            if (playerController != null)
            {
                playerController.SetMoveSpeed(baseSpeed);
            }
        }

        private void Update()
        {
            // Handle invulnerability timer
            if (isInvulnerable)
            {
                invulnerabilityTimer -= Time.deltaTime;
                if (invulnerabilityTimer <= 0f)
                {
                    isInvulnerable = false;
                }
            }
        }

        /// <summary>
        /// Apply damage to the vehicle (IDamageable interface)
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isInvulnerable)
            {
                Debug.Log("Player is invulnerable, ignoring damage");
                return;
            }

            Debug.Log($"Player taking {damage} damage");

            // Apply armor reduction
            float totalArmor = armor + armorBonus;
            float actualDamage = Mathf.Max(1f, damage - totalArmor);
            
            // Shield absorbs damage first
            if (shieldHealth > 0f)
            {
                float shieldAbsorbed = Mathf.Min(shieldHealth, actualDamage);
                shieldHealth -= shieldAbsorbed;
                actualDamage -= shieldAbsorbed;
                
                if (shieldHealth <= 0f)
                {
                    shieldHealth = 0f;
                }
            }
            
            // Apply remaining damage to health
            if (actualDamage > 0f)
            {
                currentHealth -= actualDamage;
                currentHealth = Mathf.Max(0f, currentHealth);
            }

            OnDamageTaken?.Invoke(actualDamage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Start invulnerability period
            if (invulnerabilityDuration > 0f)
            {
                isInvulnerable = true;
                invulnerabilityTimer = invulnerabilityDuration;
            }

            // Check if vehicle is destroyed
            if (currentHealth <= 0f)
            {
                OnVehicleDestroyed?.Invoke();
            }
        }

        /// <summary>
        /// Heal the vehicle
        /// </summary>
        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(maxHealth, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Set maximum health (for upgrades)
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            float healthPercentage = currentHealth / maxHealth;
            maxHealth = Mathf.Max(1f, newMaxHealth);
            currentHealth = maxHealth * healthPercentage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Add armor bonus (from upgrades)
        /// </summary>
        public void AddArmorBonus(float bonus)
        {
            armorBonus += bonus;
        }

        /// <summary>
        /// Set damage multiplier (from upgrades)
        /// </summary>
        public void SetDamageMultiplier(float multiplier)
        {
            damageMultiplier = Mathf.Max(0.1f, multiplier);
        }

        /// <summary>
        /// Set speed multiplier (from upgrades)
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Max(0.1f, multiplier);
            if (playerController != null)
            {
                playerController.SetMoveSpeed(baseSpeed * speedMultiplier);
            }
        }

        /// <summary>
        /// Get current health
        /// </summary>
        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        /// <summary>
        /// Get maximum health
        /// </summary>
        public float GetMaxHealth()
        {
            return maxHealth;
        }

        /// <summary>
        /// Get health percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }

        /// <summary>
        /// Get damage multiplier
        /// </summary>
        public float GetDamageMultiplier()
        {
            return damageMultiplier;
        }

        /// <summary>
        /// Check if vehicle is alive
        /// </summary>
        public bool IsAlive()
        {
            return currentHealth > 0f;
        }

        /// <summary>
        /// Check if vehicle is invulnerable
        /// </summary>
        public bool IsInvulnerable()
        {
            return isInvulnerable;
        }

        /// <summary>
        /// Set shield health (for temporary shield upgrade)
        /// </summary>
        public void SetShieldHealth(float shield)
        {
            shieldHealth = Mathf.Max(0f, shield);
        }

        /// <summary>
        /// Get current shield health
        /// </summary>
        public float GetShieldHealth()
        {
            return shieldHealth;
        }

        /// <summary>
        /// Reset vehicle to full health (for restart/respawn)
        /// </summary>
        public void ResetVehicle()
        {
            currentHealth = maxHealth;
            isInvulnerable = false;
            invulnerabilityTimer = 0f;
            shieldHealth = 0f;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}

