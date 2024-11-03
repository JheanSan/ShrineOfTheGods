using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class PlayerInfoDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text usernameText;
    public Image profileImage;

    void Start()
    {
        LoadPlayerInfo();
    }

    void LoadPlayerInfo()
    {
        User currentUser = UserManager.Instance.GetCurrentUser();
        if (currentUser == null)
        {
            Debug.LogError("No user logged in!");
            return;
        }

        // Set username
        if (usernameText != null)
        {
            usernameText.text = currentUser.Username;
        }

        // Load profile image
        if (profileImage != null)
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
    }
}