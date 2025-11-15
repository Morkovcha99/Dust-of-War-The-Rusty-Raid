using UnityEngine;

public class CarStats : MonoBehaviour
{
    public string carName;
    public int baseSpeed;
    public int baseArmor;
    public int baseDamage;
    // ... другие базовые характеристики

    public int currentSpeedLevel = 0;
    public int currentArmorLevel = 0;
    public int currentDamageLevel = 0;
    // ...

    public int speedUpgradeCost = 100;
    public int armorUpgradeCost = 100;
    public int damageUpgradeCost = 100;
    // ...

    public int GetCurrentSpeed()
    {
        return baseSpeed + currentSpeedLevel * 5; // Пример: +5 к скорости за уровень
    }
    // ... аналогично для других характеристик
}