using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CookieController cookieController;
    [SerializeField] private ObjectSpawner objectSpawner;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI mainCounterText;
    [SerializeField] private TextMeshProUGUI cookiesPerSecondText;

    [Header("Selected Upgrade")]
    [SerializeField] private string selectedUpgradeId;
    private bool hasSelectedUpgrade = false;

    [Header("Upgrades Configuration")]
    [SerializeField] private List<UpgradeDefinition> availableUpgrades = new List<UpgradeDefinition>();

    [Header("Runtime Data")]
    [SerializeField] private float totalCookies = 0;
    [SerializeField] private float cookiesPerSecond = 0;

    private Dictionary<string, List<GameObject>> activeUpgrades = new Dictionary<string, List<GameObject>>();

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        SetupSingleton();
        InitializeUpgrades();
    }

    private void Start()
    {
        if (objectSpawner != null)
        {
            objectSpawner.objectSpawned -= OnUpgradeObjectSpawned;  // Remove if already connected
            objectSpawner.objectSpawned += OnUpgradeObjectSpawned;  // Connect placement handler
            objectSpawner.spawnValidationCallback = ValidateUpgradeSpawn;
        }
        
        FindExistingUpgrades();
        UpdateGameState();
    }

    private void Update()
    {
        if (CheckForDestroyedUpgrades())
        {
            UpdateGameState();
        }

        GenerateCookies();
    }

    #region Core Game Functions

    public void AddCookies(float amount)
    {
        totalCookies += amount;
        UpdateUI();
    }

    public float GetTotalCookies() => totalCookies;

    private void GenerateCookies()
    {
        float cookiesGenerated = cookiesPerSecond * Time.deltaTime;
        if (cookiesGenerated > 0)
        {
            AddCookies(cookiesGenerated);
        }
    }

    #endregion

    #region Upgrade Management

    public bool CanAffordUpgrade(string upgradeId)
    {
        float cost = GetUpgradeCost(upgradeId);
        return totalCookies >= cost && cost < float.MaxValue;
    }

    public bool PurchaseUpgrade(string upgradeId, Vector3 spawnPosition, Vector3 spawnNormal)
    {
        float cost = GetUpgradeCost(upgradeId);

        if (totalCookies < cost) return false;

        UpgradeDefinition upgrade = GetUpgradeDefinition(upgradeId);
        if (upgrade == null) return false;

        // Deduct cookies
        totalCookies -= cost;

        // Spawn the upgrade object
        objectSpawner.spawnOptionIndex = availableUpgrades.IndexOf(upgrade);
        bool success = objectSpawner.TrySpawnObject(spawnPosition, spawnNormal);

        if (success)
        {
            UpdateUI();
            return true;
        }

        return false;
    }

    public UpgradeDefinition GetUpgradeDefinition(string upgradeId)
    {
        return availableUpgrades.Find(u => u.id == upgradeId);
    }

    public float GetUpgradeCost(string upgradeId)
    {
        UpgradeDefinition upgrade = GetUpgradeDefinition(upgradeId);
        if (upgrade == null) return float.MaxValue;

        int count = GetUpgradeCount(upgradeId);
        return upgrade.baseCost * Mathf.Pow(upgrade.costScalingFactor, count);
    }

    private int GetUpgradeCount(string upgradeId)
    {
        return activeUpgrades.TryGetValue(upgradeId, out List<GameObject> upgrades) ? upgrades.Count : 0;
    }

    #endregion

    #region Initialization and Setup

    private void SetupSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeUpgrades()
    {
        foreach (var upgrade in availableUpgrades)
        {
            activeUpgrades[upgrade.id] = new List<GameObject>();
        }
    }

    private void FindExistingUpgrades()
    {
        UpgradeBehavior[] existingUpgrades = FindObjectsOfType<UpgradeBehavior>();

        foreach (var upgrade in existingUpgrades)
        {
            RegisterUpgrade(upgrade.gameObject, upgrade.upgradeId);
        }
    }

    #endregion

    #region Update Handlers
    
    // Modify OnUpgradeObjectSpawned method
    private void OnUpgradeObjectSpawned(GameObject spawnedObject)
    {
        UpgradeBehavior upgradeBehavior = spawnedObject.GetComponent<UpgradeBehavior>();
        
        if (upgradeBehavior != null)
        {
            // If this was from a selected upgrade, handle the purchase
            if (hasSelectedUpgrade && upgradeBehavior.upgradeId == selectedUpgradeId)
            {
                OnUpgradePlaced(spawnedObject);
            }
            
            RegisterUpgrade(spawnedObject, upgradeBehavior.upgradeId);
        }
    }

    private void RegisterUpgrade(GameObject upgradeObject, string upgradeId)
    {
        UpgradeDefinition upgradeDef = GetUpgradeDefinition(upgradeId);
        if (upgradeDef == null)
        {
            Debug.LogWarning($"Unknown upgrade ID: {upgradeId}");
            return;
        }

        if (!activeUpgrades.ContainsKey(upgradeId))
        {
            activeUpgrades[upgradeId] = new List<GameObject>();
        }

        activeUpgrades[upgradeId].Add(upgradeObject);
        UpdateGameState();

        Debug.Log($"Registered upgrade: {upgradeId}. Total active: {activeUpgrades[upgradeId].Count}");
    }

    private bool CheckForDestroyedUpgrades()
    {
        bool upgradesChanged = false;

        foreach (var entry in activeUpgrades)
        {
            List<GameObject> upgradesList = entry.Value;

            for (int i = upgradesList.Count - 1; i >= 0; i--)
            {
                if (upgradesList[i] == null)
                {
                    upgradesList.RemoveAt(i);
                    upgradesChanged = true;
                }
            }
        }

        return upgradesChanged;
    }

    #endregion

    #region State and UI Updates

    private void UpdateGameState()
    {
        CalculateTotalCookiesPerSecond();
        UpdateUI();
    }

    private void CalculateTotalCookiesPerSecond()
    {
        cookiesPerSecond = 0;

        foreach (var upgrade in availableUpgrades)
        {
            int count = GetUpgradeCount(upgrade.id);
            cookiesPerSecond += count * upgrade.cookiesPerSecond;
        }

        Debug.Log($"Recalculated CPS: {cookiesPerSecond}/s");
    }

    private void UpdateUI()
    {
        if (mainCounterText != null)
        {
            mainCounterText.text = totalCookies.ToString("N0");
        }

        if (cookiesPerSecondText != null)
        {
            cookiesPerSecondText.text = $"{cookiesPerSecond:F1}/s";
        }
    }

    #endregion

    private bool ValidateUpgradeSpawn(int spawnOptionIndex)
    {
        // Only allow spawning if an upgrade is selected
        if (!hasSelectedUpgrade)
            return false;

        string upgradeId = selectedUpgradeId;
        float cost = GetUpgradeCost(upgradeId);

        bool canAfford = totalCookies >= cost;

        if (!canAfford)
        {
            Debug.Log($"Cannot afford upgrade {upgradeId}: Cost {cost}, Have {totalCookies}");
            hasSelectedUpgrade = false;  // Reset selection if can't afford
        }

        return canAfford;
    }

    public void SelectUpgrade(string upgradeId)
    {
        if (!CanAffordUpgrade(upgradeId))
        {
            Debug.Log($"Cannot afford upgrade: {upgradeId}");
            return;
        }

        selectedUpgradeId = upgradeId;
        hasSelectedUpgrade = true;

        // Configure object spawner to use the selected upgrade prefab
        UpgradeDefinition upgrade = GetUpgradeDefinition(upgradeId);
        if (upgrade != null && objectSpawner != null)
        {
            objectSpawner.spawnOptionIndex = availableUpgrades.IndexOf(upgrade);
        }

        Debug.Log($"Selected upgrade: {upgradeId}");
    }

    public void OnUpgradePlaced(GameObject spawnedObject)
    {
        if (!hasSelectedUpgrade)
            return;

        // Handle the purchase transaction
        float cost = GetUpgradeCost(selectedUpgradeId);
        totalCookies -= cost;

        // Reset selection
        hasSelectedUpgrade = false;

        UpdateUI();
        Debug.Log($"Successfully purchased {selectedUpgradeId} for {cost} cookies");
    }

    [Serializable]
    public class UpgradeDefinition
    {
        public string id;
        public string displayName;
        public float baseCost;
        public float cookiesPerSecond;
        public float costScalingFactor = 1.15f;
        public GameObject prefab;
    }
}