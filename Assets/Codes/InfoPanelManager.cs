using UnityEngine;
using TMPro;
using UnityEngine.UI; // Add this for Image component

public class InfoPanelManager : MonoBehaviour
{
    public TMP_Text cardNameText;
    public TMP_Text cardStatsText;
    public TMP_Text cardLoreText;
    public Image cardImage; // Add this field for the card image

    public void UpdateInfoPanel(Card card)
    {
        if (card != null)
        {
            cardNameText.text = card.Name;
            
            // Updated to display CurrentDefense and MaxDefense
            cardStatsText.text = $"Attack: {card.Attack} | Defense: {card.CurrentDefense}/{card.MaxDefense} | Element: {card.Element}";
            
            cardLoreText.text = card.Lore;

            // Load and display the card image
            if (!string.IsNullOrEmpty(card.ImagePath))
            {
                Sprite cardSprite = Resources.Load<Sprite>(card.ImagePath);
                if (cardSprite != null && cardImage != null)
                {
                    cardImage.sprite = cardSprite;
                    cardImage.enabled = true;
                }
                else
                {
                    Debug.LogWarning($"Could not load image for card: {card.Name} at path: {card.ImagePath}");
                    cardImage.enabled = false;
                }
            }
            else
            {
                cardImage.enabled = false;
            }

            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}