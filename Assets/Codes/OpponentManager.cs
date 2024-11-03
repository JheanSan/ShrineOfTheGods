using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OpponentManager : MonoBehaviour
{
    public static OpponentManager Instance { get; private set; }

    public GameObject cardPrefab;
    public Transform opponentHandArea;
    public Transform opponentFieldArea;
    public int startingHandSize = 4;
    public int maxHandSize = 8;

    // Animation settings
    [Header("Animation Settings")]
    public float actionDelay = 0.5f;         // Delay between actions
    public float cardPlayDuration = 0.3f;    // How long it takes to play a card
    public float cardDrawDuration = 0.2f;    // How long it takes to draw a card

    private List<Card> opponentDeck = new List<Card>();
    private List<CardDisplay> opponentHand = new List<CardDisplay>();
    private OpponentAttackManager attackManager;

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
        attackManager = GetComponent<OpponentAttackManager>();
    }

    void Start()
    {
        InitializeOpponentDeck();
        DrawInitialHand();
    }

    void InitializeOpponentDeck()
    {
        List<Card> allCards = PlayerDeck.Instance.GetAllCards();
        
        for (int i = 0; i < 30; i++)
        {
            int randomIndex = Random.Range(0, allCards.Count);
            opponentDeck.Add(allCards[randomIndex]);
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
        if (opponentDeck.Count > 0 && opponentHand.Count < maxHandSize)
        {
            int randomIndex = Random.Range(0, opponentDeck.Count);
            Card drawnCard = opponentDeck[randomIndex].Clone();
            opponentDeck.RemoveAt(randomIndex);
            
            GameObject cardObject = Instantiate(cardPrefab, opponentHandArea);
            int index = opponentHand.Count;
            float spacing = 100f;
            
            // Set initial position slightly above the hand area
            cardObject.transform.localPosition = new Vector3(index * spacing, 50f, 0);
            
            CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
            cardDisplay.SetCard(drawnCard, true);
            cardDisplay.UpdateVisual();
            
            // Animate the card moving into position
            StartCoroutine(AnimateCardDraw(cardObject.transform, new Vector3(index * spacing, 0, 0)));
            
            opponentHand.Add(cardDisplay);
        }
    }

    private IEnumerator AnimateCardDraw(Transform cardTransform, Vector3 targetPosition)
    {
        Vector3 startPos = cardTransform.localPosition;
        float elapsedTime = 0;

        while (elapsedTime < cardDrawDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / cardDrawDuration;
            cardTransform.localPosition = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        cardTransform.localPosition = targetPosition;
    }

    public void PlayTurn()
    {
        StartCoroutine(PlayTurnSequence());
    }

    private IEnumerator PlayTurnSequence()
    {
        // Draw a card
        DrawCard();
        yield return new WaitForSeconds(actionDelay);

        // Play cards
        while (CanPlayCard())
        {
            yield return PlayRandomCard();
            yield return new WaitForSeconds(actionDelay);
        }

        // Wait before attacks
        yield return new WaitForSeconds(actionDelay);
        
        // Perform attacks
        attackManager.PerformAttacks();
    }

    private bool CanPlayCard()
    {
        if (opponentHand.Count == 0) return false;
        int availableMana = ManaManager.Instance.CurrentMana;
        return opponentHand.Any(cardDisplay => cardDisplay.card.ManaCost <= availableMana);
    }

    private IEnumerator PlayRandomCard()
    {
        int availableMana = ManaManager.Instance.CurrentMana;
        List<CardDisplay> playableCards = opponentHand
            .Where(cardDisplay => cardDisplay.card.ManaCost <= availableMana)
            .ToList();

        if (playableCards.Count > 0)
        {
            int randomIndex = Random.Range(0, playableCards.Count);
            CardDisplay cardToPlay = playableCards[randomIndex];

            // Store the original position
            Vector3 startPosition = cardToPlay.transform.position;
            
            // Calculate target position on the field
            float spacing = 120f; // Adjust based on your card width
            int fieldPosition = opponentFieldArea.childCount;
            Vector3 targetPosition = opponentFieldArea.TransformPoint(new Vector3(fieldPosition * spacing, 0, 0));

            // Remove from hand immediately to prevent duplicate plays
            opponentHand.Remove(cardToPlay);

            // Animate card movement
            float elapsedTime = 0;
            Vector3 midPoint = Vector3.Lerp(startPosition, targetPosition, 0.5f) + (Vector3.up * 50f); // Arc midpoint

            while (elapsedTime < cardPlayDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / cardPlayDuration;
                
                // Create an arc motion using quadratic Bezier curve
                Vector3 m1 = Vector3.Lerp(startPosition, midPoint, t);
                Vector3 m2 = Vector3.Lerp(midPoint, targetPosition, t);
                cardToPlay.transform.position = Vector3.Lerp(m1, m2, t);
                
                yield return null;
            }

            // Ensure final position is exact
            cardToPlay.transform.position = targetPosition;
            cardToPlay.transform.SetParent(opponentFieldArea);

            // Use mana after successful play
            ManaManager.Instance.UseMana(cardToPlay.card.ManaCost);

            Debug.Log($"Opponent played {cardToPlay.card.Name} (Cost: {cardToPlay.card.ManaCost})");
        }
    }
}