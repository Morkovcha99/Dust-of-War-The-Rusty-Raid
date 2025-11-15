using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Обычно не нужен для этого скрипта, но может быть полезен

public class HangarManager : MonoBehaviour
{
    // --- Вложенный класс CarConfig ---
    // Этот класс содержит все данные и текущее состояние одной машины.
    // [System.Serializable] нужен, чтобы Unity отображал эти поля в инспекторе.
    [System.Serializable]
    public class CarConfig
    {
        public string carName = "New Car";
        public Sprite carIcon; // Иконка для кнопки выбора

        [Header("Base Stats")]
        public int baseSpeed = 10;
        public int baseArmor = 10;
        public int baseDamage = 5;
        public int baseHandling = 10; // Маневренность

        [Header("Upgrade Costs")]
        public int speedUpgradeCost = 100;
        public int armorUpgradeCost = 100;
        public int damageUpgradeCost = 100;
        public int handlingUpgradeCost = 100;

        [Header("Upgrade Values (per level)")]
        public int speedIncreasePerLevel = 2;
        public int armorIncreasePerLevel = 2;
        public int damageIncreasePerLevel = 1;
        public int handlingIncreasePerLevel = 2;

        [Header("Current State (resets per selection)")]
        // Эти поля хранят текущий уровень прокачки машины.
        // Они будут сбрасываться при выборе новой машины.
        public int currentSpeedLevel = 0;
        public int currentArmorLevel = 0;
        public int currentDamageLevel = 0;
        public int currentHandlingLevel = 0;

        // --- Методы для получения текущих характеристик и стоимости ---
        public int GetCurrentSpeed()
        {
            return baseSpeed + currentSpeedLevel * speedIncreasePerLevel;
        }

        public int GetCurrentArmor()
        {
            return baseArmor + currentArmorLevel * armorIncreasePerLevel;
        }

        public int GetCurrentDamage()
        {
            return baseDamage + currentDamageLevel * damageIncreasePerLevel;
        }

        public int GetCurrentHandling()
        {
            return baseHandling + currentHandlingLevel * handlingIncreasePerLevel;
        }

        public int GetSpeedUpgradeCost()
        {
            // Простая формула для увеличения стоимости каждого следующего уровня
            return speedUpgradeCost + currentSpeedLevel * Mathf.RoundToInt(speedUpgradeCost * 0.5f);
        }

        public int GetArmorUpgradeCost()
        {
            return armorUpgradeCost + currentArmorLevel * Mathf.RoundToInt(armorUpgradeCost * 0.5f);
        }

        public int GetDamageUpgradeCost()
        {
            return damageUpgradeCost + currentDamageLevel * Mathf.RoundToInt(damageUpgradeCost * 0.5f);
        }

        public int GetHandlingUpgradeCost()
        {
            return handlingUpgradeCost + currentHandlingLevel * Mathf.RoundToInt(handlingUpgradeCost * 0.5f);
        }

        // Метод для сброса уровней прокачки при смене машины
        public void ResetLevels()
        {
            currentSpeedLevel = 0;
            currentArmorLevel = 0;
            currentDamageLevel = 0;
            currentHandlingLevel = 0;
        }
    }
    // --- Конец вложенного класса CarConfig ---

    [Header("Car Configurations")]
    // Массив, содержащий все машины, доступные для выбора, с их данными и состояниями
    public CarConfig[] availableCarsConfig;
    // Массив спрайтов машинок, которые будут отображаться в ангаре.
    // Порядок спрайтов должен соответствовать порядку CarConfig.
    public Sprite[] carSprites;

    [Header("Display Settings")]
    // Трансформ, на котором будет размещаться спрайт текущей машины.
    public Transform carDisplayPosition;
    // Рендерер спрайта, который будет менять свой спрайт в зависимости от выбора.
    public SpriteRenderer carDisplaySpriteRenderer;

    [Header("UI References")]
    // Панель, содержащая кнопки выбора машинок (обычно Scroll Rect Content)
    public Transform carButtonParent;
    // Prefab кнопки, которая будет создаваться для каждой машинки в списке
    public GameObject carSelectionButtonPrefab;

    // UI элементы для отображения информации о текущей машине
    public TextMeshProUGUI carNameText;
    public Image carIconImage; // Иконка машины (для кнопки выбора и, возможно, тут)

