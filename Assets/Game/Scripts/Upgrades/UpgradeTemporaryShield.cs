using UnityEngine;

namespace DustOfWar.Upgrades
{
    /// <summary>
    /// Временный щит
    /// Поглощает определенное количество урона
    /// </summary>
    [CreateAssetMenu(fileName = "TemporaryShield", menuName = "Dust of War/Upgrades/Temporary Shield")]
    public class UpgradeTemporaryShield : TemporaryUpgrade
    {
        [Header("Shield Settings")]
        [SerializeField] private float shieldHealth = 50f;
        [SerializeField] private Color shieldColor = new Color(0.2f, 0.6f, 1f, 0.5f); // Blue tint

        private float currentShieldHealth;
        private DustOfWar.Player.PlayerVehicle vehicle;
        private GameObject shieldVisual;

        public override void ApplyUpgrade(GameObject player)
        {
            vehicle = player.GetComponent<DustOfWar.Player.PlayerVehicle>();
            if (vehicle == null) return;

            currentShieldHealth = shieldHealth;
            vehicle.SetShieldHealth(shieldHealth);

            // Create shield visual (optional)
            CreateShieldVisual(player);

            // Subscribe to damage events to update visual
            vehicle.OnDamageTaken += OnDamageTaken;
        }

        public override void RemoveUpgrade(GameObject player)
        {
            if (vehicle != null)
            {
                vehicle.OnDamageTaken -= OnDamageTaken;
            }

            // Remove shield visual
            if (shieldVisual != null)
            {
                Destroy(shieldVisual);
            }
        }

        private void OnDamageTaken(float damage)
        {
            // Update shield health from vehicle
            if (vehicle != null)
            {
                currentShieldHealth = vehicle.GetShieldHealth();
            }

            // Update shield visual
            UpdateShieldVisual();

            // Shield depleted
            if (currentShieldHealth <= 0f)
            {
                RemoveUpgrade(vehicle.gameObject);
            }
        }

        private void CreateShieldVisual(GameObject player)
        {
            // Create a simple visual representation of the shield
            GameObject shieldObj = new GameObject("ShieldVisual");
            shieldObj.transform.SetParent(player.transform);
            shieldObj.transform.localPosition = Vector3.zero;
            shieldObj.transform.localScale = Vector3.one * 1.2f;

            SpriteRenderer sr = shieldObj.AddComponent<SpriteRenderer>();
            sr.color = shieldColor;
            sr.sortingOrder = -1; // Behind player

            // Use player's sprite as base or create circle
            SpriteRenderer playerSr = player.GetComponent<SpriteRenderer>();
            if (playerSr != null)
            {
                sr.sprite = playerSr.sprite;
            }

            shieldVisual = shieldObj;
        }

        private void UpdateShieldVisual()
        {
            if (shieldVisual != null)
            {
                float alpha = currentShieldHealth / shieldHealth;
                SpriteRenderer sr = shieldVisual.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color color = shieldColor;
                    color.a = alpha * 0.5f;
                    sr.color = color;
                }
            }
        }

        public float GetShieldHealth() => currentShieldHealth;
        public float GetMaxShieldHealth() => shieldHealth;
    }
}

