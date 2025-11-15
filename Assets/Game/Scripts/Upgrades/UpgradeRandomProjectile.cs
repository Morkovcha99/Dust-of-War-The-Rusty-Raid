using UnityEngine;
using System.Collections;

namespace DustOfWar.Upgrades
{
    /// <summary>
    /// Случайный Снаряд
    /// Временно меняет тип снарядов на более мощный
    /// </summary>
    [CreateAssetMenu(fileName = "RandomProjectile", menuName = "Dust of War/Upgrades/Random Projectile")]
    public class UpgradeRandomProjectile : TemporaryUpgrade
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject[] upgradedProjectilePrefabs; // More powerful projectiles
        [SerializeField] private float damageMultiplier = 1.5f;
        [SerializeField] private float speedMultiplier = 1.2f;
        [SerializeField] private float duration = 15f;

        private GameObject originalProjectilePrefab;
        private float originalDamage;
        private float originalSpeed;
        private DustOfWar.Player.PlayerAutoShooter shooter;
        private GameObject currentProjectilePrefab;
        private float endTime;

        public override void ApplyUpgrade(GameObject player)
        {
            shooter = player.GetComponent<DustOfWar.Player.PlayerAutoShooter>();
            if (shooter == null) return;

            // Store original values
            originalProjectilePrefab = shooter.GetProjectilePrefab();
            originalDamage = shooter.GetProjectileDamage();
            originalSpeed = shooter.GetProjectileSpeed();

            // Select random upgraded projectile
            if (upgradedProjectilePrefabs != null && upgradedProjectilePrefabs.Length > 0)
            {
                currentProjectilePrefab = upgradedProjectilePrefabs[Random.Range(0, upgradedProjectilePrefabs.Length)];
                shooter.SetProjectilePrefab(currentProjectilePrefab);
            }

            // Apply multipliers
            shooter.SetProjectileDamage(originalDamage * damageMultiplier);
            shooter.SetProjectileSpeed(originalSpeed * speedMultiplier);

            endTime = Time.time + duration;
            TemporaryUpgradeManager manager = player.GetComponent<TemporaryUpgradeManager>();
            if (manager != null)
            {
                manager.StartCoroutine(RemoveAfterDuration(player));
            }
        }

        public override void RemoveUpgrade(GameObject player)
        {
            if (shooter != null)
            {
                shooter.SetProjectilePrefab(originalProjectilePrefab);
                shooter.SetProjectileDamage(originalDamage);
                shooter.SetProjectileSpeed(originalSpeed);
            }
        }

        private IEnumerator RemoveAfterDuration(GameObject player)
        {
            yield return new WaitForSeconds(duration);
            RemoveUpgrade(player);
        }
    }
}

