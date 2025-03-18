// GameManager.cs
using UnityEngine;

public class GameController : MonoBehaviour
{
public static GameController Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<GameController>(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Method for the player to claim a province
    public bool ClaimProvince(Province province)
    {
        TurnManager turnManager = ServiceLocator.Get<TurnManager>();
        if (turnManager == null || !turnManager.IsPlayerTurn) return false;
        
        Nation playerNation = turnManager.CurrentNation;
        
        // Check if province is adjacent to player's territory
        bool isAdjacent = IsProvinceAdjacentToNation(province, playerNation);
        
        if (isAdjacent && province.ownerNation == null && playerNation.gold >= 50)
        {
            playerNation.gold -= 50;
            playerNation.AddProvince(province);
            Debug.Log($"Player claimed province for 50 gold");
            return true;
        }
        
        return false;
    }
    
    // Method for the player to collect resources
    public void CollectResources()
    {
        TurnManager turnManager = ServiceLocator.Get<TurnManager>();
        EconomyManager economyManager = ServiceLocator.Get<EconomyManager>();
        
        if (turnManager == null || !turnManager.IsPlayerTurn || economyManager == null) 
            return;
        
        economyManager.CollectNationResources(turnManager.CurrentNation);
    }
    
    // Method for the player to end their turn
    public void EndPlayerTurn()
    {
        TurnManager turnManager = ServiceLocator.Get<TurnManager>();
        
        if (turnManager != null && turnManager.IsPlayerTurn)
        {
            turnManager.EndTurn();
        }
    }
    
    private bool IsProvinceAdjacentToNation(Province province, Nation nation)
    {
        // Check all four adjacent tiles
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };
        
        MapGenerator mapGenerator = ServiceLocator.Get<MapGenerator>();
        if (mapGenerator == null || mapGenerator.provinces == null)
            return false;
        
        for (int i = 0; i < 4; i++)
        {
            int nx = province.x + dx[i];
            int ny = province.y + dy[i];
            
            // Check bounds
            if (nx >= 0 && nx < mapGenerator.provinces.GetLength(0) && 
                ny >= 0 && ny < mapGenerator.provinces.GetLength(1))
            {
                Province neighbor = mapGenerator.provinces[nx, ny];
                if (neighbor.ownerNation == nation)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}