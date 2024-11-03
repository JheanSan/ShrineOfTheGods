using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ProfileScreen : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text usernameText;
    public Image profileImage;
    public Transform deckContainer;
    public GameObject cardPrefab;
    public Button backButton;
    public Button changePictureButton;

    [Header("Stats Display")]
    public TMP_Text gamesPlayedText;
    public TMP_Text winsText;
    public TMP_Text lossesText;
    public TMP_Text winRateText;

    private User currentUser;

    void Start()
    {
        currentUser = UserManager.Instance.GetCurrentUser();
        if (currentUser == null)
        {
            Debug.LogError("No user logged in. This should not happen.");
            SceneManager.LoadScene("LoginScene");
            return;
        }

        SetupUI();
        DisplayUserStats();
    }

    void SetupUI()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        changePictureButton.onClick.AddListener(ChangePicture);

        usernameText.text = "Username: " + currentUser.Username;

        LoadProfileImage();
        DisplayDeck();
    }

    void LoadProfileImage()
    {
        string imagePath = PlayerPrefs.GetString(currentUser.Username + "_ProfileImagePath", "");
        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            byte[] imageData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageData))
            {
                profileImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        else
        {
            profileImage.sprite = Resources.Load<Sprite>("DefaultProfileImage");
        }
    }

    void ChangePicture()
    {
        #if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Profile Picture", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageData))
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    profileImage.sprite = sprite;
                    PlayerPrefs.SetString(currentUser.Username + "_ProfileImagePath", path);
                    PlayerPrefs.Save();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading image: " + e.Message);
            }
        }
        #else
        Debug.Log("Changing picture is only supported in the Unity Editor for this example.");
        #endif
    }

    void DisplayDeck()
    {
        foreach (Transform child in deckContainer)
        {
            Destroy(child.gameObject);
        }

        List<Card> playerDeck = PlayerDeck.Instance.GetDeck();

        if (playerDeck.Count == 0)
        {
            TMP_Text emptyDeckText = Instantiate(new GameObject("EmptyDeckText", typeof(TMP_Text)), deckContainer).GetComponent<TMP_Text>();
            emptyDeckText.text = "Your deck is empty. Go to Manage Deck to add cards!";
            emptyDeckText.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            foreach (Card card in playerDeck)
            {
                GameObject cardObject = Instantiate(cardPrefab, deckContainer);
                CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
                if (cardDisplay != null)
                {
                    cardDisplay.SetCard(card);
                }
            }
        }
    }

    void DisplayUserStats()
    {
        UserStats stats = currentUser.Stats;
        gamesPlayedText.text = $"Games Played: {stats.GamesPlayed}";
        winsText.text = $"Wins: {stats.Wins}";
        lossesText.text = $"Losses: {stats.Losses}";
        
        float winRate = stats.GamesPlayed > 0 ? (float)stats.Wins / stats.GamesPlayed * 100 : 0;
        winRateText.text = $"Win Rate: {winRate:F1}%";
    }
}