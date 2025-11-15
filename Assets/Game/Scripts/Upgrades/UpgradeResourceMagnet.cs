using UnityEngine;

namespace DustOfWar.Upgrades
{
    /// <summary>
    /// Бонус к сбору ресурсов
    /// Увеличивает притяжение ресурсов
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceMagnet", menuName = "Dust of War/Upgrades/Resource Magnet")]
    public class UpgradeResourceMagnet : TemporaryUpgrade
    {
        [Header("Resource Magnet Settings")]
        [SerializeField] private float magnetRadiusMultiplier = 2f; // Double the attraction radius
        [SerializeField] private float magnetSpeedMultiplier = 1.5f; // Faster attraction

        private DustOfWar.Resources.ResourceMagnet magnetComponent;

        public override void ApplyUpgrade(GameObject player)
        {
            // Get or add ResourceMagnet component
            magnetComponent = player.GetComponent<DustOfWar.Resources.ResourceMagnet>();
            if (magnetComponent == null)
            {
                magnetComponent = player.AddComponent<DustOfWar.Resources.ResourceMagnet>();
            }

            // Apply multipliers
            magnetComponent.SetRadiusMultiplier(magnetRadiusMultiplier);
            magnetComponent.SetSpeedMultiplier(magnetSpeedMultiplier);
        }

        public override void RemoveUpgrade(GameObject player)
        {
            if (magnetComponent != null)
            {
                magnetComponent.SetRadiusMultiplier(1f);
                magnetComponent.SetSpeedMultiplier(1f);
            }
        }
    }
}

