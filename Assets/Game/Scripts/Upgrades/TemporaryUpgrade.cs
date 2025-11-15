using UnityEngine;

namespace DustOfWar.Upgrades
{
    /// <summary>
    /// Base class for temporary upgrades (Bent Parts)
    /// These upgrades last only until the end of the current run
    /// </summary>
    public abstract class TemporaryUpgrade : ScriptableObject
    {
        [Header("Upgrade Info")]
        [SerializeField] protected string upgradeName = "Bent Part";
        [SerializeField] protected string description = "Temporary upgrade";
        [SerializeField] protected Sprite icon;

        public string UpgradeName => upgradeName;
        public string Description => description;
        public Sprite Icon => icon;

        /// <summary>
        /// Apply the upgrade effect
        /// </summary>
        public abstract void ApplyUpgrade(GameObject player);

        /// <summary>
        /// Remove the upgrade effect (when run ends or upgrade expires)
        /// </summary>
        public abstract void RemoveUpgrade(GameObject player);
    }
}

