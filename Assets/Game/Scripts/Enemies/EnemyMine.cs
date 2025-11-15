using UnityEngine;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Mine - Static explosive trap
    /// Activates when player approaches, explodes after delay
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyMine : MonoBehaviour
    {
        [Header("Mine Settings")]
        [SerializeField] private float activationRange = 3.5f; // Slightly larger activation range
        [SerializeField] private float explosionDelay = 2f; // Longer delay (more time to escape)
        [SerializeField] private float explosionRadius = 2.5f;
        [SerializeField] private float explosionDamage = 25f; // Reduced damage
        [SerializeField] private GameObject explosionEffect;
        [SerializeField] private GameObject warningEffect;

        [Header("Visual")]
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;

        private Enemy enemy;
        private Transform playerTarget;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private MineState currentState = MineState.Idle;
        private float activationTime = 0f;
        private GameObject warningInstance;

        private enum MineState
        {
            Idle,        // Waiting for player
            Activated,   // Player nearby, counting down
            Exploding    // About to explode
        }

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // Disable Rigidbody2D movement (static)
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
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
                case MineState.Idle:
                    CheckForActivation(distanceToPlayer);
                    break;
                case MineState.Activated:
                    CountdownToExplosion();
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

        private void CheckForActivation(float distanceToPlayer)
        {
            if (distanceToPlayer <= activationRange)
            {
                ActivateMine();
            }
        }

        private void ActivateMine()
        {
            currentState = MineState.Activated;
            activationTime = Time.time;

            // Show warning effect
            if (warningEffect != null)
            {
                warningInstance = Instantiate(warningEffect, transform.position, Quaternion.identity, transform);
            }
        }

        private void CountdownToExplosion()
        {
            float timeSinceActivation = Time.time - activationTime;
            float progress = timeSinceActivation / explosionDelay;

            // Visual feedback - pulsing
            if (spriteRenderer != null)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                spriteRenderer.color = Color.Lerp(originalColor, warningColor, progress + pulse);
            }

            // Explode when delay is over
            if (timeSinceActivation >= explosionDelay)
            {
                Explode();
            }
        }

        private void Explode()
        {
            currentState = MineState.Exploding;

            // Deal damage to player in radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    DustOfWar.Player.PlayerVehicle playerVehicle = collider.GetComponent<DustOfWar.Player.PlayerVehicle>();
                    if (playerVehicle != null)
                    {
                        // Damage decreases with distance
                        float distance = Vector2.Distance(transform.position, collider.transform.position);
                        float damageMultiplier = 1f - (distance / explosionRadius);
                        playerVehicle.TakeDamage(explosionDamage * damageMultiplier);
                    }
                }
            }

            // Spawn explosion effect
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            // Destroy mine
            if (enemy != null)
            {
                enemy.Die();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw activation range
            Gizmos.color = Color.yellow;
            DrawWireCircle(transform.position, activationRange);

            // Draw explosion radius
            Gizmos.color = Color.red;
            DrawWireCircle(transform.position, explosionRadius);
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

