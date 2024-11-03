using UnityEngine;
using TMPro;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Health Settings")]
    public int startingHealth = 30; // Hearthstone default

    [Header("Game Over Settings")]
    public int victoryXP = 100;
    public int defeatXP = 25;

    [Header("UI References")]
    public TMP_Text playerHealthText;
    public TMP_Text opponentHealthText;

    private int playerHealth;
    private int opponentHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeHealth();
    }

    void InitializeHealth()
    {
        playerHealth = startingHealth;
        opponentHealth = startingHealth;
        UpdateHealthDisplay();
    }

    public void DealDamageToPlayer(int damage)
    {
        playerHealth = Mathf.Max(0, playerHealth - damage);
        UpdateHealthDisplay();
        CheckGameOver();
    }

    public void DealDamageToOpponent(int damage)
    {
        opponentHealth = Mathf.Max(0, opponentHealth - damage);
        UpdateHealthDisplay();
        CheckGameOver();
    }

    private void UpdateHealthDisplay()
    {
        if (playerHealthText != null)
        {
            playerHealthText.text = $"Health: {playerHealth}";
        }
        if (opponentHealthText != null)
        {
            opponentHealthText.text = $"Health: {opponentHealth}";
        }
    }

    private void CheckGameOver()
    {
        if (playerHealth <= 0)
        {
            EndGame(false);
        }
        else if (opponentHealth <= 0)
        {
            EndGame(true);
        }
    }

    private void EndGame(bool playerWon)
    {
        // Update player stats
        int xpGained = playerWon ? victoryXP : defeatXP;
        UserManager.Instance.UpdateUserStats(1, playerWon ? 1 : 0, playerWon ? 0 : 1);
        
        // Show game over UI
        GameOverUI.Instance.ShowGameOver(playerWon, xpGained);

        // Disable game interactions
        DisableGameplay();
    }

    private void DisableGameplay()
    {
        // Disable the end turn button
        if (TurnManager.Instance != null && TurnManager.Instance.endTurnButton != null)
        {
            TurnManager.Instance.endTurnButton.interactable = false;
        }

        // Disable card dragging
        Draggable[] draggables = FindObjectsOfType<Draggable>();
        foreach (Draggable drag in draggables)
        {
            drag.enabled = false;
        }
    }

    public bool IsGameActive()
    {
        return playerHealth > 0 && opponentHealth > 0;
    }
}