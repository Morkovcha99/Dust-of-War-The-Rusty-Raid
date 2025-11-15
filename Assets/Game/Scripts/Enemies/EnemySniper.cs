using UnityEngine;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Sniper - Long-range shooter with aiming phase
    /// Stops, aims for 1-2 seconds, fires powerful shot, then retreats
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemySniper : MonoBehaviour
    {
        [Header("Sniper Settings")]
        [SerializeField] private float moveSpeed = 2.5f; // Slower movement
        [SerializeField] private float retreatSpeed = 3.5f; // Slower retreat
        [SerializeField] private float attackRange = 14f; // Slightly reduced
        [SerializeField] private float minRange = 8f;
        [SerializeField] private float aimDuration = 2f; // Longer aim time (more warning)
        [SerializeField] private float projectileSpeed = 18f; // Slightly slower projectile
        [SerializeField] private float projectileDamage = 20f; // Reduced damage
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float retreatDistance = 5f;

        [Header("Visual")]
        [SerializeField] private GameObject sniperBarrelPrefab; // Long barrel sprite
        [SerializeField] private Color aimColor = Color.red;
        [SerializeField] private LineRenderer aimLine; // Optional aim indicator

        private Enemy enemy;
        private Rigidbody2D rb;
        private Transform playerTarget;
        private Vector2 currentVelocity;
        private SniperState currentState = SniperState.Moving;
        private float aimStartTime = 0f;
        private GameObject barrelInstance;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        private enum SniperState
        {
            Moving,      // Moving to position
            Aiming,      // Aiming at player
            Firing,      // Just fired
            Retreating   // Moving away
        }

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 1f;
                rb.angularDamping = 0f;
            }

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Start()
        {
            FindPlayerTarget();
        }

        private void Update()
        {
            if (enemy == null || !enemy.IsAlive()) return;

            if (playerTarget == null)
            {
                FindPlayerTarget();
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            switch (currentState)
            {
                case SniperState.Moving:
                    MoveToPosition(distanceToPlayer);
                    break;
                case SniperState.Aiming:
                    AimAtPlayer();
                    break;
                case SniperState.Firing:
                    Fire();
                    break;
                case SniperState.Retreating:
                    Retreat();
                    break;
            }
        }

        private void FindPlayerTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void MoveToPosition(float distanceToPlayer)
        {
            if (distanceToPlayer < minRange)
            {
                // Too close, retreat
                currentState = SniperState.Retreating;
                return;
            }

            if (distanceToPlayer <= attackRange && distanceToPlayer >= minRange)
            {
                // In range, start aiming
                currentState = SniperState.Aiming;
                aimStartTime = Time.time;
                currentVelocity = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
                
                // Show barrel
                ShowBarrel();
                return;
            }

            // Move towards player
            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            Vector2 targetVelocity = directionToPlayer * moveSpeed;
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, 3f * Time.deltaTime);
            rb.linearVelocity = currentVelocity;

            // Rotate towards player
            float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void AimAtPlayer()
        {
            // Stop moving
            rb.linearVelocity = Vector2.zero;

            // Rotate towards player
            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Visual feedback
            if (spriteRenderer != null)
            {
                float aimProgress = (Time.time - aimStartTime) / aimDuration;
                spriteRenderer.color = Color.Lerp(originalColor, aimColor, aimProgress);
            }

            // Draw aim line
            if (aimLine != null)
            {
                aimLine.enabled = true;
                aimLine.SetPosition(0, firePoint.position);
                aimLine.SetPosition(1, playerTarget.position);
            }

            // Check if aiming is complete
            if (Time.time - aimStartTime >= aimDuration)
            {
                currentState = SniperState.Firing;
            }
        }

        private void Fire()
        {
            if (playerTarget == null) return;

            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;

            // Fire projectile
            if (projectilePrefab != null)
            {
                GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);

                DustOfWar.Combat.Projectile projectile = projectileObj.GetComponent<DustOfWar.Combat.Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(directionToPlayer * projectileSpeed, projectileDamage, 5f);
                }
                else
                {
                    Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = directionToPlayer * projectileSpeed;
                    }
                }
            }

            // Hide barrel
            HideBarrel();

            // Reset color
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            // Hide aim line
            if (aimLine != null)
            {
                aimLine.enabled = false;
            }

            // Start retreating
            currentState = SniperState.Retreating;
        }

        private void Retreat()
        {
            if (playerTarget == null) return;

            // Move away from player
            Vector2 directionAwayFromPlayer = (transform.position - playerTarget.position).normalized;
            Vector2 targetVelocity = directionAwayFromPlayer * retreatSpeed;
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, 5f * Time.deltaTime);
            rb.linearVelocity = currentVelocity;

            // Rotate away from player
            float targetAngle = Mathf.Atan2(directionAwayFromPlayer.y, directionAwayFromPlayer.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // After retreating, go back to moving
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer >= attackRange)
            {
                currentState = SniperState.Moving;
            }
        }

        private void ShowBarrel()
        {
            if (barrelInstance != null) return;

            if (sniperBarrelPrefab != null)
            {
                barrelInstance = Instantiate(sniperBarrelPrefab, firePoint.position, firePoint.rotation, transform);
            }
        }

        private void HideBarrel()
        {
            if (barrelInstance != null)
            {
                Destroy(barrelInstance);
                barrelInstance = null;
            }
        }

        private void OnDestroy()
        {
            HideBarrel();
        }
    }
}

