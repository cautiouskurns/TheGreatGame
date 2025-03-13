// SettlementInfoPanel.cs - Create this as a new file
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettlementInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI resourceText;
    [SerializeField] private TextMeshProUGUI growthText;
    [SerializeField] private Image settlementIcon;
    
    [Header("Icons")]
    [SerializeField] private Sprite villageIcon;
    [SerializeField] private Sprite townIcon;
    [SerializeField] private Sprite cityIcon;
    [SerializeField] private Sprite metropolisIcon;
    
    private Settlement currentSettlement;
    
    // Singleton pattern for easy access
    public static SettlementInfoPanel Instance { get; private set; }
    
    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Hide the panel initially
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
    
    // Call this to show the panel with a settlement's info
    public void ShowSettlementInfo(Settlement settlement)
    {
        if (settlement == null || panelRoot == null) return;
        
        currentSettlement = settlement;
        
        // Update the UI elements
        if (titleText != null)
            titleText.text = settlement.settlementName;
            
        if (sizeText != null)
            sizeText.text = settlement.size.ToString();
            
        if (populationText != null)
            populationText.text = $"Population: {settlement.population} / {settlement.populationCap}";
            
        if (resourceText != null)
            resourceText.text = $"Resource Bonus: +{settlement.resourceBonus}";
            
        if (growthText != null)
        {
            int growthAmount = Mathf.RoundToInt(settlement.population * settlement.growthRate);
            growthText.text = $"Growth: +{growthAmount} per turn ({settlement.growthRate * 100}%)";
        }
        
        // Set the appropriate icon based on settlement size
        if (settlementIcon != null)
        {
            switch (settlement.size)
            {
                case Settlement.Size.Village:
                    settlementIcon.sprite = villageIcon;
                    break;
                case Settlement.Size.Town:
                    settlementIcon.sprite = townIcon;
                    break;
                case Settlement.Size.City:
                    settlementIcon.sprite = cityIcon;
                    break;
                case Settlement.Size.Metropolis:
                    settlementIcon.sprite = metropolisIcon;
                    break;
            }
        }
        
        // Show the panel
        panelRoot.SetActive(true);
    }
    
    // Call this to hide the panel
    public void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        
        currentSettlement = null;
    }
    
    // Method to update the panel (call this if settlement changes while panel is open)
    public void UpdatePanel()
    {
        if (currentSettlement != null && panelRoot != null && panelRoot.activeSelf)
        {
            ShowSettlementInfo(currentSettlement);
        }
    }
}