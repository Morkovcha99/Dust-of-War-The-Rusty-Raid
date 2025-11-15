using UnityEngine;

namespace DustOfWar.Upgrades
{
    /// <summary>
    /// Кратковременное усиление оружия
    /// Увеличение скорострельности и урона
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponBoost", menuName = "Dust of War/Upgrades/Weapon Boost")]
    public class UpgradeWeaponBoost : TemporaryUpgrade
    {
        [Header("Weapon Boost Settings")]
        [SerializeField] private float fireRateMultiplier = 1.5f; // 50% faster
        [SerializeField] private float damageMultiplier = 1.3f; // 30% more damage

        private float originalFireRate;
        private float originalDamageMultiplier;
        private DustOfWar.Player.PlayerAutoShooter shooter;
        private DustOfWar.Player.PlayerVehicle vehicle;

        public override void ApplyUpgrade(GameObject player)
        {
            shooter = player.GetComponent<DustOfWar.Player.PlayerAutoShooter>();
            vehicle = player.GetComponent<DustOfWar.Player.PlayerVehicle>();

            if (shooter != null)
            {
                // Store original values
                originalFireRate = shooter.GetFireRate();
                
                // Apply fire rate boost
                shooter.SetFireRate(originalFireRate * fireRateMultiplier);
            }

            if (vehicle != null)
            {
                // Store original damage multiplier
                originalDamageMultiplier = vehicle.GetDamageMultiplier();
                
                // Apply damage boost
                vehicle.SetDamageMultiplier(originalDamageMultiplier * damageMultiplier);
            }
        }

        public override void RemoveUpgrade(GameObject player)
        {
            if (shooter != null)
            {
                shooter.SetFireRate(originalFireRate);
            }

            if (vehicle != null)
            {
                vehicle.SetDamageMultiplier(originalDamageMultiplier);
            }
        }
    }
}

