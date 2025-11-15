using UnityEngine;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Bulldozer - Slow, relentless pressure enemy
    /// Moves straight at player, no shooting, pushes player to walls
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBulldozer : MonoBehaviour
    {
        [Header("Bulldozer Settings")]
        [SerializeField] private float moveSpeed = 1.2f; // Even slower
        [SerializeField] private float acceleration = 1.5f; // Slower acceleration
        [SerializeField] private float rotationSpeed = 60f; // Much slower rotation
        [SerializeField] private float pushDamage = 3f; // Reduced damage
        [SerializeField] private float pushForce = 1.5f; // Less push force
        [SerializeField] private float damageInterval = 0.5f; // Damage every 0.5 seconds instead of every frame
        private float lastDamageTime = 0f;

        [Header("Visual")]
        [SerializeField] private Color bulldozerColor = new Color(0.3f, 0.3f, 0.3f); // Darker
        [SerializeField] private float sizeMultiplier = 1.2f;

        private Enemy enemy;
        private Rigidbody2D rb;
        private Transform playerTarget;
        private Vector2 currentVelocity;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            rb = GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 0.5f; // Some damping for steady movement
                rb.angularDamping = 0f;
            }

            ApplyVisualChanges();
        }

        private void Start()
        {
            FindPlayerTarget();
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

            PushTowardsPlayer();
        }

        private void FindPlayerTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void PushTowardsPlayer()
        {
            if (playerTarget == null) return;

            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            
            // Move straight at player with steady speed
            Vector2 targetVelocity = directionToPlayer * moveSpeed;
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
            rb.linearVelocity = currentVelocity;

            // Rotate towards player slowly
            float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void ApplyVisualChanges()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = bulldozerColor;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                DustOfWar.Player.PlayerVehicle playerVehicle = collision.gameObject.GetComponent<DustOfWar.Player.PlayerVehicle>();
                if (playerVehicle != null)
                {
                    playerVehicle.TakeDamage(pushDamage);
                }

                // Push player away
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                    playerRb.linearVelocity += pushDirection * pushForce;
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Continuous push damage with interval
            if (collision.gameObject.CompareTag("Player"))
            {
                if (Time.time - lastDamageTime >= damageInterval)
                {
                    DustOfWar.Player.PlayerVehicle playerVehicle = collision.gameObject.GetComponent<DustOfWar.Player.PlayerVehicle>();
                    if (playerVehicle != null)
                    {
                        playerVehicle.TakeDamage(pushDamage);
                        lastDamageTime = Time.time;
                    }
                }
            }
        }
    }
}

