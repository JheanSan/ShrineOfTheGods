using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public Transform playerDeckPanel;
    public GameObject cardPrefab;

    void Start()
    {
        InitializePlayerDeck();
    }

    void InitializePlayerDeck()
    {
        foreach (Card card in PlayerDeck.Instance.GetDeck())
        {
            GameObject cardObject = Instantiate(cardPrefab, playerDeckPanel);
            CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
            cardDisplay.SetCard(card);
        }
    }
}
