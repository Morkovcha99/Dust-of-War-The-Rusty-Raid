using UnityEngine;
using System.Collections.Generic;

namespace DustOfWar.Upgrades
{
    /// <summary>
    /// Manages temporary upgrades (Bent Parts) for the player
    /// Tracks active upgrades and applies their effects
    /// </summary>
    [RequireComponent(typeof(DustOfWar.Player.PlayerVehicle))]
    [RequireComponent(typeof(DustOfWar.Player.PlayerAutoShooter))]
    [RequireComponent(typeof(DustOfWar.Player.PlayerController))]
    public class TemporaryUpgradeManager : MonoBehaviour
    {
        private List<TemporaryUpgrade> activeUpgrades = new List<TemporaryUpgrade>();
        private DustOfWar.Player.PlayerVehicle playerVehicle;
        private DustOfWar.Player.PlayerAutoShooter playerShooter;
        private DustOfWar.Player.PlayerController playerController;

        // Events
        public System.Action<TemporaryUpgrade> OnUpgradeApplied;
        public System.Action<TemporaryUpgrade> OnUpgradeRemoved;

        private void Awake()
        {
            playerVehicle = GetComponent<DustOfWar.Player.PlayerVehicle>();
            playerShooter = GetComponent<DustOfWar.Player.PlayerAutoShooter>();
            playerController = GetComponent<DustOfWar.Player.PlayerController>();
        }

        /// <summary>
        /// Apply a temporary upgrade
        /// </summary>
        public void ApplyUpgrade(TemporaryUpgrade upgrade)
        {
            if (upgrade == null) return;

            upgrade.ApplyUpgrade(gameObject);
            activeUpgrades.Add(upgrade);
            OnUpgradeApplied?.Invoke(upgrade);
        }

        /// <summary>
        /// Remove all temporary upgrades (when run ends)
        /// </summary>
        public void RemoveAllUpgrades()
        {
            foreach (var upgrade in activeUpgrades)
            {
                if (upgrade != null)
                {
                    upgrade.RemoveUpgrade(gameObject);
                    OnUpgradeRemoved?.Invoke(upgrade);
                }
            }
            activeUpgrades.Clear();
        }

        /// <summary>
        /// Get list of active upgrades
        /// </summary>
        public List<TemporaryUpgrade> GetActiveUpgrades()
        {
            return new List<TemporaryUpgrade>(activeUpgrades);
        }

        /// <summary>
        /// Check if upgrade is active
        /// </summary>
        public bool HasUpgrade(TemporaryUpgrade upgrade)
        {
            return activeUpgrades.Contains(upgrade);
        }
    }
}

