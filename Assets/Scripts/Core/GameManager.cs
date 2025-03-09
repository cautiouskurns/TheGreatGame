// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private MapGenerator mapGenerator;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Find required components if not assigned
        if (turnManager == null) turnManager = FindAnyObjectByType<TurnManager>();
        if (mapGenerator == null) mapGenerator = FindAnyObjectByType<MapGenerator>();
    }
    
    // Method for the player to claim a province
    public bool ClaimProvince(Province province)
    {
        if (!turnManager.IsPlayerTurn) return false;
        
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
        if (!turnManager.IsPlayerTurn) return;
        
        turnManager.CurrentNation.CollectAllResources();
    }
    
    // Method for the player to end their turn
    public void EndPlayerTurn()
    {
        if (turnManager.IsPlayerTurn)
        {
            turnManager.EndTurn();
        }
    }
    
    private bool IsProvinceAdjacentToNation(Province province, Nation nation)
    {
        // Check all four adjacent tiles
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };
        
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