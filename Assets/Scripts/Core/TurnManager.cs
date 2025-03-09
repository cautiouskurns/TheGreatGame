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
    
    void Start()
    {
        // Find all nations if not assigned
        if (nations.Count == 0)
        {
            nations.AddRange(FindObjectsByType<Nation>(FindObjectsSortMode.None));
        }
        
        // Distribute initial provinces
        AssignInitialProvinces();
        
        // Start first turn
        StartTurn();
    }
    
    private void AssignInitialProvinces()
    {
        MapGenerator mapGen = FindAnyObjectByType<MapGenerator>();
        if (mapGen == null || mapGen.provinces == null) return;
        
        // Give first nation provinces on the left side
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < mapGen.provinces.GetLength(1); y++)
            {
                nations[0].AddProvince(mapGen.provinces[x, y]);
            }
        }
        
        // Give second nation provinces on the right side
        int width = mapGen.provinces.GetLength(0);
        for (int x = width - 3; x < width; x++)
        {
            for (int y = 0; y < mapGen.provinces.GetLength(1); y++)
            {
                nations[1].AddProvince(mapGen.provinces[x, y]);
            }
        }
    }
    
    private void StartTurn()
    {
        Debug.Log($"Starting turn for {CurrentNation.nationName}");
        
        // Regenerate resources in all provinces
        RegenerateAllProvinces();
        
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
}