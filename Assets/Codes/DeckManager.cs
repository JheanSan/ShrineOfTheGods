using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class DeckManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform contentPanel;
    public Transform playerDeckPanel;
    public TMP_Text deckSizeText;

    private List<CardDisplay> allCardDisplays = new List<CardDisplay>();

    void Start()
    {
        InitializeCards();
        UpdatePlayerDeckDisplay();
    }

void InitializeCards()
{
    if (PlayerDeck.Instance.GetAllCards().Count == 0)
    {
        List<Card> allCards = new List<Card>
        {
           new Card("Dragon", "Fire", 3, 2, "A fearsome dragon that breathes fire. Its scales glisten like gold in the sunlight.", 3, "CardArt/dragon"),
           new Card("Warrior", "Earth", 4, 2, "A strong warrior with unbreakable will. They wield a sword and shield emblazoned with their family crest.", 4, "CardArt/warrior"),
           new Card("Mage", "Water", 2, 3, "A wise mage with powerful water spells. They carry a staff infused with ancient magic.", 4, "CardArt/mage"),
           new Card("Elf", "Air", 3, 2, "An agile elf with swift movement. They possess a bow and quiver full of enchanted arrows.", 3, "CardArt/elf"),
           new Card("Dwarf", "Earth", 5, 2, "A sturdy dwarf with unyielding defense. They wield a mighty axe and wear plate armor emblazoned with the emblem of their clan.", 3, "CardArt/dwarf"),
           new Card("Troll", "Fire", 5, 3, "A powerful troll with massive strength. They roam the countryside, searching for their next meal.", 4, "CardArt/troll"),
           new Card("Goblin", "Air", 3, 3, "A cunning goblin with quick reflexes. They are known for their mischievous tricks and love of shiny objects.", 2, "CardArt/goblin"),
           new Card("Wizard", "Water", 2, 4, "A powerful wizard with arcane magic. They carry a staff adorned with gems and wear a long, flowing beard.", 6, "CardArt/wizard"),
           new Card("Cyclops", "Earth", 4, 3, "A powerful Cyclops with enormous strength. They roam the countryside, searching for their next meal.", 5, "CardArt/cyclops"),
           new Card("Mermaid", "Water", 4, 2, "A beautiful mermaid with alluring songs. They sing to lure sailors to their doom.", 4, "CardArt/mermaid"),
           new Card("Gryphon", "Air", 4, 3, "A majestic gryphon with the body of a lion and the head and wings of an eagle. They soar the skies, searching for prey.", 5, "CardArt/gryphon"),
           new Card("Minotaur", "Earth", 5, 3, "A fearsome minotaur with a powerful charge. They roam the countryside, searching for their next meal.", 6, "CardArt/minotaur"),
           new Card("Unicorn", "Air", 4, 4, "A majestic unicorn with a shimmering coat and a horn that shines like a diamond. They possess magical powers.", 7, "CardArt/unicorn"),
           new Card("Yeti", "Earth", 4, 3, "A powerful yeti with thick, white fur. They roam the mountains, searching for their next meal.", 6, "CardArt/yeti"),
           new Card("Phoenix", "Fire", 5, 3, "A majestic phoenix with wings that shine like the sun. They possess the power of rebirth.", 2, "CardArt/phoenix"),
           new Card("Chimera", "Fire", 4, 4, "A fearsome chimera with the head of a lion, the body of a snake, and the tail of a scorpion. They breathe fire and poison.", 8, "CardArt/chimera"),
           new Card("Hydra", "Water", 5, 4, "A powerful hydra with multiple heads and a powerful bite. They roam the countryside, searching for their next meal.", 7, "CardArt/hydra"),
           new Card("Harpy", "Air", 5, 3, "A beautiful harpy with the body of a bird and the face of a woman. They possess the power of flight.", 8, "CardArt/harpy"),
           new Card("Gorgon", "Earth", 5, 4, "A fearsome gorgon with snakes for hair and a petrifying gaze. They can turn people to stone.", 8, "CardArt/gorgon"),
        };
        PlayerDeck.Instance.InitializeAllCards(allCards);
    }
    foreach (Card card in PlayerDeck.Instance.GetAllCards())
    {
        CreateCardUI(card, contentPanel);
    }
}




    void CreateCardUI(Card card, Transform parent)
    {
        GameObject cardUI = Instantiate(cardPrefab, parent);
        CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
        cardDisplay.SetCard(card);
        allCardDisplays.Add(cardDisplay);
    }

    public void UpdatePlayerDeckDisplay()
    {
        foreach (Transform child in playerDeckPanel)
        {
            Destroy(child.gameObject);
        }

        List<Card> playerCards = PlayerDeck.Instance.GetDeck();
        foreach (Card card in playerCards)
        {
            CreateCardUI(card, playerDeckPanel);
        }

        deckSizeText.text = $"Deck Size: {playerCards.Count}/8";

        // Update visuals for all cards
        foreach (CardDisplay cardDisplay in allCardDisplays)
        {
            cardDisplay.UpdateVisual();
        }
    }

 public void StartGame()
{
    if (PlayerDeck.Instance.GetDeck().Count == 8)
    {
        SceneManager.LoadScene("PlayScene");
    }
    else
    {
        Debug.Log("Please select exactly 8 cards before starting the game.");
    }
}


    void Update()
    {
        UpdatePlayerDeckDisplay();
    }
}