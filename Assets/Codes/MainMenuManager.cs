using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Add this for UI components

public class MainMenuManager : MonoBehaviour
{
    public Button logoutButton; // Reference to the logout button in the Unity Inspector

    private void Start()
    {
        // Ensure the logout button is assigned and add a listener
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(Logout);
        }
        else
        {
            Debug.LogWarning("Logout button is not assigned in the MainMenuManager.");
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("PlayScene");
    }

    public void ManageDeck()
    {
        SceneManager.LoadScene("ManageDeckScene");
    }

    public void ViewProfile()
    {
        SceneManager.LoadScene("ViewProfileScene");
    }

    public void Logout()
    {
        // Call the Logout method from UserManager
        UserManager.Instance.Logout();
        // The UserManager will handle redirecting to the LoginScene
    }
}