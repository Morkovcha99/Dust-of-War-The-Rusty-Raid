using UnityEngine;
using System.Collections;

namespace DustOfWar.Upgrades
{
    /// <summary>
    /// Ярость Пустоши
    /// Оглушение ближайших врагов, увеличение урона, спецэффекты
    /// </summary>
    [CreateAssetMenu(fileName = "WastelandRage", menuName = "Dust of War/Upgrades/Wasteland Rage")]
    public class UpgradeWastelandRage : TemporaryUpgrade
    {
        [Header("Rage Settings")]
        [SerializeField] private float stunRadius = 8f;
        [SerializeField] private float stunDuration = 3f;
        [SerializeField] private float damageMultiplier = 2f; // Double damage
        [SerializeField] private float duration = 10f;
        [SerializeField] private GameObject acidRainEffect; // Optional visual effect

        private float originalDamageMultiplier;
        private DustOfWar.Player.PlayerVehicle vehicle;
        private DustOfWar.Player.PlayerAutoShooter shooter;
        private GameObject effectInstance;
        private float endTime;

        public override void ApplyUpgrade(GameObject player)
        {
            vehicle = player.GetComponent<DustOfWar.Player.PlayerVehicle>();
            shooter = player.GetComponent<DustOfWar.Player.PlayerAutoShooter>();

            if (vehicle != null)
            {
                originalDamageMultiplier = vehicle.GetDamageMultiplier();
                vehicle.SetDamageMultiplier(originalDamageMultiplier * damageMultiplier);
            }

            // Stun nearby enemies
            StunNearbyEnemies(player.transform.position);

            // Create visual effect
            if (acidRainEffect != null)
            {
                effectInstance = Instantiate(acidRainEffect, player.transform);
            }

            endTime = Time.time + duration;
            TemporaryUpgradeManager manager = player.GetComponent<TemporaryUpgradeManager>();
            if (manager != null)
            {
                manager.StartCoroutine(RemoveAfterDuration(player));
            }
        }

        public override void RemoveUpgrade(GameObject player)
        {
            if (vehicle != null)
            {
                vehicle.SetDamageMultiplier(originalDamageMultiplier);
            }

            if (effectInstance != null)
            {
                Destroy(effectInstance);
            }
        }

        private void StunNearbyEnemies(Vector3 center)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, stunRadius);
            
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    // Apply stun effect (enemies will need a stun component)
                    EnemyStun stun = collider.GetComponent<EnemyStun>();
                    if (stun == null)
                    {
                        stun = collider.gameObject.AddComponent<EnemyStun>();
                    }
                    stun.ApplyStun(stunDuration);
                }
            }
        }

        private IEnumerator RemoveAfterDuration(GameObject player)
        {
            yield return new WaitForSeconds(duration);
            RemoveUpgrade(player);
        }
    }

    /// <summary>
    /// Component for stunning enemies
    /// </summary>
    public class EnemyStun : MonoBehaviour
    {
        private bool isStunned = false;
        private Rigidbody2D rb;
        private Vector2 originalVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void ApplyStun(float duration)
        {
            if (isStunned) return;

            StartCoroutine(StunCoroutine(duration));
        }

        private IEnumerator StunCoroutine(float duration)
        {
            isStunned = true;
            
            if (rb != null)
            {
                originalVelocity = rb.linearVelocity;
                rb.linearVelocity = Vector2.zero;
            }

            // Visual feedback (optional)
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color originalColor = sr != null ? sr.color : Color.white;
            if (sr != null)
            {
                sr.color = Color.gray;
            }

            yield return new WaitForSeconds(duration);

            isStunned = false;
            if (rb != null)
            {
                rb.linearVelocity = originalVelocity;
            }
            if (sr != null)
            {
                sr.color = originalColor;
            }
        }

        public bool IsStunned() => isStunned;
    }
}

