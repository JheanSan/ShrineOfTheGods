using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class OpponentAttackManager : MonoBehaviour
{
    private CardInteraction cardInteraction;
    private bool hasAttackedThisTurn = false;  // Add this to track if cards have attacked

    private void Start()
    {
        cardInteraction = FindObjectOfType<CardInteraction>();
    }

    public void PerformAttacks()
    {
        // Get all opponent cards currently on the field
        List<CardDisplay> opponentFieldCards = GetOpponentFieldCards();
        List<CardDisplay> playerCards = GetPlayerFieldCards();

        // Only allow cards that are actually on the field to attack
        foreach (CardDisplay attackerCard in opponentFieldCards)
        {
            // Randomly decide whether to attack a card or the player directly
            if (playerCards.Count > 0 && Random.value > 0.3f) // 70% chance to attack cards if available
            {
                // Pick a random player card to attack
                int randomIndex = Random.Range(0, playerCards.Count);
                CardDisplay targetCard = playerCards[randomIndex];

                // Perform the attack
                cardInteraction.InteractCards(attackerCard, targetCard);
                
                // Remove destroyed cards from the list
                playerCards = GetPlayerFieldCards(); // Refresh the list after combat
            }
            else
            {
                // Attack player directly
                HealthManager.Instance.DealDamageToPlayer(attackerCard.card.Attack);
                Debug.Log($"Opponent's {attackerCard.card.Name} attacks player directly for {attackerCard.card.Attack} damage!");
            }

            // Add a small delay between attacks for visual clarity
            // Note: In Unity, you'd typically use Coroutines for this
            System.Threading.Thread.Sleep(500);
        }
    }

    // Updated to only get cards that are children of the OpponentFieldArea
    private List<CardDisplay> GetOpponentFieldCards()
    {
        return GameObject.Find("OpponentFieldArea")
            .GetComponentsInChildren<CardDisplay>()
            .Where(cd => cd.isOpponentCard && cd.transform.parent.name == "OpponentFieldArea")
            .ToList();
    }

    private List<CardDisplay> GetPlayerFieldCards()
    {
        return GameObject.Find("PlayerFieldArea")
            .GetComponentsInChildren<CardDisplay>()
            .Where(cd => !cd.isOpponentCard && cd.transform.parent.name == "PlayerFieldArea")
            .ToList();
    }

    // Add this method to reset attack status at the start of turn
    public void ResetAttackStatus()
    {
        hasAttackedThisTurn = false;
    }
}