    // UI элементы для отображения текущих характеристик
    public TextMeshProUGUI speedValueText;
    public TextMeshProUGUI armorValueText;
    public TextMeshProUGUI damageValueText;
    public TextMeshProUGUI handlingValueText;

    // UI элементы для отображения стоимости улучшений
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI armorCostText;
    public TextMeshProUGUI damageCostText;
    public TextMeshProUGUI handlingCostText;

    // UI элементы для отображения игровой валюты
    public TextMeshProUGUI rustyBoltsText;
    public TextMeshProUGUI fuelCansText;

    // Кнопки прокачки
    public Button upgradeSpeedButton;
    public Button upgradeArmorButton;
    public Button upgradeDamageButton;
    public Button upgradeHandlingButton;

    // --- Индексы текущей выбранной машины ---
    private int currentCarConfigIndex = 0; // Индекс в массиве availableCarsConfig
    private int currentCarSpriteIndex = 0; // Индекс в массиве carSprites

    void Awake()
    {
        // Проверки на корректность назначенных ассетов
        if (availableCarsConfig == null || availableCarsConfig.Length == 0)
        {
            Debug.LogError("No Car Configurations found! Please assign availableCarsConfig in HangarManager.");
            enabled = false; // Деактивируем скрипт, если нет данных
            return;
        }
        if (carDisplaySpriteRenderer == null)
        {
            Debug.LogError("Car Display Sprite Renderer is not assigned! Assign the SpriteRenderer of the car display object.");
            enabled = false;
            return;
        }
        if (carSprites == null || carSprites.Length != availableCarsConfig.Length)
        {
            Debug.LogError("Car Sprites array is not assigned or its length does not match the number of Car Configurations!");
            enabled = false;
            return;
        }

        // Устанавливаем первую машину как выбранную по умолчанию при старте
        currentCarConfigIndex = 0;
        currentCarSpriteIndex = 0;
        LoadCar(currentCarConfigIndex); // Загружаем первую машину
    }

    void Start()
    {
        UpdateUI(); // Обновляем UI с данными первой машины
        PopulateCarSelection(); // Создаем кнопки для выбора всех доступных машин
        SetupUpgradeButtons(); // Настраиваем слушатели событий для кнопок прокачки
    }

    void Update()
    {
        // В 2D-ангаре нет вращения камеры вокруг машины, как в 3D.
        // Если вам нужна какая-то анимация или эффект, их можно реализовать здесь.
        UpdateCurrencyDisplay(); // Постоянно обновляем отображение валюты
    }

    // --- Загрузка и отображение выбранной машинки ---

    // Загружает данные и спрайт для выбранной машины по индексу
    public void LoadCar(int index)
    {
        // Проверка на валидность индекса
        if (index < 0 || index >= availableCarsConfig.Length)
        {
            Debug.LogError("Invalid car index requested!");
            return;
        }

        // Обновляем текущие индексы
        currentCarConfigIndex = index;
        currentCarSpriteIndex = index; // В данном случае, индекс конфигурации и спрайта совпадают

        CarConfig selectedCarConfig = availableCarsConfig[currentCarConfigIndex];

        // Отображаем спрайт машинки в ангаре
        if (carSprites.Length > currentCarSpriteIndex && carSprites[currentCarSpriteIndex] != null)
        {
            carDisplaySpriteRenderer.sprite = carSprites[currentCarSpriteIndex];
            // Позиционируем спрайт, если он не привязан к transform carDisplayPosition
            carDisplaySpriteRenderer.transform.position = carDisplayPosition.position;
            // Возможно, потребуется масштабирование спрайта (carDisplaySpriteRenderer.transform.localScale)
        }
        else
        {
            Debug.LogError($"Sprite not found for car index {currentCarSpriteIndex}. Ensure the sprite is assigned and the index is correct.");
            carDisplaySpriteRenderer.sprite = null; // Скрываем, если спрайта нет
        }
        if (carDisplaySpriteRenderer == null)
        {
            Debug.LogError("Car Display Sprite Renderer is null or destroyed! Cannot load car.");
            return; // Прерываем выполнение функции, если рендерер уничтожен
        }

        // Сбрасываем уровни прокачки для новой машины
        selectedCarConfig.ResetLevels();

        UpdateUI(); // Обновляем UI с новыми данными
        UpdateUpgradeButtonsInteractable(); // Обновляем доступность кнопок прокачки
    }

