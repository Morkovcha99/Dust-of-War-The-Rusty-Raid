using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    public int rustyBolts = 500;
    public int fuelCans = 50;

    // «десь будут хранитьс€ данные дл€ всех разблокированных и прокачанных машин игрока
    // ¬ упрощенном виде, мы будем использовать CurrentCarState дл€ отображени€,
    // но в реальной игре, эти данные нужно будет сохран€ть и загружать.

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // —охран€ем между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddRustyBolts(int amount)
    {
        rustyBolts += amount;
    }

    public void AddFuelCans(int amount)
    {
        fuelCans += amount;
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
        }
    }

    public void SpendFuelCans(int amount)
    {
        if (HasEnoughFuelCans(amount))
        {
            fuelCans -= amount;
        }
    }
}