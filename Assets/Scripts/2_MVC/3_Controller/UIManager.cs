// UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentTurnText;
    [SerializeField] private TextMeshProUGUI playerGoldText;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button collectResourcesButton;
    [SerializeField] private Button zoomInButton;
    [SerializeField] private Button zoomOutButton;
    
    private TurnManager turnManager;
    
    void Start()
    {
        turnManager = FindAnyObjectByType<TurnManager>();
        
        // Subscribe to turn changed event
        if (turnManager != null)
        {
            turnManager.OnTurnChanged += UpdateUI;
        }
        
        // Set up button events
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(() => {
                GameController.Instance.EndPlayerTurn();
            });
        }
        
        if (collectResourcesButton != null)
        {
            collectResourcesButton.onClick.AddListener(() => {
                GameController.Instance.CollectResources();
                UpdateUI(turnManager.CurrentNation);
            });
        }
        
        // Set up zoom buttons
        CameraController cameraController = FindAnyObjectByType<CameraController>();
        
        if (zoomInButton != null && cameraController != null)
        {
            zoomInButton.onClick.AddListener(() => {
                cameraController.ZoomIn();
            });
        }
        
        if (zoomOutButton != null && cameraController != null)
        {
            zoomOutButton.onClick.AddListener(() => {
                cameraController.ZoomOut();
            });
        }
        
        // Initial UI update
        if (turnManager != null)
        {
            UpdateUI(turnManager.CurrentNation);
        }
    }
    
    private void UpdateUI(Nation currentNation)
    {
        if (currentTurnText != null)
        {
            currentTurnText.text = $"Current Turn: {currentNation.nationName}";
        }
        
        if (playerGoldText != null && currentNation.isPlayerControlled)
        {
            playerGoldText.text = $"Gold: {currentNation.gold}";
        }
        
        // Enable/disable buttons based on player turn
        bool isPlayerTurn = turnManager.IsPlayerTurn;
        if (endTurnButton != null) endTurnButton.interactable = isPlayerTurn;
        if (collectResourcesButton != null) collectResourcesButton.interactable = isPlayerTurn;
    }
}