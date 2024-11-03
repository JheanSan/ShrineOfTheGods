using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CardDisplay : MonoBehaviour
{
    public TMP_Text cardNameText;
    public TMP_Text cardStatsText;
    public TMP_Text manaCostText;
    public TMP_Text elementText;
    public TMP_Text loreText;
    public Card card;
    private Button button;
    private InfoPanelManager infoPanelManager;
    public Image cardArtwork; 
    
    public bool isOpponentCard = false;
    public Vector3 startPosition;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        button.onClick.AddListener(OnCardClicked);

        infoPanelManager = FindObjectOfType<InfoPanelManager>();

        startPosition = transform.position;
    }

   public void SetCard(Card newCard, bool isOpponent = false)
    {
        card = newCard;
        isOpponentCard = isOpponent;
        
        // Load and set the card artwork
        if (!string.IsNullOrEmpty(card.ImagePath))
        {
            // Load the sprite from your Resources folder
            Sprite artwork = Resources.Load<Sprite>(card.ImagePath);
            if (artwork != null && cardArtwork != null)
            {
                cardArtwork.sprite = artwork;
            }
        }
        
        UpdateCardDisplay();
    }

 public void UpdateCardDisplay()
    {
        if (cardNameText != null)
            cardNameText.text = card.Name;
        if (cardStatsText != null)
            cardStatsText.text = $"{card.Attack} | {card.CurrentDefense}/{card.MaxDefense}";
        if (manaCostText != null)
            manaCostText.text = card.ManaCost.ToString();
        if (elementText != null)
            elementText.text = card.Element;
        if (loreText != null)
            loreText.text = card.Lore;

        UpdateVisual();
    }
private void OnCardClicked()
{
    string currentSceneName = SceneManager.GetActiveScene().name;

    if (currentSceneName == "ManageDeckScene")
    {
        // Existing deck building logic
        if (PlayerDeck.Instance.IsCardInDeck(card))
        {
            PlayerDeck.Instance.RemoveCard(card);
            Debug.Log($"Removed {card.Name} from the player's deck");
        }
        else
        {
            bool added = PlayerDeck.Instance.AddCard(card);
            if (added)
                Debug.Log($"Added {card.Name} to the player's deck");
            else
                Debug.Log("Failed to add card to the deck (deck might be full or card already exists)");
        }
    }
    else if (currentSceneName == "PlayScene" && !isOpponentCard)
    {
        // New play logic
        PlayerManager.Instance.PlayCard(this);
    }

    UpdateVisual();

    if (infoPanelManager != null)
        infoPanelManager.UpdateInfoPanel(card);
}

    public void UpdateVisual()
    {
        bool isInPlayerDeck = PlayerDeck.Instance.IsCardInDeck(card);
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "ManageDeckScene")
        {
            Color buttonColor = isInPlayerDeck ? Color.green : Color.white;
            button.image.color = buttonColor;
        }
        else if (currentSceneName == "PlayScene")
        {
            button.image.color = Color.white;
        }

        button.interactable = !isOpponentCard;
    }

    public void ResetToStartPosition()
    {
        transform.position = startPosition;
    }



    public void RemoveFromPlay()
    {
        gameObject.SetActive(false);
        Debug.Log($"{card.Name} has been removed from play");
        // Additional logic for removing the card from play:
        // - Inform game manager
        // - Move to "graveyard"
        // - Trigger "on destroy" effects
    }
}

