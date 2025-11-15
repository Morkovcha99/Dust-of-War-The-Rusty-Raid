using UnityEngine;
using System.Collections;

namespace DustOfWar.Upgrades
{
    /// <summary>
    /// Ускорение движения
    /// Кратковременное повышение скорости
    /// </summary>
    [CreateAssetMenu(fileName = "SpeedBoost", menuName = "Dust of War/Upgrades/Speed Boost")]
    public class UpgradeSpeedBoost : TemporaryUpgrade
    {
        [Header("Speed Boost Settings")]
        [SerializeField] private float speedMultiplier = 1.5f; // 50% faster
        [SerializeField] private float duration = 10f; // Duration in seconds

        private float originalSpeed;
        private DustOfWar.Player.PlayerController controller;
        private float endTime;

        public override void ApplyUpgrade(GameObject player)
        {
            controller = player.GetComponent<DustOfWar.Player.PlayerController>();
            if (controller == null) return;

            originalSpeed = controller.GetMoveSpeed();
            controller.SetMoveSpeed(originalSpeed * speedMultiplier);
            endTime = Time.time + duration;

            // Schedule removal
            TemporaryUpgradeManager manager = player.GetComponent<TemporaryUpgradeManager>();
            if (manager != null)
            {
                manager.StartCoroutine(RemoveAfterDuration(player));
            }
        }

        public override void RemoveUpgrade(GameObject player)
        {
            if (controller != null)
            {
                controller.SetMoveSpeed(originalSpeed);
            }
        }

        private IEnumerator RemoveAfterDuration(GameObject player)
        {
            yield return new WaitForSeconds(duration);
            RemoveUpgrade(player);
        }
    }
}

