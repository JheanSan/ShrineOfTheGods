using System;
using System.Collections.Generic;
using Shrine.Data;

namespace Shrine.Gameplay
{
    public enum BattleActionKind
    {
        Play,
        AttackCard,
        AttackHero
    }

    public readonly struct BattleAction
    {
        public BattleActionKind Kind { get; }
        public int CardUid { get; }
        public int TargetUid { get; }

        public BattleAction(BattleActionKind kind, int cardUid, int targetUid = 0)
        {
            Kind = kind;
            CardUid = cardUid;
            TargetUid = targetUid;
        }
    }

    /// <summary>
    /// Deterministic opponent planner. Returns an ordered action list; the presentation
    /// layer executes it with delays, so nothing here touches Unity, time or threads.
    ///
    /// Structure (read this before changing the AI):
    ///   1. <see cref="PlanTurn"/> is the only public entry point.
    ///   2. <see cref="ChoosePlays"/> picks the best set of cards for the mana it owns.
    ///   3. <see cref="PlanAttacks"/> spends every ready card, aiming for kills.
    ///   4. Every number comes from <see cref="AiTuning"/> - tune there, not here.
    ///
    /// docs/AI_GUIDE.md explains the weights, the invariants and how to add a behaviour.
    /// </summary>
    public sealed class OpponentAI
    {
        private readonly BattleModel _model;
        private readonly IRandom _random;
        private readonly AiTuning _tuning;

        public OpponentAI(BattleModel model, IRandom random, AiTuning tuning = null)
        {
            _model = model;
            // Kept for deterministic tie-breaking and future randomised personalities.
            _random = random;
            _tuning = tuning ?? AiTuning.From(AiDifficulty.Aggressive);
        }

        public AiTuning Tuning => _tuning;

        /// <summary>Human-readable result of the last plan, used by the battle log and tests.</summary>
        public string LastPlanSummary { get; private set; } = "idle";

        public List<BattleAction> PlanTurn()
        {
            var actions = new List<BattleAction>();
            LastPlanSummary = "idle";
            if (_model.IsOver || _model.CurrentSide != Side.Opponent) return actions;

            List<CardInstance> fresh = ChoosePlays();
            foreach (CardInstance card in fresh)
                actions.Add(new BattleAction(BattleActionKind.Play, card.Uid));

            PlanAttacks(actions, fresh);
            LastPlanSummary = Describe(actions);
            return actions;
        }

        /// <summary>
        /// Picks the most valuable set of cards the rival can afford this turn. The
        /// search is an exhaustive subset scan over the hand (bounded by balance: 8
        /// cards), which fills the board better than picking one card at a time — with
        /// four mana it buys two efficient cards instead of one expensive body.
        /// </summary>
        private List<CardInstance> ChoosePlays()
        {
            var chosen = new List<CardInstance>();
            Participant rival = _model.Opponent;
            int slots = Math.Min(_tuning.MaxCardsPerTurn, _model.MaxFieldSize - rival.Field.Count);
            if (slots <= 0 || rival.Hand.Count == 0 || rival.Mana <= 0) return chosen;

            var candidates = new List<Candidate>(rival.Hand.Count);
            foreach (CardInstance card in rival.Hand)
            {
                int cost = _model.EffectiveCost(card, Side.Opponent);
                candidates.Add(new Candidate(card, cost, ScorePlay(card, cost)));
            }

            // The scan is 2^n, so keep it bounded even if a future balance pass raises
            // the hand size. Dropping the least valuable cards can never lose a real pick.
            if (candidates.Count > 16)
            {
                candidates.Sort((a, b) => b.Value.CompareTo(a.Value));
                candidates.RemoveRange(16, candidates.Count - 16);
            }

            int count = candidates.Count;
            int bestMask = 0;
            float bestValue = 0f;
            for (int mask = 1; mask < 1 << count; mask++)
            {
                int cards = 0;
                int mana = 0;
                float value = 0f;
                bool valid = true;
                for (int i = 0; i < count; i++)
                {
                    if ((mask & (1 << i)) == 0) continue;
                    cards++;
                    mana += candidates[i].Cost;
                    if (cards > slots || mana > rival.Mana)
                    {
                        valid = false;
                        break;
                    }
                    value += candidates[i].Value;
                }

                if (!valid) continue;
                if (value > bestValue)
                {
                    bestValue = value;
                    bestMask = mask;
                }
            }

            for (int i = 0; i < count; i++)
                if ((bestMask & (1 << i)) != 0) chosen.Add(candidates[i].Card);

            // Cheap cards first: it reads better and keeps the mana bookkeeping obvious.
            chosen.Sort((a, b) => _model.EffectiveCost(a, Side.Opponent)
                .CompareTo(_model.EffectiveCost(b, Side.Opponent)));
            return chosen;
        }

        /// <summary>
        /// Value of a single card: raw stats plus stats per mana, with attack amplified
        /// while the player is nearly dead and defence amplified while behind on board.
        /// </summary>
        private float ScorePlay(CardInstance card, int cost)
        {
            float attack = card.Data.Attack;
            float defense = card.CurrentDefense;

            if (_model.Player.Health <= _tuning.LowHealth)
                attack *= _tuning.LowHealthAggression;

            bool behind = _model.Player.Field.Count > _model.Opponent.Field.Count;
            if (behind) defense *= _tuning.BehindBoardBonus;

            float value = attack * _tuning.AttackWeight + defense * _tuning.DefenseWeight;
            float efficiency = value / Math.Max(1, cost);
            float edge = ElementEdge(card);
            float jitter = (float)_random.NextDouble() * _tuning.TieBreakJitter;
            return value * _tuning.StatWeight + efficiency * _tuning.ValuePerManaWeight
                + edge + jitter;
        }