    // Метод-обертка для вызова LoadCar() с проверкой
    public void SelectCarByIndex(int index)
    {
        // Проверяем, существует ли еще SpriteRenderer
        if (carDisplaySpriteRenderer != null)
        {
            LoadCar(index); // Вызываем основную логику загрузки
        }
        else
        {
            Debug.LogError("Car Display Sprite Renderer is null or destroyed! Cannot load car.");
            // Можно попробовать переинициализировать HangarManager или сцену,
            // или просто сообщить об ошибке, если повторная загрузка невозможна.
        }
    }
    // Метод для перехода к следующей машинке
    public void NextCar()
    {
        // Переходим к следующему индексу, если достигли конца - возвращаемся к первому
        currentCarConfigIndex = (currentCarConfigIndex + 1) % availableCarsConfig.Length;
        // Важно: индекс спрайта тоже должен соответствовать
        currentCarSpriteIndex = currentCarConfigIndex;
        LoadCar(currentCarConfigIndex);
        UpdateUI();
        UpdateUpgradeButtonsInteractable();
    }

    // Метод для перехода к предыдущей машинке
    public void PreviousCar()
    {
        // Переходим к предыдущему индексу, если достигли начала - возвращаемся к последнему
        currentCarConfigIndex--;
        if (currentCarConfigIndex < 0)
        {
            currentCarConfigIndex = availableCarsConfig.Length - 1;
        }
        // Важно: индекс спрайта тоже должен соответствовать
        currentCarSpriteIndex = currentCarConfigIndex;
        LoadCar(currentCarConfigIndex);
        UpdateUI();
        UpdateUpgradeButtonsInteractable();
    }
    // --- Управление UI ---

    // Создает кнопки выбора машинок в Scroll View
    void PopulateCarSelection()
    {
        // Очищаем старые кнопки, если они есть
        foreach (Transform child in carButtonParent)
        {
            Destroy(child.gameObject);
        }

        // Создаем новую кнопку для каждой доступной машины
        for (int i = 0; i < availableCarsConfig.Length; i++)
        {
            int carIndex = i; // Важно для корректной передачи индекса в лямбду
            GameObject buttonGO = Instantiate(carSelectionButtonPrefab, carButtonParent);
            Button button = buttonGO.GetComponent<Button>();

            // Настройка отображения кнопки
            var buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = availableCarsConfig[carIndex].carName;

            var buttonImage = buttonGO.GetComponentInChildren<Image>(); // Для иконки машинки на кнопке
            if (buttonImage != null && availableCarsConfig[carIndex].carIcon != null)
            {
                buttonImage.sprite = availableCarsConfig[carIndex].carIcon;
                buttonImage.enabled = true;
            }
            else if (buttonImage != null)
            {
                buttonImage.enabled = false; // Скрываем Image, если нет иконки
            }

            // Добавляем слушатель события нажатия на кнопку
            button.onClick.AddListener(() => SelectCarByIndex(carIndex));
        }
    }

    // Обработчик нажатия на кнопку выбора машинки
    void OnCarSelectButtonClick(int index)
    {
        LoadCar(index); // Загружаем выбранную машину
        UpdateUI(); // Обновляем UI с её данными
        UpdateUpgradeButtonsInteractable(); // Обновляем состояние кнопок прокачки
    }

    // Обновляет все UI элементы, отображающие информацию о текущей машине
    void UpdateUI()
    {
        CarConfig currentConfig = availableCarsConfig[currentCarConfigIndex]; // Текущая конфигурация машины

        // Обновляем название и иконку машины
        carNameText.text = currentConfig.carName;
        if (carIconImage != null && currentConfig.carIcon != null)
        {
            carIconImage.sprite = currentConfig.carIcon;
            carIconImage.enabled = true;
        }
        else if (carIconImage != null)
        {
            carIconImage.enabled = false; // Скрываем, если иконки нет
        }

        // Обновляем текущие характеристики
        speedValueText.text = currentConfig.GetCurrentSpeed().ToString();
        armorValueText.text = currentConfig.GetCurrentArmor().ToString();
        damageValueText.text = currentConfig.GetCurrentDamage().ToString();
        handlingValueText.text = currentConfig.GetCurrentHandling().ToString();

        // Обновляем стоимость улучшений
        speedCostText.text = currentConfig.GetSpeedUpgradeCost().ToString();
        armorCostText.text = currentConfig.GetArmorUpgradeCost().ToString();
        damageCostText.text = currentConfig.GetDamageUpgradeCost().ToString();
        handlingCostText.text = currentConfig.GetHandlingUpgradeCost().ToString();
    }

