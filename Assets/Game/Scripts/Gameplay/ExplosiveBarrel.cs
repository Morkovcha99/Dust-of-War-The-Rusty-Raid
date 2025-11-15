using UnityEngine;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Explosive barrel obstacle
    /// Explodes when hit by projectiles or player collision
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ExplosiveBarrel : MonoBehaviour
    {
        [Header("Explosion Settings")]
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private float explosionDamage = 40f;
        [SerializeField] private float explosionForce = 5f;
        [SerializeField] private GameObject explosionEffect;
        [SerializeField] private LayerMask damageLayers = -1;

        [Header("Health")]
        [SerializeField] private float health = 10f;

        private bool hasExploded = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Explode on projectile hit
            if (other.CompareTag("Projectile") || other.GetComponent<DustOfWar.Combat.Projectile>() != null)
            {
                TakeDamage(health); // Instant explode
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Explode on player collision
            if (collision.gameObject.CompareTag("Player"))
            {
                Explode();
            }
        }

        public void TakeDamage(float damage)
        {
            if (hasExploded) return;

            health -= damage;
            if (health <= 0f)
            {
                Explode();
            }
        }

        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            // Deal damage in radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageLayers);
            
            foreach (Collider2D collider in colliders)
            {
                // Damage player
                if (collider.CompareTag("Player"))
                {
                    DustOfWar.Player.PlayerVehicle playerVehicle = collider.GetComponent<DustOfWar.Player.PlayerVehicle>();
                    if (playerVehicle != null)
                    {
                        float distance = Vector2.Distance(transform.position, collider.transform.position);
                        float damageMultiplier = 1f - (distance / explosionRadius);
                        playerVehicle.TakeDamage(explosionDamage * damageMultiplier);
                    }

                    // Push player
                    Rigidbody2D playerRb = collider.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 pushDirection = (collider.transform.position - transform.position).normalized;
                        playerRb.linearVelocity += pushDirection * explosionForce;
                    }
                }

                // Damage enemies
                if (collider.CompareTag("Enemy"))
                {
                    DustOfWar.Enemies.Enemy enemy = collider.GetComponent<DustOfWar.Enemies.Enemy>();
                    if (enemy != null)
                    {
                        float distance = Vector2.Distance(transform.position, collider.transform.position);
                        float damageMultiplier = 1f - (distance / explosionRadius);
                        enemy.TakeDamage(explosionDamage * damageMultiplier);
                    }
                }
            }

            // Spawn explosion effect
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            // Destroy barrel
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
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

