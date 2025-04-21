using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private string upgradeId;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI cpsText;
    [SerializeField] private Button button;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    
    private float currentCost;
    
    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        if (interactable == null)
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        
        UpdateButtonState();
    }
    
    private void Update()
    {
        UpdateButtonState();
    }
    
    public void UpdateButtonState()
    {
        if (GameManager.Instance == null)
            return;
            
        // Get the upgrade definition
        var upgradeDef = GameManager.Instance.GetUpgradeDefinition(upgradeId);
        if (upgradeDef == null)
        {
            Debug.LogWarning($"No upgrade definition found for ID: {upgradeId}");
            return;
        }
        
        // Update the display texts
        if (nameText != null)
            nameText.text = upgradeDef.displayName;
            
        bool canAfford = GameManager.Instance.CanAffordUpgrade(upgradeId);
        currentCost = GameManager.Instance.GetUpgradeCost(upgradeId);
        
        if (costText != null)
            costText.text = currentCost.ToString("N0");
            
        if (cpsText != null)
            cpsText.text = $"+{upgradeDef.cookiesPerSecond:F1}/s";
        
        // Enable/disable the button based on affordability
        if (button != null)
            button.interactable = canAfford;
            
        // Set the XR interactable active/inactive based on affordability
        if (interactable != null)
            interactable.enabled = canAfford;
    }
    
    public void TryPurchase()
    {
        if (GameManager.Instance == null)
            return;
            
        GameManager.Instance.SelectUpgrade(upgradeId);
    }
}