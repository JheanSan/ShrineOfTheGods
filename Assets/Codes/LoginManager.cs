using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public TMP_Text messageText;

    void Start()
    {
        // Check if UserManager instance exists
        if (UserManager.Instance == null)
        {
            Debug.LogError("UserManager instance is null. Ensure it's properly initialized.");
            return;
        }

        // Ensure UI elements are assigned
        if (loginButton != null)
            loginButton.onClick.AddListener(HandleLogin);
        else
            Debug.LogWarning("Login button is not assigned in the Inspector.");

        if (registerButton != null)
            registerButton.onClick.AddListener(HandleRegister);
        else
            Debug.LogWarning("Register button is not assigned in the Inspector.");

        // Check if user is already logged in
        if (UserManager.Instance.GetCurrentUser() != null)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    void HandleLogin()
    {
        if (usernameInput == null || passwordInput == null)
        {
            Debug.LogError("Username or password input field is not assigned.");
            return;
        }

        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetMessage("Please enter both username and password.");
            return;
        }

        if (UserManager.Instance.Login(username, password))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            SetMessage("Invalid username or password.");
        }
    }

    void HandleRegister()
    {
        if (usernameInput == null || passwordInput == null)
        {
            Debug.LogError("Username or password input field is not assigned.");
            return;
        }

        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetMessage("Please enter both username and password.");
            return;
        }

        if (UserManager.Instance.Register(username, password))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            SetMessage("Username already exists. Please choose a different one.");
        }
    }

    void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
        else
            Debug.LogWarning("Message text is not assigned. Message: " + message);
    }
}