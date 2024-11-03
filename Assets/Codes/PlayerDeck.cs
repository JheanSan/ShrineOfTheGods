using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerDeck : MonoBehaviour
{
    private static PlayerDeck _instance;
    public static PlayerDeck Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerDeck>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerDeck");
                    _instance = go.AddComponent<PlayerDeck>();
                }
            }
            return _instance;
        }
    }

    private List<Card> deck = new List<Card>();
    private List<Card> allCards = new List<Card>();
    private const int MAX_DECK_SIZE = 8;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void InitializeAllCards(List<Card> cards)
    {
        allCards = cards;
    }

    public bool AddCard(Card card)
    {
        if (deck.Count < MAX_DECK_SIZE && !IsCardInDeck(card))
        {
            deck.Add(card);
            Debug.Log($"Added {card.Name} to the deck. Deck size: {deck.Count}");
            return true;
        }
        else
        {
            Debug.Log("Deck is full or card already exists. Cannot add card.");
            return false;
        }
    }

    public void RemoveCard(Card card)
    {
        Card cardToRemove = deck.FirstOrDefault(c => c.Name == card.Name);
        if (cardToRemove != null)
        {
            deck.Remove(cardToRemove);
            Debug.Log($"Removed {card.Name} from the deck. Deck size: {deck.Count}");
        }
        else
        {
            Debug.Log($"Card {card.Name} not found in the deck.");
        }
    }

    public List<Card> GetDeck()
    {
        return new List<Card>(deck);
    }

    public List<Card> GetAllCards()
    {
        return new List<Card>(allCards);
    }

    public bool IsCardInDeck(Card card)
    {
        return deck.Any(c => c.Name == card.Name);
    }
}