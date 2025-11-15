using UnityEngine;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Rock obstacle - blocks movement and can be destroyed
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Rock : MonoBehaviour
    {
        [Header("Rock Settings")]
        [SerializeField] private float health = 30f;
        [SerializeField] private bool canBeDestroyed = true;
        [SerializeField] private GameObject destructionEffect;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Take damage from projectiles
            DustOfWar.Combat.Projectile projectile = other.GetComponent<DustOfWar.Combat.Projectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.GetDamage());
            }
        }

        public void TakeDamage(float damage)
        {
            if (!canBeDestroyed) return;

            health -= damage;
            if (health <= 0f)
            {
                DestroyRock();
            }
        }

        private void DestroyRock()
        {
            if (destructionEffect != null)
            {
                Instantiate(destructionEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}

