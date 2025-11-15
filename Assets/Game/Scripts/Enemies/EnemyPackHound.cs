using UnityEngine;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Pack Hound - Fast ramming enemy that charges at player
    /// Low health, high speed, no shooting
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyPackHound : MonoBehaviour
    {
        [Header("Pack Hound Settings")]
        [SerializeField] private float chargeSpeed = 7f; // Slightly reduced
        [SerializeField] private float acceleration = 12f; // Reduced acceleration
        [SerializeField] private float rotationSpeed = 300f; // Slightly slower rotation
        [SerializeField] private float ramDamage = 10f; // Reduced damage
        [SerializeField] private float bounceForce = 2.5f;
        [SerializeField] private float bounceCooldown = 0.8f; // Longer cooldown

        [Header("Visual")]
        [SerializeField] private Color packHoundColor = new Color(1f, 0.7f, 0.7f); // Light red tint
        [SerializeField] private float sizeMultiplier = 0.9f;

        private Enemy enemy;
        private Rigidbody2D rb;
        private Transform playerTarget;
        private Vector2 currentVelocity;
        private float lastRamTime = 0f;
        private bool isBouncing = false;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            rb = GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 0f;
                rb.angularDamping = 0f;
            }

            // Apply visual changes
            ApplyVisualChanges();
        }

        private void Start()
        {
            FindPlayerTarget();
            
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

            ChargeAtPlayer();
        }

        private void FindPlayerTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void ChargeAtPlayer()
        {
            if (playerTarget == null || isBouncing) return;

            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            
            // Accelerate towards player
            Vector2 targetVelocity = directionToPlayer * chargeSpeed;
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
            rb.linearVelocity = currentVelocity;

            // Rotate towards player (sprite faces up by default, so -90 offset)
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Time.time - lastRamTime < bounceCooldown) return;

            // Check if hit player
            if (collision.gameObject.CompareTag("Player"))
            {
                DustOfWar.Player.PlayerVehicle playerVehicle = collision.gameObject.GetComponent<DustOfWar.Player.PlayerVehicle>();
                if (playerVehicle != null)
                {
                    playerVehicle.TakeDamage(ramDamage);
                }

                // Bounce away
                Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
                rb.linearVelocity = bounceDirection * bounceForce;
                
                isBouncing = true;
                lastRamTime = Time.time;

                // Stop bouncing after short time
                Invoke(nameof(StopBouncing), 0.3f);
            }
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                // Bounce off other enemies
                Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
                rb.linearVelocity = bounceDirection * bounceForce * 0.5f;
            }
        }

        private void StopBouncing()
        {
            isBouncing = false;
        }
    }
}

