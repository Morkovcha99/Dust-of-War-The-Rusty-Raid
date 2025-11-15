using UnityEngine;

namespace DustOfWar.Enemies
{
    /// <summary>
    /// Guard - Stationary shooter that moves slowly to reposition
    /// Stops and shoots, then moves to new position if player is far
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyGuard : MonoBehaviour
    {
        [Header("Guard Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float repositionDistance = 12f;
        [SerializeField] private float fireRange = 9f; // Slightly reduced range
        [SerializeField] private float fireRate = 1f; // Slower fire rate (1 shot per second)
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private float projectileSpeed = 11f; // Slightly slower projectiles
        [SerializeField] private float projectileDamage = 6f; // Reduced damage
        [SerializeField] private float rotationSpeed = 150f; // Slower rotation

        private Enemy enemy;
        private Rigidbody2D rb;
        private Transform playerTarget;
        private Vector2 currentVelocity;
        private float lastFireTime = 0f;
        private bool isShooting = false;
        private Vector3 targetPosition;
        private bool hasTargetPosition = false;

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

            if (firePoints == null || firePoints.Length == 0)
            {
                firePoints = new Transform[] { transform };
            }
        }

        private void Start()
        {
            FindPlayerTarget();
            targetPosition = transform.position;
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

            // Decide: shoot or reposition
            if (distanceToPlayer <= fireRange)
            {
                // Stop and shoot
                isShooting = true;
                currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, 5f * Time.deltaTime);
                rb.linearVelocity = currentVelocity;

                // Rotate towards player (sprite faces up by default, so -90 offset)
                Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
                float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = transform.eulerAngles.z;
                float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);

                // Fire at player
                AttemptFire();
            }
            else if (distanceToPlayer > repositionDistance)
            {
                // Move towards player slowly
                isShooting = false;
                MoveToPlayer();
            }
            else
            {
                // Stay in place
                isShooting = false;
                currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, 5f * Time.deltaTime);
                rb.linearVelocity = currentVelocity;
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

        private void MoveToPlayer()
        {
            if (playerTarget == null) return;

            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            Vector2 targetVelocity = directionToPlayer * moveSpeed;
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, 3f * Time.deltaTime);
            rb.linearVelocity = currentVelocity;

            // Rotate towards movement direction (sprite faces up by default, so -90 offset)
            if (currentVelocity.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = transform.eulerAngles.z;
                float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void AttemptFire()
        {
            if (!isShooting || projectilePrefab == null || playerTarget == null) return;

            float timeSinceLastFire = Time.time - lastFireTime;
            float fireInterval = 1f / fireRate;

            if (timeSinceLastFire >= fireInterval)
            {
                FireAtPlayer();
                lastFireTime = Time.time;
            }
        }

        private void FireAtPlayer()
        {
            if (playerTarget == null) return;

            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;

            foreach (Transform firePoint in firePoints)
            {
                FireProjectile(firePoint.position, directionToPlayer);
            }
        }

        private void FireProjectile(Vector2 position, Vector2 direction)
        {
            if (projectilePrefab == null) return;

            GameObject projectileObj = Instantiate(projectilePrefab, position, Quaternion.identity);
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            DustOfWar.Combat.Projectile projectile = projectileObj.GetComponent<DustOfWar.Combat.Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(direction * projectileSpeed, projectileDamage, 5f);
            }
            else
            {
                Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * projectileSpeed;
                }
            }
        }
    }
}

