using UnityEngine;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Pack Hound - Orbital enemy that circles around player and occasionally attacks
    /// Low health, orbits player at distance, rare attacks
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyPackHound : MonoBehaviour
    {
        [Header("Orbital Settings")]
        [SerializeField] private float orbitRadius = 4f; // Distance from player
        [SerializeField] private float orbitSpeed = 90f; // Degrees per second
        [SerializeField] private float approachSpeed = 3f; // Speed to reach orbit radius
        [SerializeField] private float rotationSpeed = 300f; // Sprite rotation speed

        [Header("Attack Settings")]
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackCooldown = 3f; // Time between attacks
        [SerializeField] private float attackRange = 2.5f; // Range for attack
        [SerializeField] private float attackChargeSpeed = 10f; // Speed when charging attack
        [SerializeField] private float attackDuration = 0.3f; // How long attack lasts

        [Header("Visual")]
        [SerializeField] private Color packHoundColor = new Color(1f, 0.7f, 0.7f); // Light red tint
        [SerializeField] private float sizeMultiplier = 0.9f;

        private Enemy enemy;
        private Rigidbody2D rb;
        private Transform playerTarget;
        private float orbitAngle = 0f; // Current angle in orbit
        private float lastAttackTime = 0f;
        private bool isAttacking = false;
        private float attackStartTime = 0f;
        private Vector2 attackTargetPosition;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            rb = GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 2f; // Some damping for smoother movement
                rb.angularDamping = 0f;
            }

            // Apply visual changes
            ApplyVisualChanges();
        }

        private void Start()
        {
            FindPlayerTarget();
            
            // Randomize starting orbit angle for variety
            orbitAngle = Random.Range(0f, 360f);
            
            // Scale down slightly
            transform.localScale = Vector3.one * sizeMultiplier;
        }

        private void Update()
        {
            if (enemy == null || !enemy.IsAlive()) return;

            if (playerTarget == null)
            {
                FindPlayerTarget();
                return;
            }

            // Handle attack cooldown and timing
            if (isAttacking)
            {
                HandleAttack();
            }
            else
            {
                OrbitAroundPlayer();
                CheckForAttack();
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

        private void OrbitAroundPlayer()
        {
            if (playerTarget == null || rb == null) return;

            Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
            float distanceToPlayer = toPlayer.magnitude;

            // Update orbit angle
            orbitAngle += orbitSpeed * Time.deltaTime;
            if (orbitAngle >= 360f) orbitAngle -= 360f;

            // Calculate desired orbit position
            float angleRad = orbitAngle * Mathf.Deg2Rad;
            Vector2 desiredPosition = (Vector2)playerTarget.position + new Vector2(
                Mathf.Cos(angleRad) * orbitRadius,
                Mathf.Sin(angleRad) * orbitRadius
            );

            // Move towards orbit position
            Vector2 directionToOrbit = (desiredPosition - (Vector2)transform.position).normalized;
            float distanceToOrbit = Vector2.Distance(transform.position, desiredPosition);

            // If too far from orbit, approach faster
            if (distanceToOrbit > 0.5f)
            {
                Vector2 targetVelocity = directionToOrbit * approachSpeed;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, 5f * Time.deltaTime);
            }
            else
            {
                // Maintain orbit position with orbital velocity
                Vector2 tangentDirection = new Vector2(-Mathf.Sin(angleRad), Mathf.Cos(angleRad));
                float orbitalSpeed = orbitSpeed * Mathf.Deg2Rad * orbitRadius;
                rb.linearVelocity = tangentDirection * orbitalSpeed;
            }

            // Rotate sprite to face movement direction
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = transform.eulerAngles.z;
                float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void CheckForAttack()
        {
            if (playerTarget == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            float timeSinceLastAttack = Time.time - lastAttackTime;

            // Attack if close enough and cooldown is ready
            if (distanceToPlayer <= attackRange && timeSinceLastAttack >= attackCooldown)
            {
                StartAttack();
            }
        }

        private void StartAttack()
        {
            isAttacking = true;
            attackStartTime = Time.time;
            attackTargetPosition = playerTarget.position;
            lastAttackTime = Time.time;
        }

        private void HandleAttack()
        {
            if (playerTarget == null || rb == null) return;

            float attackProgress = (Time.time - attackStartTime) / attackDuration;

            if (attackProgress >= 1f)
            {
                // Attack finished
                isAttacking = false;
                return;
            }

            // Charge towards player during attack
            Vector2 directionToPlayer = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            Vector2 attackVelocity = directionToPlayer * attackChargeSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, attackVelocity, 10f * Time.deltaTime);

            // Rotate towards player during attack
            float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void ApplyVisualChanges()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = packHoundColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Don't process collisions if dead
            if (enemy == null || !enemy.IsAlive()) return;

            // Only deal damage during attack
            if (!isAttacking) return;

            // Check if hit player
            if (other.CompareTag("Player"))
            {
                DustOfWar.Player.PlayerVehicle playerVehicle = other.GetComponent<DustOfWar.Player.PlayerVehicle>();
                if (playerVehicle != null)
                {
                    playerVehicle.TakeDamage(attackDamage);
                    // End attack after hitting player
                    isAttacking = false;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw orbit radius
            if (playerTarget != null)
            {
                Gizmos.color = Color.cyan;
                DrawWireCircle(playerTarget.position, orbitRadius);
            }

            // Draw attack range
            Gizmos.color = Color.red;
            DrawWireCircle(transform.position, attackRange);
        }

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

