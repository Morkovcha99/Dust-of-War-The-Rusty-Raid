using UnityEngine;
using System.Collections.Generic;
using DustOfWar.Combat;

namespace DustOfWar.Player
{
    /// <summary>
    /// Automatic targeting and shooting system
    /// Finds nearest enemies and automatically fires at them
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAutoShooter : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private float fireRate = 0.5f; // Shots per second
        [SerializeField] private float weaponRange = 10f;
        [SerializeField] private float targetingAngle = 360f; // Full circle by default
        [SerializeField] private float accuracyFalloffDistance = 15f;
        [SerializeField] private float minAccuracy = 0.5f; // Minimum accuracy at max range
        
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform[] firePoints; // Multiple fire points for spread
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private float projectileLifetime = 5f;

        [Header("Visual Settings")]
        [SerializeField] private bool showTargetIndicator = true;
        [SerializeField] private Color targetIndicatorColor = Color.red;
        [SerializeField] private float indicatorSize = 0.5f;

        private PlayerController playerController;
        private PlayerVehicle playerVehicle;
        private float lastFireTime = 0f;
        private Transform currentTarget = null;
        private List<Transform> enemiesInRange = new List<Transform>();

        // Targeting layers
        private LayerMask enemyLayer;

        // Events
        public System.Action<Transform> OnTargetAcquired;
        public System.Action OnTargetLost;
        public System.Action OnWeaponFired;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerVehicle = GetComponent<PlayerVehicle>();

            // Set default fire point if none assigned
            if (firePoints == null || firePoints.Length == 0)
            {
                firePoints = new Transform[] { transform };
            }

            // Set enemy layer (default to layer 8 "Enemy" or can be configured)
            enemyLayer = LayerMask.GetMask("Enemy");
            if (enemyLayer.value == 0)
            {
                // Fallback: use tag-based detection
                enemyLayer = ~0; // All layers
            }
        }

        private void Update()
        {
            if (playerVehicle == null || !playerVehicle.IsAlive()) return;

            FindNearestTarget();
            AttemptFire();
        }

        private void FindNearestTarget()
        {
            enemiesInRange.Clear();
            currentTarget = null;

            float nearestDistance = float.MaxValue;

            // Find all enemies in range
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, weaponRange, enemyLayer);
            
            foreach (Collider2D collider in colliders)
            {
                // Check if it's actually an enemy (tag-based fallback)
                if (!collider.CompareTag("Enemy") && enemyLayer.value != ~0)
                {
                    continue;
                }

                Transform enemy = collider.transform;
                Vector2 directionToEnemy = (enemy.position - transform.position);
                float distance = directionToEnemy.magnitude;

                // Check if enemy is within targeting angle
                if (targetingAngle < 360f)
                {
                    float angle = Vector2.Angle(transform.right, directionToEnemy);
                    if (angle > targetingAngle * 0.5f)
                    {
                        continue;
                    }
                }

                enemiesInRange.Add(enemy);

                // Check if this is the nearest enemy
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    currentTarget = enemy;
                }
            }

            // Notify target changes
            if (currentTarget != null)
            {
                OnTargetAcquired?.Invoke(currentTarget);
            }
            else
            {
                OnTargetLost?.Invoke();
            }
        }

        private void AttemptFire()
        {
            if (currentTarget == null) return;

            float timeSinceLastFire = Time.time - lastFireTime;
            float fireInterval = 1f / fireRate;

            if (timeSinceLastFire >= fireInterval)
            {
                FireAtTarget();
                lastFireTime = Time.time;
            }
        }

        private void FireAtTarget()
        {
            if (currentTarget == null || projectilePrefab == null) return;

            Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;

            // Calculate accuracy based on distance
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            float accuracy = CalculateAccuracy(distance);

            // Apply accuracy spread
            float spreadAngle = (1f - accuracy) * 30f; // Max 30 degrees spread
            float randomAngle = Random.Range(-spreadAngle, spreadAngle);
            Quaternion spreadRotation = Quaternion.Euler(0, 0, randomAngle);
            Vector2 finalDirection = spreadRotation * directionToTarget;

            // Fire from all fire points
            foreach (Transform firePoint in firePoints)
            {
                FireProjectile(firePoint.position, finalDirection);
            }

            OnWeaponFired?.Invoke();
        }

        private void FireProjectile(Vector2 position, Vector2 direction)
        {
            // Instantiate projectile
            GameObject projectileObj = Instantiate(projectilePrefab, position, Quaternion.identity);
            
            // Set rotation to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Get projectile component and configure
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                float finalDamage = projectileDamage * playerVehicle.GetDamageMultiplier();
                projectile.Initialize(direction * projectileSpeed, finalDamage, projectileLifetime);
            }
            else
            {
                // Fallback: use Rigidbody2D for basic projectile
                Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * projectileSpeed;
                }
                
                // Destroy after lifetime
                Destroy(projectileObj, projectileLifetime);
            }
        }

        private float CalculateAccuracy(float distance)
        {
            if (distance <= accuracyFalloffDistance)
            {
                return 1f; // Perfect accuracy within falloff distance
            }

            // Linear falloff beyond falloff distance
            float accuracyRange = weaponRange - accuracyFalloffDistance;
            if (accuracyRange <= 0f) return 1f;

            float falloffAmount = (distance - accuracyFalloffDistance) / accuracyRange;
            float accuracy = Mathf.Lerp(1f, minAccuracy, falloffAmount);
            
            return Mathf.Clamp01(accuracy);
        }

        /// <summary>
        /// Set fire rate (shots per second)
        /// </summary>
        public void SetFireRate(float newFireRate)
        {
            fireRate = Mathf.Max(0.1f, newFireRate);
        }

        /// <summary>
        /// Set weapon range
        /// </summary>
        public void SetWeaponRange(float newRange)
        {
            weaponRange = Mathf.Max(1f, newRange);
        }

        /// <summary>
        /// Get current target
        /// </summary>
        public Transform GetCurrentTarget()
        {
            return currentTarget;
        }

        /// <summary>
        /// Get all enemies in range
        /// </summary>
        public List<Transform> GetEnemiesInRange()
        {
            return new List<Transform>(enemiesInRange);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw weapon range
            Gizmos.color = Color.yellow;
            this.DrawWireCircle(transform.position, weaponRange);

            // Draw targeting angle
            if (targetingAngle < 360f)
            {
                Gizmos.color = Color.red;
                float halfAngle = targetingAngle * 0.5f;
                Vector2 right = transform.right;
                
                Vector2 dir1 = Quaternion.Euler(0, 0, halfAngle) * right;
                Vector2 dir2 = Quaternion.Euler(0, 0, -halfAngle) * right;
                
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir1 * weaponRange);
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir2 * weaponRange);
            }

            // Draw line to current target
            if (currentTarget != null)
            {
                Gizmos.color = targetIndicatorColor;
                Gizmos.DrawLine(transform.position, currentTarget.position);
                Gizmos.DrawWireSphere(currentTarget.position, indicatorSize);
            }
        }

        /// <summary>
        /// Helper method to draw a wire circle in 2D using Gizmos
        /// </summary>
        private void DrawWireCircle(Vector3 center, float radius, int segments = 32)
        {
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                Vector3 nextPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }
}

