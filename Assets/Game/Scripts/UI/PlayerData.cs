using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    public int rustyBolts = 0;
    public int fuelCans = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Sync with SaveSystem when enabled (e.g., when returning to menu)
        LoadFromSaveSystem();
    }

    /// <summary>
    /// Load bolts and fuel from SaveSystem
    /// </summary>
    public void LoadFromSaveSystem()
    {
        if (DustOfWar.Gameplay.SaveSystem.Instance != null)
        {
            rustyBolts = DustOfWar.Gameplay.SaveSystem.Instance.LoadRustyBolts();
            fuelCans = DustOfWar.Gameplay.SaveSystem.Instance.LoadFuelCanisters();
        }
    }

    public void AddRustyBolts(int amount)
    {
        rustyBolts += amount;
        // Save to SaveSystem immediately
        if (DustOfWar.Gameplay.SaveSystem.Instance != null)
        {
            DustOfWar.Gameplay.SaveSystem.Instance.SaveRustyBolts(rustyBolts);
        }
    }

    public void AddFuelCans(int amount)
    {
        fuelCans += amount;
        // Save to SaveSystem immediately
        if (DustOfWar.Gameplay.SaveSystem.Instance != null)
        {
            DustOfWar.Gameplay.SaveSystem.Instance.SaveFuelCanisters(fuelCans);
        }
    }

    public bool HasEnoughRustyBolts(int amount)
    {
        return rustyBolts >= amount;
    }

    public bool HasEnoughFuelCans(int amount)
    {
        return fuelCans >= amount;
    }

    public void SpendRustyBolts(int amount)
    {
        if (HasEnoughRustyBolts(amount))
        {
            rustyBolts -= amount;
            // Save to SaveSystem immediately
            if (DustOfWar.Gameplay.SaveSystem.Instance != null)
            {
                DustOfWar.Gameplay.SaveSystem.Instance.SaveRustyBolts(rustyBolts);
            }
        }
    }

    public void SpendFuelCans(int amount)
    {
        if (HasEnoughFuelCans(amount))
        {
            fuelCans -= amount;
            // Save to SaveSystem immediately
            if (DustOfWar.Gameplay.SaveSystem.Instance != null)
            {
                DustOfWar.Gameplay.SaveSystem.Instance.SaveFuelCanisters(fuelCans);
            }
        }
    }
}