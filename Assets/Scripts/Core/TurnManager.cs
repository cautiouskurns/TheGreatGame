// TurnManager.cs
using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private List<Nation> nations = new List<Nation>();
    private int currentNationIndex = 0;
    
    public Nation CurrentNation => nations[currentNationIndex];
    public bool IsPlayerTurn => CurrentNation.isPlayerControlled;
    
    // Events for UI updates
    public delegate void TurnChanged(Nation nation);
    public event TurnChanged OnTurnChanged;
    
    void Awake()
    {
        ServiceLocator.Register<TurnManager>(this);
    }
    
    void Start()
    {
        // Find all nations if not assigned
        if (nations.Count == 0)
        {
            nations.AddRange(FindObjectsByType<Nation>(FindObjectsSortMode.None));
        }
        
        // Start first turn
        StartTurn();
    }
    
    private void StartTurn()
    {
        Debug.Log($"Starting turn for {CurrentNation.nationName}");
        
        // Process economic systems using the dedicated manager
        EconomyManager economyManager = ServiceLocator.Get<EconomyManager>();
        if (economyManager != null)
        {
            economyManager.ProcessSettlements();
            economyManager.ProcessProvinceResources();
        }
        
        // If AI, execute its turn
        if (!IsPlayerTurn)
        {
            CurrentNation.ExecuteAITurn();
            EndTurn();
        }
        else
        {
            // Notify UI that player turn has started
            OnTurnChanged?.Invoke(CurrentNation);
        }
    }
    
    public void EndTurn()
    {
        // Move to next nation
        currentNationIndex = (currentNationIndex + 1) % nations.Count;
        
        // Start the next turn
        StartTurn();
    }
    
    private void RegenerateAllProvinces()
    {
        MapGenerator mapGen = FindAnyObjectByType<MapGenerator>();
        if (mapGen == null || mapGen.provinces == null) return;
        
        foreach (Province province in mapGen.provinces)
        {
            province.RegenerateResources();
        }
    }
    
    // Process growth and production for all settlements
    private void ProcessAllSettlements()
    {
        Settlement[] settlements = FindObjectsByType<Settlement>(FindObjectsSortMode.None);
        
        foreach (Settlement settlement in settlements)
        {
            settlement.ProcessTurn();
        }
        
        Debug.Log($"Processed {settlements.Length} settlements");
    }
}