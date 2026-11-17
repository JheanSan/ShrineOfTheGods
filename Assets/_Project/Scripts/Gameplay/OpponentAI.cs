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
    /// Deterministic opponent planner. It returns actions; the presentation layer
    /// executes them with delays. No threading and no Unity calls.
    /// </summary>
    public sealed class OpponentAI
    {
        private readonly BattleModel _model;
        private readonly IRandom _random;

        public OpponentAI(BattleModel model, IRandom random)
        {
            _model = model;
            _random = random;
        }

        public List<BattleAction> PlanTurn()
        {
            var actions = new List<BattleAction>();
            if (_model.IsOver || _model.CurrentSide != Side.Opponent) return actions;

            Participant opponent = _model.Opponent;
            Participant player = _model.Player;

            var playable = new List<CardInstance>(opponent.Hand);
            playable.Sort((a, b) => b.Data.ManaCost.CompareTo(a.Data.ManaCost));
            int mana = opponent.Mana;
            foreach (CardInstance card in playable)
            {
                if (opponent.Field.Count + actions.Count >= 5) break;
                int cost = _model.EffectiveCost(card, Side.Opponent);
                if (cost > mana) continue;
                actions.Add(new BattleAction(BattleActionKind.Play, card.Uid));
                mana -= cost;
            }

            foreach (CardInstance attacker in new List<CardInstance>(opponent.Field))
            {
                if (!_model.CanAttack(Side.Opponent, attacker.Uid)) continue;
                if (player.Field.Count > 0 && _random.NextDouble() > 0.35)
                {
                    CardInstance target = player.Field[_random.Next(0, player.Field.Count)];
                    actions.Add(new BattleAction(BattleActionKind.AttackCard, attacker.Uid, target.Uid));
                }
                else
                {
                    actions.Add(new BattleAction(BattleActionKind.AttackHero, attacker.Uid));
                }
            }
            return actions;
        }
    }
}
