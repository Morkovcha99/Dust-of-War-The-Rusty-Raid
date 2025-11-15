using UnityEngine;

namespace DustOfWar.Resources
{
    /// <summary>
    /// Resource pickup that can be collected by player
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ResourcePickup : MonoBehaviour
    {
        [Header("Resource Settings")]
        [SerializeField] private ResourceType resourceType = ResourceType.RustyBolt;
        [SerializeField] private int value = 1;

        public enum ResourceType
        {
            RustyBolt,      // Основная валюта
            FuelCanister,    // Редкая валюта
            HealthPickup,    // Восстановление здоровья
            UpgradePart      // Деталь для улучшения
        }

        public ResourceType Type => resourceType;
        public int Value => value;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CollectResource(other.gameObject);
            }
        }

        private void CollectResource(GameObject player)
        {
            // Notify resource manager or player
            // This will be handled by a resource manager system
            
            Destroy(gameObject);
        }
    }
}

