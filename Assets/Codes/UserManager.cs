using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class User
{
    public string Username;
    public string Password;
    public UserStats Stats;

    public User(string username, string password)
    {
        Username = username;
        Password = password;
        Stats = new UserStats();
    }
}

[System.Serializable]
public class UserStats
{
    public int GamesPlayed;
    public int Wins;
    public int Losses;
}

[System.Serializable]
public class UserList
{
    public List<User> users = new List<User>();
}

public class UserManager : MonoBehaviour
{
    private static UserManager _instance;
    public static UserManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("UserManager");
                _instance = go.AddComponent<UserManager>();
            }
            return _instance;
        }
    }

    private UserList userList = new UserList();
    private User currentUser;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadUsers();
        AttemptAutoLogin();

        // Subscribe to scene loading event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loading event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LoadUsers()
    {
        string json = PlayerPrefs.GetString("UserData", JsonUtility.ToJson(new UserList()));
        userList = JsonUtility.FromJson<UserList>(json);
    }

    private void SaveUsers()
    {
        string json = JsonUtility.ToJson(userList);
        PlayerPrefs.SetString("UserData", json);
        PlayerPrefs.Save();
    }

    private void AttemptAutoLogin()
    {
        string savedUsername = PlayerPrefs.GetString("LastLoggedInUser", "");
        if (!string.IsNullOrEmpty(savedUsername))
        {
            User user = userList.users.Find(u => u.Username == savedUsername);
            if (user != null)
            {
                currentUser = user;
                Debug.Log($"Auto-logged in as {currentUser.Username}");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "LoginScene" && currentUser == null)
        {
            Debug.Log("No user logged in. Redirecting to Login Scene.");
            SceneManager.LoadScene("LoginScene");
        }
    }

    public bool Register(string username, string password)
    {
        if (userList.users.Exists(u => u.Username == username))
        {
            Debug.Log("Username already exists");
            return false;
        }

        User newUser = new User(username, password);
        userList.users.Add(newUser);
        SaveUsers();
        Login(username, password);
        return true;
    }

    public bool Login(string username, string password)
    {
        User user = userList.users.Find(u => u.Username == username && u.Password == password);
        if (user != null)
        {
            currentUser = user;
            PlayerPrefs.SetString("LastLoggedInUser", username);
            PlayerPrefs.Save();
            Debug.Log($"Logged in as {username}");
            return true;
        }
        Debug.Log("Login failed");
        return false;
    }

    public void Logout()
    {
        currentUser = null;
        PlayerPrefs.DeleteKey("LastLoggedInUser");
        PlayerPrefs.Save();
        SceneManager.LoadScene("LoginScene");
    }

    public User GetCurrentUser()
    {
        return currentUser;
    }

    public void UpdateUserStats(int gamesPlayed, int wins, int losses)
    {
        if (currentUser != null)
        {
            currentUser.Stats.GamesPlayed += gamesPlayed;
            currentUser.Stats.Wins += wins;
            currentUser.Stats.Losses += losses;
            SaveUsers();
        }
    }
}