using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public enum PlayerTurn { Player, Opponent }
    public PlayerTurn CurrentTurn { get; private set; }
    public Button endTurnButton;
    public TMP_Text turnDisplayText;
    private int roundNumber = 0;

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

    private void Start()
    {
        endTurnButton.onClick.AddListener(EndTurn);
        DetermineFirstPlayer();
    }

    private void DetermineFirstPlayer()
    {
        CurrentTurn = Random.value > 0.5f ? PlayerTurn.Player : PlayerTurn.Opponent;
        UpdateTurnDisplay();
        
        StartNewRound();
    }

    private void EndTurn()
    {
        if (CurrentTurn == PlayerTurn.Player)
        {
            CurrentTurn = PlayerTurn.Opponent;
            StartOpponentTurn();
        }
        else
        {
            StartNewRound();
        }
        UpdateTurnDisplay();
    }

    private void StartNewRound()
    {
        roundNumber++;
        CurrentTurn = PlayerTurn.Player;
        ManaManager.Instance.IncreaseManaPool();
        StartPlayerTurn();
    }

private void StartPlayerTurn()
{
    Debug.Log($"Player's turn starts (Round {roundNumber})");
    ManaManager.Instance.ResetMana();
    endTurnButton.interactable = true;
    PlayerManager.Instance.DrawCard(); // Draw a card at the start of turn
}

    private void StartOpponentTurn()
    {
        Debug.Log($"Opponent's turn starts (Round {roundNumber})");
        ManaManager.Instance.ResetMana();
        endTurnButton.interactable = false;

        // Play the opponent's turn
        OpponentManager.Instance.PlayTurn();

        // End the opponent's turn after a delay
        Invoke(nameof(EndTurn), 2f);
    }

    private void UpdateTurnDisplay()
    {
        turnDisplayText.text = $"Turn: {CurrentTurn}";
    }
}