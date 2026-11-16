using UnityEngine;

namespace Shrine.Data
{
    /// <summary>Elemental alignment of a card. Values drive the combat matrix.</summary>
    public enum ElementKind
    {
        Fire,
        Water,
        Earth,
        Air
    }

    /// <summary>The patron whose blessing can be invoked at full divine favour.</summary>
    public enum PatronKind
    {
        Flame,
        Tide,
        Stone,
        Gale
    }

    /// <summary>Which participant is acting in a battle.</summary>
    public enum Side
    {
        Player,
        Opponent
    }

    /// <summary>Immutable, serialisable card values shared by model and UI.</summary>
    [System.Serializable]
    public struct CardData
    {
        public string Id;
        public string DisplayName;
        public ElementKind Element;
        public int Attack;
        public int Defense;
        public int ManaCost;
        [TextArea(2, 5)] public string Lore;
        public string ArtResourcePath;

        public CardData(string id, string name, ElementKind element, int attack,
            int defense, int manaCost, string lore, string artResourcePath)
        {
            Id = id;
            DisplayName = name;
            Element = element;
            Attack = attack;
            Defense = defense;
            ManaCost = manaCost;
            Lore = lore;
            ArtResourcePath = artResourcePath;
        }
    }

    /// <summary>ScriptableObject wrapper so designs can be tuned in the Inspector.</summary>
    [CreateAssetMenu(menuName = "Shrine/Card", fileName = "Card")]
    public sealed class CardDefinition : ScriptableObject
    {
        [SerializeField] private CardData _data;

        public CardData Data => _data;

        public void Set(CardData data) => _data = data;
    }
}
