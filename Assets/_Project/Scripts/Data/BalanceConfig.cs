using UnityEngine;

namespace Shrine.Data
{
    /// <summary>Central tuning values. Every rule reads from this asset.</summary>
    [CreateAssetMenu(menuName = "Shrine/Balance", fileName = "BalanceConfig")]
    public sealed class BalanceConfig : ScriptableObject
    {
        [Header("Hero")]
        [SerializeField] private int _startingHealth = 30;
        [SerializeField] private int _startingMana = 1;
        [SerializeField] private int _maxMana = 10;

        [Header("Cards")]
        [SerializeField] private int _startingHandSize = 4;
        [SerializeField] private int _maxHandSize = 8;
        [SerializeField] private int _maxFieldSize = 5;
        [SerializeField] private int _deckSize = 8;
        [SerializeField] private int _copiesPerCard = 3;

        [Header("Divine favour")]
        [SerializeField] private int _favourMax = 100;
        [SerializeField] private int _favourPerFavouredCard = 10;
        [SerializeField] private int _favourPerOffering = 16;
        [SerializeField] private int _favourPerCardKill = 6;

        [Header("Rewards")]
        [SerializeField] private int _victoryXp = 100;
        [SerializeField] private int _defeatXp = 25;

        public int StartingHealth => _startingHealth;
        public int StartingMana => _startingMana;
        public int MaxMana => _maxMana;
        public int StartingHandSize => _startingHandSize;
        public int MaxHandSize => _maxHandSize;
        public int MaxFieldSize => _maxFieldSize;
        public int DeckSize => _deckSize;
        public int CopiesPerCard => _copiesPerCard;
        public int FavourMax => _favourMax;
        public int FavourPerFavouredCard => _favourPerFavouredCard;
        public int FavourPerOffering => _favourPerOffering;
        public int FavourPerCardKill => _favourPerCardKill;
        public int VictoryXp => _victoryXp;
        public int DefeatXp => _defeatXp;

        public void ResetDefaults()
        {
            _startingHealth = 30;
            _startingMana = 1;
            _maxMana = 10;
            _startingHandSize = 4;
            _maxHandSize = 8;
            _maxFieldSize = 5;
            _deckSize = 8;
            _copiesPerCard = 3;
            _favourMax = 100;
            _favourPerFavouredCard = 10;
            _favourPerOffering = 16;
            _favourPerCardKill = 6;
            _victoryXp = 100;
            _defeatXp = 25;
        }
    }
}
