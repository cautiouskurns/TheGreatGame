using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Game/Building")]
public class Building : ScriptableObject
{
    [Header("Basic Info")]
    public string buildingName;
    public string description;
    public Sprite icon;
    
    [Header("Requirements")]
    public SettlementType minimumSettlementType;
    public int productionCost = 50;
    public int goldCost = 50;
    
    [Header("Production")]
    public int goldProduction = 0;
    public int foodProduction = 0;
    public int productionProduction = 0;
    
    [Header("Bonuses")]
    public float populationGrowthModifier = 0f;
    public int populationCapIncrease = 0;
    
    // Called each turn to update any time-based effects
    public virtual void ProcessTurn()
    {
        // Base implementation does nothing; can be overridden for special buildings
    }
}