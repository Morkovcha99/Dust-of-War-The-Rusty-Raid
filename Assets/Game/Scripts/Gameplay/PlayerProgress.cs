using UnityEngine;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Manages player's permanent progress (currency, unlocks, etc.)
    /// Loads and saves progress using SaveSystem
    /// </summary>
    public class PlayerProgress : MonoBehaviour
    {
        private static PlayerProgress instance;
        public static PlayerProgress Instance => instance;

        [Header("Currency")]
        private int rustyBolts = 0;
        private int fuelCanisters = 0;

        [Header("Statistics")]
        private float totalPlayTime = 0f;
        private int totalEnemiesKilled = 0;
        private int highestWave = 0;

        [Header("Progression")]
        private int playerLevel = 1;
        private int experience = 0;

        // Events
        public System.Action<int> OnRustyBoltsChanged;
        public System.Action<int> OnFuelCanistersChanged;
        public System.Action<int> OnLevelChanged;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Load all progress from save system
        /// </summary>
        public void LoadProgress()
        {
            if (SaveSystem.Instance == null) return;

            rustyBolts = SaveSystem.Instance.LoadRustyBolts();
            fuelCanisters = SaveSystem.Instance.LoadFuelCanisters();
            totalPlayTime = SaveSystem.Instance.LoadTotalPlayTime();
            totalEnemiesKilled = SaveSystem.Instance.LoadTotalEnemiesKilled();
            highestWave = SaveSystem.Instance.LoadHighestWave();
            playerLevel = SaveSystem.Instance.LoadPlayerLevel();
            experience = SaveSystem.Instance.LoadExperience();
            
            // Notify listeners about loaded values
            OnRustyBoltsChanged?.Invoke(rustyBolts);
            OnFuelCanistersChanged?.Invoke(fuelCanisters);
        }

        /// <summary>
        /// Save all progress
        /// </summary>
        public void SaveProgress()
        {
            if (SaveSystem.Instance == null) return;

            SaveSystem.Instance.SaveRustyBolts(rustyBolts);
            SaveSystem.Instance.SaveFuelCanisters(fuelCanisters);
            SaveSystem.Instance.SaveTotalPlayTime(totalPlayTime);
            SaveSystem.Instance.SaveTotalEnemiesKilled(totalEnemiesKilled);
            SaveSystem.Instance.SaveHighestWave(highestWave);
            SaveSystem.Instance.SavePlayerLevel(playerLevel);
            SaveSystem.Instance.SaveExperience(experience);
            SaveSystem.Instance.SaveAllProgress();
        }

        // Currency Getters/Setters
        public int GetRustyBolts() => rustyBolts;
        public int GetFuelCanisters() => fuelCanisters;

        public void AddRustyBolts(int amount)
        {
            rustyBolts += amount;
            // Sync with SaveSystem immediately
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveRustyBolts(rustyBolts);
            }
            OnRustyBoltsChanged?.Invoke(rustyBolts);
        }
        
        /// <summary>
        /// Sync bolts from SaveSystem (useful when bolts are saved elsewhere)
        /// </summary>
        public void SyncBoltsFromSave()
        {
            if (SaveSystem.Instance != null)
            {
                int savedBolts = SaveSystem.Instance.LoadRustyBolts();
                if (savedBolts != rustyBolts)
                {
                    rustyBolts = savedBolts;
                    OnRustyBoltsChanged?.Invoke(rustyBolts);
                }
            }
        }

        public bool SpendRustyBolts(int amount)
        {
            if (rustyBolts >= amount)
            {
                rustyBolts -= amount;
                SaveSystem.Instance?.SaveRustyBolts(rustyBolts);
                OnRustyBoltsChanged?.Invoke(rustyBolts);
                return true;
            }
            return false;
        }

        public void AddFuelCanisters(int amount)
        {
            fuelCanisters += amount;
            SaveSystem.Instance?.SaveFuelCanisters(fuelCanisters);
            OnFuelCanistersChanged?.Invoke(fuelCanisters);
        }

        public bool SpendFuelCanisters(int amount)
        {
            if (fuelCanisters >= amount)
            {
                fuelCanisters -= amount;
                SaveSystem.Instance?.SaveFuelCanisters(fuelCanisters);
                OnFuelCanistersChanged?.Invoke(fuelCanisters);
                return true;
            }
            return false;
        }

        // Statistics Getters
        public float GetTotalPlayTime() => totalPlayTime;
        public int GetTotalEnemiesKilled() => totalEnemiesKilled;
        public int GetHighestWave() => highestWave;

        public void UpdateTotalPlayTime(float time)
        {
            totalPlayTime = time;
            SaveSystem.Instance?.SaveTotalPlayTime(totalPlayTime);
        }

        public void UpdateTotalEnemiesKilled(int count)
        {
            totalEnemiesKilled = count;
            SaveSystem.Instance?.SaveTotalEnemiesKilled(totalEnemiesKilled);
        }

        public void UpdateHighestWave(int wave)
        {
            if (wave > highestWave)
            {
                highestWave = wave;
                SaveSystem.Instance?.SaveHighestWave(highestWave);
            }
        }

        // Progression
        public int GetPlayerLevel() => playerLevel;
        public int GetExperience() => experience;

        public void AddExperience(int exp)
        {
            experience += exp;
            SaveSystem.Instance?.SaveExperience(experience);

            // Check level up (simple: 100 exp per level)
            int newLevel = (experience / 100) + 1;
            if (newLevel > playerLevel)
            {
                playerLevel = newLevel;
                SaveSystem.Instance?.SavePlayerLevel(playerLevel);
                OnLevelChanged?.Invoke(playerLevel);
            }
        }

        /// <summary>
        /// Check if vehicle is unlocked
        /// </summary>
        public bool IsVehicleUnlocked(string vehicleId)
        {
            return SaveSystem.Instance?.IsVehicleUnlocked(vehicleId) ?? false;
        }

        /// <summary>
        /// Unlock a vehicle
        /// </summary>
        public void UnlockVehicle(string vehicleId)
        {
            SaveSystem.Instance?.UnlockVehicle(vehicleId);
        }
    }
}