    // Обновляет отображение текущей валюты игрока
    void UpdateCurrencyDisplay()
    {
        if (PlayerData.Instance != null)
        {
            rustyBoltsText.text = PlayerData.Instance.rustyBolts.ToString();
            fuelCansText.text = PlayerData.Instance.fuelCans.ToString();
        }
    }

    // --- Прокачка машины ---

    // Настраивает слушатели событий для кнопок прокачки
    void SetupUpgradeButtons()
    {
        upgradeSpeedButton.onClick.AddListener(UpgradeSpeed);
        upgradeArmorButton.onClick.AddListener(UpgradeArmor);
        upgradeDamageButton.onClick.AddListener(UpgradeDamage);
        upgradeHandlingButton.onClick.AddListener(UpgradeHandling);
    }

    // Обновляет доступность кнопок прокачки в зависимости от наличия валюты
    void UpdateUpgradeButtonsInteractable()
    {
        CarConfig currentConfig = availableCarsConfig[currentCarConfigIndex];

        // Кнопка активна, только если у игрока достаточно валюты
        upgradeSpeedButton.interactable = PlayerData.Instance.HasEnoughRustyBolts(currentConfig.GetSpeedUpgradeCost());
        upgradeArmorButton.interactable = PlayerData.Instance.HasEnoughRustyBolts(currentConfig.GetArmorUpgradeCost());
        upgradeDamageButton.interactable = PlayerData.Instance.HasEnoughRustyBolts(currentConfig.GetDamageUpgradeCost());
        upgradeHandlingButton.interactable = PlayerData.Instance.HasEnoughRustyBolts(currentConfig.GetHandlingUpgradeCost());
    }

    // Методы для прокачки каждого параметра
    public void UpgradeSpeed()
    {
        CarConfig currentConfig = availableCarsConfig[currentCarConfigIndex];
        int cost = currentConfig.GetSpeedUpgradeCost();

        if (PlayerData.Instance.HasEnoughRustyBolts(cost))
        {
            PlayerData.Instance.SpendRustyBolts(cost); // Списываем валюту
            currentConfig.currentSpeedLevel++;        // Увеличиваем уровень прокачки
            UpdateUI(); // Обновляем UI с новыми характеристиками и стоимостью
            UpdateUpgradeButtonsInteractable(); // Обновляем доступность кнопок
        }
    }

    public void UpgradeArmor()
    {
        CarConfig currentConfig = availableCarsConfig[currentCarConfigIndex];
        int cost = currentConfig.GetArmorUpgradeCost();

        if (PlayerData.Instance.HasEnoughRustyBolts(cost))
        {
            PlayerData.Instance.SpendRustyBolts(cost);
            currentConfig.currentArmorLevel++;
            UpdateUI();
            UpdateUpgradeButtonsInteractable();
        }
    }

    public void UpgradeDamage()
    {
        CarConfig currentConfig = availableCarsConfig[currentCarConfigIndex];
        int cost = currentConfig.GetDamageUpgradeCost();

        if (PlayerData.Instance.HasEnoughRustyBolts(cost))
        {
            PlayerData.Instance.SpendRustyBolts(cost);
            currentConfig.currentDamageLevel++;
            UpdateUI();
            UpdateUpgradeButtonsInteractable();
        }
    }

    public void UpgradeHandling()
    {
        CarConfig currentConfig = availableCarsConfig[currentCarConfigIndex];
        int cost = currentConfig.GetHandlingUpgradeCost();

        if (PlayerData.Instance.HasEnoughRustyBolts(cost))
        {
            PlayerData.Instance.SpendRustyBolts(cost);
            currentConfig.currentHandlingLevel++;
            UpdateUI();
            UpdateUpgradeButtonsInteractable();
        }
    }

    // --- Переход к игре ---
    // Метод, вызываемый кнопкой "Start Battle"
    public void StartBattle()
    {
        // Здесь вы можете передать данные выбранной машины в игровую сцену.
        // Например, сохраняя их в PlayerData или другом Singleton-объекте.
        // PlayerData.Instance.SetSelectedCar(availableCarsConfig[currentCarConfigIndex]); // Пример

        Debug.Log("Starting Battle with " + availableCarsConfig[currentCarConfigIndex].carName + "!");
        // UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene"); // Пример загрузки игровой сцены
    }
}