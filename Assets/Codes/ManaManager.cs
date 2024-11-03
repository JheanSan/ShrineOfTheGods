using UnityEngine;
using TMPro;

public class ManaManager : MonoBehaviour
{
    public static ManaManager Instance { get; private set; }

    public int MaxMana { get; private set; } = 0;
    public int CurrentMana { get; private set; }
    public TMP_Text manaText;
    public TMP_Text messageText;

    private const int ABSOLUTE_MAX_MANA = 10;

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
    }

    public bool UseMana(int amount)
    {
        if (CurrentMana >= amount)
        {
            CurrentMana -= amount;
            UpdateManaText();
            return true;
        }
        else
        {
            ShowMessage("Not enough mana!");
            return false;
        }
    }

    public void ResetMana()
    {
        CurrentMana = MaxMana;
        UpdateManaText();
    }

    public void IncreaseManaPool()
    {
        if (MaxMana < ABSOLUTE_MAX_MANA)
        {
            MaxMana++;
        }
        ResetMana();
    }

    private void UpdateManaText()
    {
        if (manaText != null)
        {
            manaText.text = $"Mana: {CurrentMana}/{MaxMana}";
        }
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            Invoke(nameof(ClearMessage), 2f);
        }
    }

    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}