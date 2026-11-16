using Shrine.Data;

namespace Shrine.Gameplay
{
    /// <summary>Runtime copy of a card. Never shares mutable state with the definition.</summary>
    public sealed class CardInstance
    {
        private static int _nextUid;

        public int Uid { get; }
        public CardData Data { get; }
        public int CurrentDefense { get; private set; }
        public int MaxDefense => Data.Defense;
        public bool HasUsedPower { get; private set; }
        public bool HasAttacked { get; private set; }

        public CardInstance(CardData data)
        {
            Data = data;
            CurrentDefense = data.Defense;
            Uid = ++_nextUid;
        }

        public void ModifyDefense(int delta)
        {
            int before = CurrentDefense;
            CurrentDefense = UnityEngine.Mathf.Clamp(CurrentDefense + delta, 0, MaxDefense);
            _ = before;
        }

        public void ResetAttack() => HasAttacked = false;

        public void MarkAttacked() => HasAttacked = true;

        public void MarkPowerUsed() => HasUsedPower = true;

        public bool TakeDamage(int amount)
        {
            CurrentDefense = UnityEngine.Mathf.Max(0, CurrentDefense - amount);
            return CurrentDefense <= 0;
        }

        public void Restore()
        {
            CurrentDefense = MaxDefense;
            HasUsedPower = false;
            HasAttacked = false;
        }

        public static void ResetUidCounter() => _nextUid = 0;
    }
}