        /// <summary>
        /// How well this card's element answers the biggest threat on the player's side.
        /// Doubling into the largest body is worth more than a marginally bigger stat line.
        /// </summary>
        private float ElementEdge(CardInstance card)
        {
            int bestThreat = 0;
            foreach (CardInstance defender in _model.Player.Field)
            {
                if (!ElementRules.HasAdvantage(card.Data.Element, defender.Data.Element))
                    continue;
                bestThreat = Math.Max(bestThreat, defender.Data.Attack + defender.CurrentDefense);
            }

            return bestThreat * _tuning.ElementEdgeBonus * 0.25f;
        }

        /// <summary>
        /// Attacks with every ready card, including cards played this turn when the
        /// tuning allows it. The hero may only be struck when the player has no cards
        /// left (the same rule the player plays by), so clearing the board comes first.
        /// Planned damage is tracked per defender so two cards never hit a dead target.
        /// </summary>
        private void PlanAttacks(List<BattleAction> actions, List<CardInstance> fresh)
        {
            Participant rival = _model.Opponent;
            Participant player = _model.Player;

            var defenders = new List<CardInstance>(player.Field);
            var remainingDefense = new Dictionary<int, int>(defenders.Count);
            foreach (CardInstance defender in defenders)
                remainingDefense[defender.Uid] = defender.CurrentDefense;

            var attackers = new List<CardInstance>(rival.Field);
            if (_tuning.AttackWithNewCards) attackers.AddRange(fresh);
            // Biggest swing first: a kill removes the target for everyone after, so the
            // smaller attackers are never wasted chipping a card that was going to die.
            attackers.Sort((a, b) => b.Data.Attack.CompareTo(a.Data.Attack));

            foreach (CardInstance attacker in attackers)
            {
                if (!CanStrike(attacker, fresh)) continue;

                CardInstance target = ChooseTarget(attacker, defenders, remainingDefense);
                if (target == null)
                {
                    if (BoardCleared(defenders, remainingDefense))
                        actions.Add(new BattleAction(BattleActionKind.AttackHero, attacker.Uid));
                    continue;
                }

                actions.Add(new BattleAction(BattleActionKind.AttackCard, attacker.Uid, target.Uid));
                int damage = _model.EffectiveAttack(attacker, target, Side.Opponent);
                remainingDefense[target.Uid] = Math.Max(0, remainingDefense[target.Uid] - damage);
            }
        }

        /// <summary>Cards queued this turn are not on the field yet, so they are checked directly.</summary>
        private bool CanStrike(CardInstance card, List<CardInstance> fresh)
        {
            if (card.CurrentDefense <= 0) return false;
            if (fresh.Contains(card)) return true;
            return _model.CanAttack(Side.Opponent, card.Uid);
        }

        /// <summary>Best card to hit. Null means hold this attacker back.</summary>
        private CardInstance ChooseTarget(CardInstance attacker, List<CardInstance> defenders,
            Dictionary<int, int> remainingDefense)
        {
            CardInstance best = null;
            float bestScore = float.MinValue;
            foreach (CardInstance target in defenders)
            {
                if (!remainingDefense.TryGetValue(target.Uid, out int health) || health <= 0) continue;
                float score = ScoreTarget(attacker, target, health);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = target;
                }
            }

            return best != null && bestScore >= _tuning.MinimumAttackScore ? best : null;
        }

        /// <summary>
        /// Value of one attack against one card: the damage itself, a bonus for the kill
        /// and the threat removed, minus what the retaliation would cost us.
        /// </summary>
        private float ScoreTarget(CardInstance attacker, CardInstance target, int targetHealth)
        {
            int damage = _model.EffectiveAttack(attacker, target, Side.Opponent);
            bool kills = damage >= targetHealth;
            bool dies = WouldDieToRetaliation(attacker, target);

            float score = damage;
            if (kills) score += _tuning.KillBonus + target.Data.Attack * _tuning.ThreatWeight;

            if (dies)
            {
                score -= (attacker.Data.Attack + attacker.CurrentDefense) * 0.5f * _tuning.LossPenalty;
                if (!kills) score -= _tuning.WastePenalty;
            }

            return score;
        }

        private static bool BoardCleared(List<CardInstance> defenders, Dictionary<int, int> remainingDefense)
        {
            foreach (CardInstance defender in defenders)
                if (remainingDefense[defender.Uid] > 0) return false;
            return true;
        }

        /// <summary>Delegates to the model so the planner can never invent its own combat maths.</summary>
        private bool WouldDieToRetaliation(CardInstance attacker, CardInstance target)
        {
            return _model.RetaliationDamage(attacker, target) >= attacker.CurrentDefense;
        }

        private static string Describe(List<BattleAction> actions)
        {
            int plays = 0, cards = 0, face = 0;
            foreach (BattleAction action in actions)
            {
                switch (action.Kind)
                {
                    case BattleActionKind.Play: plays++; break;
                    case BattleActionKind.AttackCard: cards++; break;
                    case BattleActionKind.AttackHero: face++; break;
                }
            }
            return $"plays={plays} trades={cards} face={face}";
        }

        private readonly struct Candidate
        {
            public readonly CardInstance Card;
            public readonly int Cost;
            public readonly float Value;

            public Candidate(CardInstance card, int cost, float value)
            {
                Card = card;
                Cost = cost;
                Value = value;
            }
        }
    }
}
