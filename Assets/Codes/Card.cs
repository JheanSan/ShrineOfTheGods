using UnityEngine;

public class Card
{
    public string Name { get; private set; }
    public string Element { get; private set; }
    public int Attack { get; private set; }
    public int MaxDefense { get; private set; }
    public int CurrentDefense { get; private set; }
    public string Lore { get; private set; }
    public int ManaCost { get; private set; }
    public bool HasUsedElementalPower { get; set; }
    public string ImagePath { get; private set; }

    public Card(string name, string element, int attack, int defense, string lore, int manaCost, string imagePath)
    {
        this.Name = name;
        this.Element = element;
        this.Attack = attack;
        this.MaxDefense = defense;
        this.CurrentDefense = defense;
        this.Lore = lore;
        this.ManaCost = manaCost;
        this.HasUsedElementalPower = false;
        this.ImagePath = imagePath;
    }

    // Add an overload of the constructor for backward compatibility
    public Card(string name, string element, int attack, int defense, string lore, int manaCost)
        : this(name, element, attack, defense, lore, manaCost, "CardArt/default_card") // Use a default image path
    {
    }

    public Card Clone()
    {
        return new Card(Name, Element, Attack, MaxDefense, Lore, ManaCost, ImagePath);
    }

    public void TakeDamage(int damage)
    {
        if (Element == "Earth" && !HasUsedElementalPower)
        {
            HasUsedElementalPower = true;
            Debug.Log($"{Name} blocked damage with Earth element!");
            return;
        }
        CurrentDefense = Mathf.Max(0, CurrentDefense - damage);
    }

    public void Heal(int amount)
    {
        CurrentDefense = Mathf.Min(MaxDefense, CurrentDefense + amount);
    }

    public void DisplayInfo()
    {
        Debug.Log($"Name: {Name}, Element: {Element}, Attack: {Attack}, Defense: {CurrentDefense}/{MaxDefense}, Lore: {Lore}, Mana Cost: {ManaCost}");
    }
}