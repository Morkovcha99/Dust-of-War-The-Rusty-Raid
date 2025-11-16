using UnityEngine;
using System.Collections.Generic;
using DustOfWar.Resources;

namespace DustOfWar.Gameplay
{
    /// <summary>
    /// Save system for player progress
    /// Uses PlayerPrefs for WebGL compatibility
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private static SaveSystem instance;
        public static SaveSystem Instance => instance;

        // Keys for PlayerPrefs
        private const string KEY_RUSTY_BOLTS = "RustyBolts";
        private const string KEY_FUEL_CANISTERS = "FuelCanisters";
        private const string KEY_TOTAL_PLAY_TIME = "TotalPlayTime";
        private const string KEY_TOTAL_ENEMIES_KILLED = "TotalEnemiesKilled";
        private const string KEY_HIGHEST_WAVE = "HighestWave";
        private const string KEY_UNLOCKED_VEHICLES = "UnlockedVehicles";
        private const string KEY_PLAYER_LEVEL = "PlayerLevel";
        private const string KEY_EXPERIENCE = "Experience";

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Save currency (Rusty Bolts)
        /// </summary>
        public void SaveRustyBolts(int amount)
        {
            PlayerPrefs.SetInt(KEY_RUSTY_BOLTS, amount);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load currency (Rusty Bolts)
        /// </summary>
        public int LoadRustyBolts()
        {
            return PlayerPrefs.GetInt(KEY_RUSTY_BOLTS, 0);
        }

        /// <summary>
        /// Add currency (Rusty Bolts)
        /// </summary>
        public void AddRustyBolts(int amount)
        {
            int current = LoadRustyBolts();
            SaveRustyBolts(current + amount);
        }

        /// <summary>
        /// Save fuel canisters
        /// </summary>
        public void SaveFuelCanisters(int amount)
        {
            PlayerPrefs.SetInt(KEY_FUEL_CANISTERS, amount);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load fuel canisters
        /// </summary>
        public int LoadFuelCanisters()
        {
            return PlayerPrefs.GetInt(KEY_FUEL_CANISTERS, 0);
        }

        /// <summary>
        /// Add fuel canisters
        /// </summary>
        public void AddFuelCanisters(int amount)
        {
            int current = LoadFuelCanisters();
            SaveFuelCanisters(current + amount);
        }

        /// <summary>
        /// Save total play time
        /// </summary>
        public void SaveTotalPlayTime(float time)
        {
            PlayerPrefs.SetFloat(KEY_TOTAL_PLAY_TIME, time);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load total play time
        /// </summary>
        public float LoadTotalPlayTime()
        {
            return PlayerPrefs.GetFloat(KEY_TOTAL_PLAY_TIME, 0f);
        }

        /// <summary>
        /// Add to total play time
        /// </summary>
        public void AddTotalPlayTime(float time)
        {
            float current = LoadTotalPlayTime();
            SaveTotalPlayTime(current + time);
        }

        /// <summary>
        /// Save total enemies killed
        /// </summary>
        public void SaveTotalEnemiesKilled(int count)
        {
            PlayerPrefs.SetInt(KEY_TOTAL_ENEMIES_KILLED, count);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load total enemies killed
        /// </summary>
        public int LoadTotalEnemiesKilled()
        {
            return PlayerPrefs.GetInt(KEY_TOTAL_ENEMIES_KILLED, 0);
        }

        /// <summary>
        /// Add to total enemies killed
        /// </summary>
        public void AddTotalEnemiesKilled(int count)
        {
            int current = LoadTotalEnemiesKilled();
            SaveTotalEnemiesKilled(current + count);
        }

        /// <summary>
        /// Save highest wave reached
        /// </summary>
        public void SaveHighestWave(int wave)
        {
            int current = LoadHighestWave();
            if (wave > current)
            {
                PlayerPrefs.SetInt(KEY_HIGHEST_WAVE, wave);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Load highest wave reached
        /// </summary>
        public int LoadHighestWave()
        {
            return PlayerPrefs.GetInt(KEY_HIGHEST_WAVE, 0);
        }

        /// <summary>
        /// Save unlocked vehicles (comma-separated list of vehicle IDs)
        /// </summary>
        public void SaveUnlockedVehicles(List<string> vehicleIds)
        {
            string vehicles = string.Join(",", vehicleIds.ToArray());
            PlayerPrefs.SetString(KEY_UNLOCKED_VEHICLES, vehicles);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load unlocked vehicles
        /// </summary>
        public List<string> LoadUnlockedVehicles()
        {
            string vehicles = PlayerPrefs.GetString(KEY_UNLOCKED_VEHICLES, "");
            if (string.IsNullOrEmpty(vehicles))
            {
                return new List<string> { "default" }; // Default vehicle always unlocked
            }
            return new List<string>(vehicles.Split(','));
        }

        /// <summary>
        /// Unlock a vehicle
        /// </summary>
        public void UnlockVehicle(string vehicleId)
        {
            List<string> unlocked = LoadUnlockedVehicles();
            if (!unlocked.Contains(vehicleId))
            {
                unlocked.Add(vehicleId);
                SaveUnlockedVehicles(unlocked);
            }
        }

        /// <summary>
        /// Check if vehicle is unlocked
        /// </summary>
        public bool IsVehicleUnlocked(string vehicleId)
        {
            List<string> unlocked = LoadUnlockedVehicles();
            return unlocked.Contains(vehicleId);
        }

        /// <summary>
        /// Save player level
        /// </summary>
        public void SavePlayerLevel(int level)
        {
            PlayerPrefs.SetInt(KEY_PLAYER_LEVEL, level);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load player level
        /// </summary>
        public int LoadPlayerLevel()
        {
            return PlayerPrefs.GetInt(KEY_PLAYER_LEVEL, 1);
        }

        /// <summary>
        /// Save experience points
        /// </summary>
        public void SaveExperience(int exp)
        {
            PlayerPrefs.SetInt(KEY_EXPERIENCE, exp);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load experience points
        /// </summary>
        public int LoadExperience()
        {
            return PlayerPrefs.GetInt(KEY_EXPERIENCE, 0);
        }

        /// <summary>
        /// Add experience points
        /// </summary>
        public void AddExperience(int exp)
        {
            int current = LoadExperience();
            SaveExperience(current + exp);
        }

        /// <summary>
        /// Save all game progress
        /// </summary>
        public void SaveAllProgress()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Delete all saved data (for testing/reset)
        /// </summary>
        public void DeleteAllData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Save resources from current session to permanent storage
        /// </summary>
        public void SaveSessionResources()
        {
            if (GameStatsManager.Instance == null) return;

            // Save resources collected this session
            var resources = GameStatsManager.Instance.GetAllResources();
            
            if (resources != null && resources.ContainsKey(DustOfWar.Resources.ResourcePickup.ResourceType.RustyBolt))
            {
                int sessionBolts = resources[DustOfWar.Resources.ResourcePickup.ResourceType.RustyBolt];
                if (sessionBolts > 0)
                {
                    AddRustyBolts(sessionBolts);
                }
            }

            if (resources != null && resources.ContainsKey(DustOfWar.Resources.ResourcePickup.ResourceType.FuelCanister))
            {
                int sessionFuel = resources[DustOfWar.Resources.ResourcePickup.ResourceType.FuelCanister];
                if (sessionFuel > 0)
                {
                    AddFuelCanisters(sessionFuel);
                }
            }

            // Save statistics
            AddTotalPlayTime(GameStatsManager.Instance.GetPlayTime());
            AddTotalEnemiesKilled(GameStatsManager.Instance.GetEnemiesKilled());
        }

        /// <summary>
        /// Save bolts immediately when collected (for real-time saving)
        /// </summary>
        public void SaveBoltOnCollection(int amount)
        {
            if (amount > 0)
            {
                AddRustyBolts(amount);
            }
        }
    }
}

