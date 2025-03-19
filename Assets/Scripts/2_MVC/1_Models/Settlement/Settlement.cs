using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Settlement : MonoBehaviour
{
    public enum Size
    {
        Village,
        Town,
        City,
        Metropolis
    }
    
    [Header("Settlement Properties")]
    public string settlementName;
    public Size size = Size.Village;
    public int population = 100;
    public int resourceBonus = 5; // Extra resources produced by this settlement
    
    [Header("Growth")]
    public float growthRate = 0.1f; // 10% growth per turn
    public int populationCap = 500; // Maximum population for current size
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite villageSprite;
    [SerializeField] private Sprite townSprite;
    [SerializeField] private Sprite citySprite;
    [SerializeField] private Sprite metropolisSprite;
    [SerializeField] private Color villageColor = new Color(0.8f, 0.8f, 0.6f);
    [SerializeField] private Color townColor = new Color(0.8f, 0.7f, 0.5f);
    [SerializeField] private Color cityColor = new Color(0.7f, 0.6f, 0.4f);
    [SerializeField] private Color metropolisColor = new Color(0.6f, 0.5f, 0.3f);
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro sizeText;
    
    [Header("References")]
    public Province province;
    
    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Ensure it renders above provinces
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Provinces";
            spriteRenderer.sortingOrder = 5;
        }
        
        UpdateVisuals();
    }
    
    public void Initialize(Province hostProvince, string name = "")
    {
        province = hostProvince;
        
        // Generate a name if none provided
        if (string.IsNullOrEmpty(name))
        {
            settlementName = GenerateRandomName();
        }
        else
        {
            settlementName = name;
        }
        
        // Position slightly above the province
        transform.position = province.transform.position + new Vector3(0, 0, -0.1f);
        
        // Link the settlement to the province
        //province.settlement = this;
        
        UpdateVisuals();
        
        // Setup debug visualization
        SetupDebugVisualization();
    }
    
    private string GenerateRandomName()
    {
        string[] prefixes = { "New ", "Old ", "Fort ", "Port ", "North ", "East ", "West ", "South " };
        string[] names = { "Haven", "York", "Dale", "Ridge", "Falls", "River", "Field", "Bridge", "Town", "Spring" };
        
        string prefix = Random.value > 0.5f ? prefixes[Random.Range(0, prefixes.Length)] : "";
        string name = names[Random.Range(0, names.Length)];
        
        return prefix + name;
    }
    
    public void UpdateVisuals()
    {
        if (spriteRenderer == null) return;
        
        // Update sprite and scale based on size
        switch (size)
        {
            case Size.Village:
                spriteRenderer.color = villageColor;
                if (villageSprite != null) spriteRenderer.sprite = villageSprite;
                transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                populationCap = 500;
                resourceBonus = 5;
                break;
                
            case Size.Town:
                spriteRenderer.color = townColor;
                if (townSprite != null) spriteRenderer.sprite = townSprite;
                transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                populationCap = 1500;
                resourceBonus = 10;
                break;
                
            case Size.City:
                spriteRenderer.color = cityColor;
                if (citySprite != null) spriteRenderer.sprite = citySprite;
                transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                populationCap = 3000;
                resourceBonus = 20;
                break;
                
            case Size.Metropolis:
                spriteRenderer.color = metropolisColor;
                if (metropolisSprite != null) spriteRenderer.sprite = metropolisSprite;
                transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                populationCap = 10000;
                resourceBonus = 40;
                break;
        }
        
        // Update debug text
        UpdateDebugText();
    }
    
    // Process population growth and check for settlement upgrades
    public void ProcessTurn()
    {
        // Calculate population growth
        int growthAmount = Mathf.RoundToInt(population * growthRate);
        population = Mathf.Min(population + growthAmount, populationCap);
        
        // Check for upgrade conditions
        if (population >= populationCap && size != Size.Metropolis)
        {
            UpgradeSettlement();
        }
        
        // Update resource production
        UpdateResourceProduction();
        
        // Update debug visualization after changes
        UpdateDebugText();
    }
    
    private void UpgradeSettlement()
    {
        switch (size)
        {
            case Size.Village:
                size = Size.Town;
                Debug.Log($"{settlementName} has grown to a Town!");
                break;
                
            case Size.Town:
                size = Size.City;
                Debug.Log($"{settlementName} has grown to a City!");
                break;
                
            case Size.City:
                size = Size.Metropolis;
                Debug.Log($"{settlementName} has grown to a Metropolis!");
                break;
        }
        
        UpdateVisuals();
    }
    
    private void UpdateResourceProduction()
    {
        // Base resource bonus scales with population
        float populationScale = Mathf.Clamp01((float)population / populationCap);
        
        switch (size)
        {
            case Size.Village:
                resourceBonus = Mathf.RoundToInt(5 + (5 * populationScale));
                break;
                
            case Size.Town:
                resourceBonus = Mathf.RoundToInt(10 + (10 * populationScale));
                break;
                
            case Size.City:
                resourceBonus = Mathf.RoundToInt(20 + (20 * populationScale));
                break;
                
            case Size.Metropolis:
                resourceBonus = Mathf.RoundToInt(40 + (40 * populationScale));
                break;
        }
    }
    
    // Debug visualization methods
    private void SetupDebugVisualization()
    {
        if (!showDebugInfo) return;
        
        // Create text for settlement name if it doesn't exist
        if (nameText == null)
        {
            GameObject nameObj = new GameObject("SettlementName");
            nameObj.transform.SetParent(transform);
            nameObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            nameText = nameObj.AddComponent<TextMeshPro>();
            nameText.fontSize = 4;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.black;
        }
        
        // Create text for settlement size if it doesn't exist
        if (sizeText == null)
        {
            GameObject sizeObj = new GameObject("SettlementSize");
            sizeObj.transform.SetParent(transform);
            sizeObj.transform.localPosition = new Vector3(0, 0.3f, 0);
            
            sizeText = sizeObj.AddComponent<TextMeshPro>();
            sizeText.fontSize = 3;
            sizeText.alignment = TextAlignmentOptions.Center;
            sizeText.color = Color.blue;
        }
        
        // Update texts
        UpdateDebugText();
    }
    
    private void UpdateDebugText()
    {
        if (!showDebugInfo) return;
        
        if (nameText != null)
        {
            nameText.text = settlementName;
        }
        
        if (sizeText != null)
        {
            sizeText.text = $"{size} (Pop: {population})";
        }
    }
}