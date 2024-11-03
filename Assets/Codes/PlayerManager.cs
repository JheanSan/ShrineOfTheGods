using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public GameObject cardPrefab;
    public Transform playerHandArea;
    public Transform playerFieldArea;
    public int startingHandSize = 4;
    public int maxHandSize = 8;

    private List<Card> playerDrawDeck = new List<Card>();
    private List<CardDisplay> playerHand = new List<CardDisplay>();
    private List<CardDisplay> playerField = new List<CardDisplay>(); // Track cards in play

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
        InitializePlayerDrawDeck();
        DrawInitialHand();
    }

    void InitializePlayerDrawDeck()
    {
        List<Card> playerDeck = PlayerDeck.Instance.GetDeck();
        for (int copies = 0; copies < 4; copies++)
        {
            foreach (Card card in playerDeck)
            {
                playerDrawDeck.Add(card);
            }
        }
    }

    void DrawInitialHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawCard();
        }
    }

 public void DrawCard()
    {
        if (playerDrawDeck.Count > 0 && playerHand.Count < maxHandSize)
        {
            int randomIndex = Random.Range(0, playerDrawDeck.Count);
            // Create a fresh copy of the card
            Card drawnCard = playerDrawDeck[randomIndex].Clone();
            playerDrawDeck.RemoveAt(randomIndex);
            
            GameObject cardObject = Instantiate(cardPrefab, playerHandArea);
            CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
            cardDisplay.SetCard(drawnCard, false);
            cardDisplay.UpdateVisual();
            playerHand.Add(cardDisplay);
            RearrangeHand();
            
            Debug.Log($"Player drew {drawnCard.Name}. Cards in hand: {playerHand.Count}");
        }
    }

    public void PlayCard(CardDisplay cardDisplay)
    {
        if (ManaManager.Instance.CurrentMana >= cardDisplay.card.ManaCost)
        {
            // Remove from hand and add to field
            playerHand.Remove(cardDisplay);
            playerField.Add(cardDisplay);

            // Use mana
            ManaManager.Instance.UseMana(cardDisplay.card.ManaCost);

            // Draw a new card
            DrawCard();

            Debug.Log($"Player played {cardDisplay.card.Name} (Cost: {cardDisplay.card.ManaCost})");
        }
        else
        {
            Debug.Log($"Not enough mana to play {cardDisplay.card.Name}");
        }
    }

    public void RearrangeHand()
    {
        // Only rearrange cards that are actually in the hand
        for (int i = 0; i < playerHand.Count; i++)
        {
            if (playerHand[i] != null && playerHand[i].transform.parent == playerHandArea)
            {
                float spacing = 100f;
                playerHand[i].transform.localPosition = new Vector3(i * spacing, 0, 0);
            }
        }
    }

    // Call this when a card is moved to the field via dragging
    public void MoveCardToField(CardDisplay cardDisplay)
    {
        if (playerHand.Contains(cardDisplay))
        {
            playerHand.Remove(cardDisplay);
            playerField.Add(cardDisplay);
            RearrangeHand(); // Rearrange remaining hand cards
        }
    }

    // Call this when a card is destroyed or removed from play
    public void RemoveCardFromField(CardDisplay cardDisplay)
    {
        playerField.Remove(cardDisplay);
    }
}