namespace Shrine.Gameplay
{
    /// <summary>How hard the rival shrine plays. Chosen where the battle model is created.</summary>
    public enum AiDifficulty
    {
        /// <summary>Holds back: develops slowly and only takes clearly good trades.</summary>
        Measured,

        /// <summary>Default. Develops a board every turn and trades when it is even or better.</summary>
        Aggressive,

        /// <summary>Spends nearly every point of mana and accepts chip damage to keep pressure.</summary>
        Merciless
    }

    /// <summary>
    /// Every number the opponent planner uses. Tuning means editing this file, never
    /// scattering constants through <see cref="OpponentAI"/>. The planner is plain C#
    /// (no Unity calls) so it stays testable and deterministic.
    ///
    /// Read docs/AI_GUIDE.md before changing a weight: it explains what each one does,
    /// which preset uses it and how to verify the change with the autobattle harness.
    /// </summary>
    public sealed class AiTuning
    {
        /// <summary>Hard cap on cards played in one turn. Field size still applies on top.</summary>
        public int MaxCardsPerTurn = 4;

        /// <summary>Weight of a card's attack when valuing what to play.</summary>
        public float AttackWeight = 1.15f;

        /// <summary>Weight of a card's current defence when valuing what to play.</summary>
        public float DefenseWeight = 0.95f;

        /// <summary>Weight of the plain stat total. High values prefer big bodies.</summary>
        public float StatWeight = 1f;

        /// <summary>Weight of stats per point of mana. High values prefer efficient cards.</summary>
        public float ValuePerManaWeight = 2.2f;

        /// <summary>Extra score for an attack that destroys the target.</summary>
        public float KillBonus = 3.5f;

        /// <summary>Score per point of attack removed from the opposing board.</summary>
        public float ThreatWeight = 0.6f;

        /// <summary>Score lost per point of stats when our attacker dies to retaliation.</summary>
        public float LossPenalty = 0.6f;

        /// <summary>Extra penalty when our attacker dies without destroying the target.</summary>
        public float WastePenalty = 2.5f;

        /// <summary>Attacks scoring below this are skipped. Lower values make the rival bolder.</summary>
        public float MinimumAttackScore = 0f;

        /// <summary>Defence bonus while behind on board cards.</summary>
        public float BehindBoardBonus = 1.4f;

        /// <summary>Player health at or below which the rival starts racing.</summary>
        public int LowHealth = 12;

        /// <summary>Attack multiplier applied to card choice while the player is nearly dead.</summary>
        public float LowHealthAggression = 1.6f;

        /// <summary>Whether cards played this turn may also attack this turn.</summary>
        public bool AttackWithNewCards = true;

        /// <summary>Random spread on equal-value choices so two seeds do not play identical games.</summary>
        public float TieBreakJitter = 0.05f;

        /// <summary>Named preset. Add new difficulties here so callers stay untouched.</summary>
        public static AiTuning From(AiDifficulty difficulty)
        {
            switch (difficulty)
            {
                case AiDifficulty.Measured:
                    return new AiTuning
                    {
                        MaxCardsPerTurn = 3,
                        AttackWeight = 1.0f,
                        DefenseWeight = 1.15f,
                        StatWeight = 1.0f,
                        ValuePerManaWeight = 1.8f,
                        KillBonus = 2.8f,
                        ThreatWeight = 0.45f,
                        LossPenalty = 0.9f,
                        WastePenalty = 3.5f,
                        MinimumAttackScore = 1.5f,
                        BehindBoardBonus = 1.8f,
                        LowHealth = 8,
                        LowHealthAggression = 1.25f,
                        AttackWithNewCards = false
                    };
                case AiDifficulty.Merciless:
                    return new AiTuning
                    {
                        MaxCardsPerTurn = 5,
                        AttackWeight = 1.35f,
                        DefenseWeight = 0.8f,
                        StatWeight = 0.9f,
                        ValuePerManaWeight = 2.4f,
                        KillBonus = 4.5f,
                        ThreatWeight = 0.8f,
                        LossPenalty = 0.35f,
                        WastePenalty = 1.5f,
                        MinimumAttackScore = -3f,
                        BehindBoardBonus = 1.1f,
                        LowHealth = 16,
                        LowHealthAggression = 1.9f,
                        AttackWithNewCards = true
                    };
                default:
                    return new AiTuning();
            }
        }
    }
}
