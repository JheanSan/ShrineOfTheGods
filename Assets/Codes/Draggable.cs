using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;
    private CardDisplay cardDisplay;
    private bool isDragging = false;
    private CardInteraction cardInteraction;
    private Canvas canvas;
    private Transform originalParent;

    private void Awake()
    {
        cardDisplay = GetComponent<CardDisplay>();
        cardInteraction = FindObjectOfType<CardInteraction>();
        canvas = FindObjectOfType<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardDisplay.isOpponentCard || TurnManager.Instance.CurrentTurn != TurnManager.PlayerTurn.Player)
        {
            return;
        }

        originalPosition = transform.position;
        originalParent = transform.parent;
        isDragging = true;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            transform.position = canvas.transform.TransformPoint(localPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        // Calculate mana cost considering Air element
        int manaCost = cardDisplay.card.ManaCost;
        if (cardDisplay.card.Element == "Air" && !cardDisplay.card.HasUsedElementalPower)
        {
            manaCost = 0;
            cardDisplay.card.HasUsedElementalPower = true;
            Debug.Log($"Air card {cardDisplay.card.Name} played for free!");
        }

        // Check for opponent portrait attack
        OpponentPortrait opponentPortrait = GetOpponentPortraitUnderMouse(eventData);
        if (opponentPortrait != null)
        {
            if (ManaManager.Instance.UseMana(manaCost))
            {
                HealthManager.Instance.DealDamageToOpponent(cardDisplay.card.Attack);
                Debug.Log($"{cardDisplay.card.Name} attacks opponent directly for {cardDisplay.card.Attack} damage");
                transform.position = originalPosition;
                transform.SetParent(originalParent);
            }
            else
            {
                transform.position = originalPosition;
                transform.SetParent(originalParent);
            }
            return;
        }

        // Check for card-to-card combat
        CardDisplay targetCard = GetCardUnderMouse(eventData);
        if (targetCard != null && targetCard.isOpponentCard)
        {
            if (ManaManager.Instance.UseMana(manaCost))
            {
                cardInteraction.InteractCards(cardDisplay, targetCard);
                transform.position = originalPosition;
                transform.SetParent(originalParent);
            }
            else
            {
                transform.position = originalPosition;
                transform.SetParent(originalParent);
            }
            return;
        }

        // If the card was in hand and is being played to the field
        if (originalParent == PlayerManager.Instance.playerHandArea)
        {
            if (ManaManager.Instance.CurrentMana >= manaCost)
            {
                transform.SetParent(PlayerManager.Instance.playerFieldArea);
                PlayerManager.Instance.MoveCardToField(cardDisplay);
                ManaManager.Instance.UseMana(manaCost);
            }
            else
            {
                ManaManager.Instance.UseMana(manaCost); // This will trigger the "Not enough mana!" message
                transform.position = originalPosition;
                transform.SetParent(originalParent);
            }
        }
    }

    private CardDisplay GetCardUnderMouse(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            CardDisplay cardDisplay = result.gameObject.GetComponent<CardDisplay>();
            if (cardDisplay != null && cardDisplay != this.cardDisplay)
            {
                return cardDisplay;
            }
        }

        return null;
    }

    private OpponentPortrait GetOpponentPortraitUnderMouse(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            OpponentPortrait portrait = result.gameObject.GetComponent<OpponentPortrait>();
            if (portrait != null)
            {
                return portrait;
            }
        }

        return null;
    }
}