using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CookieController cookieController;
    [SerializeField] private ObjectSpawner objectSpawner;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI mainCounterText;
    [SerializeField] private List<TextMeshProUGUI> additionalCounterTexts = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI cookiesPerSecondText;
    
    [Header("Upgrades Configuration")]
    [SerializeField] private List<UpgradeDefinition> availableUpgrades = new List<UpgradeDefinition>();
    
    [Header("Runtime Data")]
    [SerializeField] private float totalCookies = 0;
    [SerializeField] private float cookiesPerSecond = 0;
    private Dictionary<string, int> purchasedUpgrades = new Dictionary<string, int>();
    private Dictionary<string, List<GameObject>> activeUpgradeObjects = new Dictionary<string, List<GameObject>>();
    
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize upgrade tracking
        foreach (var upgrade in availableUpgrades)
        {
            purchasedUpgrades[upgrade.id] = 0;
            activeUpgradeObjects[upgrade.id] = new List<GameObject>();
        }
    }
    
    private void Start()
    {
        
        if (objectSpawner != null)
        {
            // Subscribe to object spawned event to track new upgrades
            objectSpawner.objectSpawned += OnUpgradeObjectSpawned;
        }

        FindExistingUpgrades();
        CalculateTotalCookiesPerSecond();
        UpdateAllCounterDisplays();
    }
    
    private void Update()
    {
        // Check for destroyed upgrade objects
        CheckForDestroyedUpgrades();
        
        // Generate cookies based on time and CPS
        float cookiesGenerated = cookiesPerSecond * Time.deltaTime;
        if (cookiesGenerated > 0)
        {
            AddCookies(cookiesGenerated);
        }
    }
    
    private void FindExistingUpgrades()
    {
        // Find all UpgradeBehavior components in the scene
        UpgradeBehavior[] existingUpgrades = FindObjectsOfType<UpgradeBehavior>();
        
        foreach (var upgrade in existingUpgrades)
        {
            // Register each existing upgrade
            RegisterUpgrade(upgrade.gameObject, upgrade.upgradeId);
        }
    }
    
    private void CheckForDestroyedUpgrades()
    {
        bool needsRecalculation = false;
        
        // Check each type of upgrade
        foreach (var entry in activeUpgradeObjects)
        {
            string upgradeId = entry.Key;
            List<GameObject> upgradesList = entry.Value;
            
            // Check if any objects were destroyed
            for (int i = upgradesList.Count - 1; i >= 0; i--)
            {
                if (upgradesList[i] == null)
                {
                    // Object was destroyed, remove it from our list
                    upgradesList.RemoveAt(i);
                    needsRecalculation = true;
                }
            }
            
            // Update the count in purchasedUpgrades for consistency
            purchasedUpgrades[upgradeId] = upgradesList.Count;
        }
        
        // Recalculate CPS if needed
        if (needsRecalculation)
        {
            CalculateTotalCookiesPerSecond();
            UpdateAllCounterDisplays();
        }
    }
    
    public void AddCookies(float amount)
    {
        totalCookies += amount;
        
        // Update all UI elements
        UpdateAllCounterDisplays();
    }
    
    public void UpdateAllCounterDisplays()
    {
        // Format the cookie count with thousands separators
        string formattedCount = totalCookies.ToString("N0");
        
        // Update the main counter text if assigned
        if (mainCounterText != null)
        {
            mainCounterText.text = formattedCount;
        }
        
        // Update any additional counter texts
        foreach (var counterText in additionalCounterTexts)
        {
            if (counterText != null)
            {
                counterText.text = formattedCount;
            }
        }
        
        // Update cookies per second display if available
        if (cookiesPerSecondText != null)
        {
            cookiesPerSecondText.text = $"{cookiesPerSecond:F1}/s";
        }
    }
    
    public bool CanAffordUpgrade(string upgradeId)
    {
        UpgradeDefinition upgrade = availableUpgrades.Find(u => u.id == upgradeId);
        if (upgrade == null) return false;
        
        float cost = CalculateUpgradeCost(upgrade);
        return totalCookies >= cost;
    }
    
    public bool PurchaseUpgrade(string upgradeId, Vector3 spawnPosition, Vector3 spawnNormal)
    {
        UpgradeDefinition upgrade = availableUpgrades.Find(u => u.id == upgradeId);
        if (upgrade == null) return false;
        
        float cost = CalculateUpgradeCost(upgrade);
        
        if (totalCookies >= cost)
        {
            // Deduct cookies
            totalCookies -= cost;
            
            // Spawn the upgrade object
            int upgradeIndex = availableUpgrades.IndexOf(upgrade);
            objectSpawner.spawnOptionIndex = upgradeIndex;
            
            bool success = objectSpawner.TrySpawnObject(spawnPosition, spawnNormal);
            
            if (success)
            {
                // The ObjectSpawner.objectSpawned event will handle registering the upgrade
                // through the OnUpgradeObjectSpawned method
                
                // Update UI
                UpdateAllCounterDisplays();
                
                return true;
            }
        }
        
        return false;
    }
    
    private void OnUpgradeObjectSpawned(GameObject spawnedObject)
    {
        // Try to find an UpgradeBehavior component on the spawned object
        UpgradeBehavior upgradeBehavior = spawnedObject.GetComponent<UpgradeBehavior>();
        
        if (upgradeBehavior != null)
        {
            string upgradeId = upgradeBehavior.upgradeId;
            RegisterUpgrade(spawnedObject, upgradeId);
        }
    }
    
    private void RegisterUpgrade(GameObject upgradeObject, string upgradeId)
    {
        // Verify the upgrade ID is valid
        UpgradeDefinition upgradeDef = availableUpgrades.Find(u => u.id == upgradeId);
        if (upgradeDef == null)
        {
            Debug.LogWarning($"Unknown upgrade ID: {upgradeId}");
            return;
        }
        
        // Add to active objects list
        if (!activeUpgradeObjects.ContainsKey(upgradeId))
        {
            activeUpgradeObjects[upgradeId] = new List<GameObject>();
        }
        
        activeUpgradeObjects[upgradeId].Add(upgradeObject);
        
        // Update purchased count for consistency
        if (!purchasedUpgrades.ContainsKey(upgradeId))
        {
            purchasedUpgrades[upgradeId] = 0;
        }
        purchasedUpgrades[upgradeId] = activeUpgradeObjects[upgradeId].Count;
        
        // Recalculate CPS
        CalculateTotalCookiesPerSecond();
        
        // Update UI
        UpdateAllCounterDisplays();
        
        Debug.Log($"Registered upgrade: {upgradeId}. Total active: {activeUpgradeObjects[upgradeId].Count}");
    }
    
    private float CalculateUpgradeCost(UpgradeDefinition upgrade)
    {
        int count = 0;
        if (activeUpgradeObjects.ContainsKey(upgrade.id))
        {
            count = activeUpgradeObjects[upgrade.id].Count;
        }
        
        // Apply cost scaling based on how many of this upgrade are already in the scene
        return upgrade.baseCost * Mathf.Pow(upgrade.costScalingFactor, count);
    }
    
    private void CalculateTotalCookiesPerSecond()
    {
        cookiesPerSecond = 0;
        
        // Calculate based on active upgrade objects instead of purchasedUpgrades
        foreach (var upgrade in availableUpgrades)
        {
            if (activeUpgradeObjects.ContainsKey(upgrade.id))
            {
                int count = activeUpgradeObjects[upgrade.id].Count;
                cookiesPerSecond += count * upgrade.cookiesPerSecond;
            }
        }
        
        Debug.Log($"Recalculated CPS: {cookiesPerSecond}/s");
    }
    
    public UpgradeDefinition GetUpgradeDefinition(string upgradeId)
    {
        return availableUpgrades.Find(u => u.id == upgradeId);
    }
    
    public float GetUpgradeCost(string upgradeId)
    {
        UpgradeDefinition upgrade = availableUpgrades.Find(u => u.id == upgradeId);
        if (upgrade == null) return float.MaxValue; // Return "infinity" if upgrade doesn't exist
        
        return CalculateUpgradeCost(upgrade);
    }

    public float GetTotalCookies()
    {
        return totalCookies;
    }
    
    [Serializable]
    public class UpgradeDefinition
    {
        public string id;
        public string displayName;
        public float baseCost;
        public float cookiesPerSecond;
        public float costScalingFactor = 1.15f; // Price increases by 15% each purchase
        public GameObject prefab;
    }
}