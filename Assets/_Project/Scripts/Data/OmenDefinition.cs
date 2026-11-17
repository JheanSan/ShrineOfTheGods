using UnityEngine;

namespace Shrine.Data
{
    [System.Serializable]
    public struct OmenData
    {
        public string Id;
        public string Title;
        public string Description;
        public int AttackBonus;
        public int DefenseBonus;
        public int DirectDamageBonus;
        public bool RefreshMana;
        public bool FavourBonus;

        public OmenData(string id, string title, string description, int attackBonus = 0,
            int defenseBonus = 0, int directDamageBonus = 0, bool refreshMana = false,
            bool favourBonus = false)
        {
            Id = id;
            Title = title;
            Description = description;
            AttackBonus = attackBonus;
            DefenseBonus = defenseBonus;
            DirectDamageBonus = directDamageBonus;
            RefreshMana = refreshMana;
            FavourBonus = favourBonus;
        }
    }

    [CreateAssetMenu(menuName = "Shrine/Omen", fileName = "Omen")]
    public sealed class OmenDefinition : ScriptableObject
    {
        [SerializeField] private OmenData _data;

        public OmenData Data => _data;

        public void Set(OmenData data) => _data = data;
    }

    [System.Serializable]
    public struct PatronData
    {
        public PatronKind Kind;
        public string DisplayName;
        public string BlessingName;
        [TextArea(2, 4)] public string BlessingDescription;
        public string AvatarResourcePath;
        public ElementKind FavouredElement;

        public PatronData(PatronKind kind, string displayName, string blessingName,
            string blessingDescription, string avatarResourcePath, ElementKind favouredElement)
        {
            Kind = kind;
            DisplayName = displayName;
            BlessingName = blessingName;
            BlessingDescription = blessingDescription;
            AvatarResourcePath = avatarResourcePath;
            FavouredElement = favouredElement;
        }
    }

    [CreateAssetMenu(menuName = "Shrine/Patron", fileName = "Patron")]
    public sealed class PatronDefinition : ScriptableObject
    {
        [SerializeField] private PatronData _data;

        public PatronData Data => _data;

        public void Set(PatronData data) => _data = data;
    }
}
