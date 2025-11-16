using UnityEngine;

namespace DustOfWar.Resources
{
    /// <summary>
    /// Resource pickup that can be collected by player
    /// Automatically attracts to player when nearby
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ResourcePickup : MonoBehaviour
    {
        [Header("Resource Settings")]
        [SerializeField] private ResourceType resourceType = ResourceType.RustyBolt;
        [SerializeField] private int value = 1;

        [Header("Magnet Settings")]
        [SerializeField] private float attractionRadius = 3f;
        [SerializeField] private float attractionSpeed = 8f;
        [SerializeField] private float attractionAcceleration = 15f;
        [SerializeField] private bool useMagnet = true;

        private Rigidbody2D rb;
        private Transform playerTarget;
        private Vector2 currentVelocity;
        private bool isAttracted = false;

        public enum ResourceType
        {
            RustyBolt,      // Основная валюта
            FuelCanister,    // Редкая валюта
            HealthPickup,    // Восстановление здоровья
            UpgradePart      // Деталь для улучшения
        }

        public ResourceType Type => resourceType;
        public int Value => value;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.linearDamping = 2f;
            }
        }

        private void Start()
        {
            FindPlayerTarget();
        }

        private void Update()
        {
            if (!useMagnet) return;

            // Find player if not found
            if (playerTarget == null)
            {
                FindPlayerTarget();
                return;
            }

            // Check distance to player
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer <= attractionRadius)
            {
                isAttracted = true;
                AttractToPlayer();
            }
            else
            {
                isAttracted = false;
                // Slow down when not attracted
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 5f * Time.deltaTime);
                }
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

        private void AttractToPlayer()
        {
            if (playerTarget == null || rb == null) return;

            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            Vector2 targetVelocity = directionToPlayer * attractionSpeed;

            // Smooth acceleration towards player
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, attractionAcceleration * Time.deltaTime);
            rb.linearVelocity = currentVelocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CollectResource(other.gameObject);
            }
        }

        private void CollectResource(GameObject player)
        {
            // Notify resource manager (for session tracking)
            if (DustOfWar.Gameplay.GameStatsManager.Instance != null)
            {
                DustOfWar.Gameplay.GameStatsManager.Instance.CollectResource(resourceType, value);
            }

            // Save bolts immediately to permanent storage (for cross-scene persistence)
            if (resourceType == ResourceType.RustyBolt && DustOfWar.Gameplay.SaveSystem.Instance != null)
            {
                DustOfWar.Gameplay.SaveSystem.Instance.SaveBoltOnCollection(value);
            }
            else if (resourceType == ResourceType.FuelCanister && DustOfWar.Gameplay.SaveSystem.Instance != null)
            {
                DustOfWar.Gameplay.SaveSystem.Instance.AddFuelCanisters(value);
            }

            // Destroy resource
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw attraction radius
            Gizmos.color = Color.yellow;
            DrawWireCircle(transform.position, attractionRadius);
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

