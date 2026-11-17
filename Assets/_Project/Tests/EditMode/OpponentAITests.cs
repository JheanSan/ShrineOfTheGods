using System.Collections.Generic;
using NUnit.Framework;
using Shrine.Data;
using Shrine.Gameplay;
using UnityEngine;

namespace Shrine.Tests
{
    /// <summary>
    /// Guards the contract of <see cref="OpponentAI"/>: it must spend the mana it has,
    /// obey the hero-attack rule, and react to <see cref="AiTuning"/> presets.
    /// See docs/AI_GUIDE.md before changing these expectations.
    /// </summary>
    public sealed class OpponentAITests
    {
        private static readonly OmenData[] NoOmen =
        {
            new OmenData("none", "Still Air", "No rule change this round.")
        };

        private static BattleModel Create(int seed = 42)
        {
            BalanceConfig balance = ScriptableObject.CreateInstance<BalanceConfig>();
            balance.ResetDefaults();
            CardData[] deck =
            {
                ContentFactory.Cards[0], ContentFactory.Cards[3], ContentFactory.Cards[6],
                ContentFactory.Cards[14], ContentFactory.Cards[1], ContentFactory.Cards[7],
                ContentFactory.Cards[4], ContentFactory.Cards[9]
            };
            PatronData patron = ContentFactory.Patrons[0];
            PatronData rival = ContentFactory.Patrons[ContentFactory.Patrons.Length - 1];
            return new BattleModel(balance, ContentFactory.Cards, NoOmen, deck, patron, rival,
                new SeededRandom(seed));
        }

        /// <summary>Fresh battle with the rival to act, its mana grown to <paramref name="mana"/>.</summary>
        private static BattleModel RivalTurn(int mana = 2, int seed = 42)
        {
            BattleModel model = Create(seed);
            model.Start();
            while (model.Opponent.Mana < mana)
            {
                model.EndTurn();
                Assert.IsFalse(model.IsOver, "Battle ended while advancing turns in a test.");
            }
            while (model.CurrentSide != Side.Opponent) model.EndTurn();
            return model;
        }

        private static CardInstance Card(string id, ElementKind element, int attack, int defense, int cost)
        {
            return new CardInstance(new CardData(id, id, element, attack, defense, cost, "test", string.Empty));
        }

        private static List<BattleAction> Plays(List<BattleAction> actions)
        {
            return actions.FindAll(a => a.Kind == BattleActionKind.Play);
        }

        [Test]
        public void PlanTurn_WhenPlayerFieldIsEmpty_AttacksTheHero()
        {
            BattleModel model = RivalTurn();
            model.Opponent.Hand.Clear();
            model.Opponent.Field.Clear();
            CardInstance goblin = Card("goblin", ElementKind.Air, 3, 3, 2);
            model.Opponent.Field.Add(goblin);

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            List<BattleAction> actions = ai.PlanTurn();

            Assert.IsTrue(actions.Exists(a => a.Kind == BattleActionKind.AttackHero && a.CardUid == goblin.Uid),
                "The rival must hit the hero once the player's field is empty.");
            StringAssert.Contains("face=1", ai.LastPlanSummary);
        }

        [Test]
        public void PlanTurn_DoesNotAttackHeroWhileADefenderSurvives()
        {
            BattleModel model = RivalTurn();
            model.Opponent.Field.Add(Card("goblin", ElementKind.Air, 3, 3, 2));
            // Twenty defence cannot be removed by any single opening attacker, so the
            // player's board is still standing when the plan finishes.
            model.Player.Field.Add(Card("wall", ElementKind.Earth, 1, 20, 2));

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            List<BattleAction> actions = ai.PlanTurn();

            Assert.IsFalse(actions.Exists(a => a.Kind == BattleActionKind.AttackHero),
                "The hero is protected while a defender survives the plan.");
            Assert.IsTrue(actions.Exists(a => a.Kind == BattleActionKind.AttackCard));
        }

        [Test]
        public void PlanTurn_SendsLeftoverAttackersAtTheHeroAfterClearingTheBoard()
        {
            BattleModel model = RivalTurn();
            model.Opponent.Hand.Clear();
            model.Opponent.Field.Clear();
            CardInstance goblin = Card("goblin", ElementKind.Air, 3, 3, 2);
            CardInstance sprinter = Card("sprinter", ElementKind.Air, 2, 2, 1);
            model.Opponent.Field.Add(goblin);
            model.Opponent.Hand.Add(sprinter);
            // Air doubles into Earth, so three attack removes the six-defence wall and the
            // cheap fresh card is free to strike the shrine in the same plan.
            model.Player.Field.Add(Card("wall", ElementKind.Earth, 1, 6, 2));

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            List<BattleAction> actions = ai.PlanTurn();

            Assert.IsTrue(actions.Exists(a => a.Kind == BattleActionKind.AttackCard
                && a.CardUid == goblin.Uid));
            Assert.IsTrue(actions.Exists(a => a.Kind == BattleActionKind.AttackHero
                && a.CardUid == sprinter.Uid), "A spare attacker should hit the shrine.");
        }

        [Test]
        public void PlanTurn_SpendsManaOnTheBestAffordableCards_AndNeverOverspends()
        {
            BattleModel model = RivalTurn();
            model.Opponent.Hand.Clear();
            CardInstance cheap = Card("cheap", ElementKind.Earth, 2, 2, 2);
            CardInstance pricey = Card("pricey", ElementKind.Earth, 5, 5, 3);
            model.Opponent.Hand.Add(cheap);
            model.Opponent.Hand.Add(pricey);

            int mana = model.Opponent.Mana;
            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Aggressive));
            List<BattleAction> actions = ai.PlanTurn();

            List<BattleAction> plays = Plays(actions);
            Assert.AreEqual(1, plays.Count, "With two mana only the two-cost card fits.");
            Assert.AreEqual(cheap.Uid, plays[0].CardUid);

            int spent = 0;
            foreach (BattleAction play in plays)
            {
                spent += model.EffectiveCost(model.Opponent.FindHand(play.CardUid), Side.Opponent);
            }
            Assert.LessOrEqual(spent, mana, "The rival must never queue more mana than it owns.");
        }

        [Test]
        public void PlanTurn_PlaysSeveralCardsInOneTurn_WhenManaAllows()
        {
            BattleModel model = RivalTurn(5);
            model.Opponent.Hand.Clear();
            for (int i = 0; i < 3; i++)
                model.Opponent.Hand.Add(Card("goblin" + i, ElementKind.Air, 3, 3, 2));

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            List<BattleAction> actions = ai.PlanTurn();

            Assert.AreEqual(2, Plays(actions).Count, "Five mana should buy two goblins.");
            StringAssert.Contains("plays=2", ai.LastPlanSummary);
        }

        [Test]
        public void PlanTurn_NeverQueuesTheSameCardTwice()
        {
            BattleModel model = RivalTurn(10);
            model.Opponent.Hand.Clear();
            for (int i = 0; i < 4; i++)
                model.Opponent.Hand.Add(Card("goblin" + i, ElementKind.Air, 3, 3, 2));

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            List<BattleAction> actions = ai.PlanTurn();

            var seen = new HashSet<int>();
            foreach (BattleAction play in Plays(actions))
                Assert.IsTrue(seen.Add(play.CardUid), "A card was queued twice in the same turn.");
        }

        [Test]
        public void EveryPresetRefusesToThrowAwayACardForNothing()
        {
            BattleModel model = RivalTurn();
            model.Opponent.Hand.Clear();
            model.Opponent.Field.Clear();
            CardInstance frail = Card("frail", ElementKind.Water, 1, 1, 2);
            CardInstance brute = Card("brute", ElementKind.Earth, 5, 4, 8);
            model.Opponent.Field.Add(frail);
            model.Player.Field.Add(brute);

            AiDifficulty[] presets =
            {
                AiDifficulty.Measured, AiDifficulty.Aggressive, AiDifficulty.Merciless
            };
            foreach (AiDifficulty preset in presets)
            {
                var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(preset));
                Assert.IsFalse(ai.PlanTurn().Exists(a => a.Kind == BattleActionKind.AttackCard),
                    $"{preset} traded a card that dies without killing anything.");
            }
        }

        [Test]
        public void Merciless_TakesAProfitableKillEvenThoughItsCardDies()
        {
            BattleModel model = RivalTurn();
            model.Opponent.Hand.Clear();
            // Earth doubles into Water, so five attack removes the one-life target; the
            // water reply is not doubled and still kills the martyr. That trade is worth it.
            model.Opponent.Field.Clear();
            CardInstance martyr = Card("martyr", ElementKind.Earth, 5, 3, 4);
            CardInstance fragile = Card("fragile", ElementKind.Water, 6, 1, 6);
            model.Opponent.Field.Add(martyr);
            model.Player.Field.Add(fragile);

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            Assert.IsTrue(ai.PlanTurn().Exists(a => a.Kind == BattleActionKind.AttackCard
                && a.CardUid == martyr.Uid), "A clean kill is always worth the body.");
        }

        [Test]
        public void Presets_AreOrderedFromCautiousToMerciless()
        {
            AiTuning measured = AiTuning.From(AiDifficulty.Measured);
            AiTuning aggressive = AiTuning.From(AiDifficulty.Aggressive);
            AiTuning merciless = AiTuning.From(AiDifficulty.Merciless);

            Assert.Less(measured.MaxCardsPerTurn, merciless.MaxCardsPerTurn);
            Assert.Greater(measured.MinimumAttackScore, aggressive.MinimumAttackScore);
            // Merciless is not reckless: it accepts thinner edges than Measured because it
            // develops a wider board, but it never declines to a losing-trade free-for-all.
            Assert.Greater(measured.MinimumAttackScore, merciless.MinimumAttackScore);
            Assert.Greater(merciless.KillBonus, aggressive.KillBonus);
            Assert.Greater(merciless.LowHealth, measured.LowHealth);
            Assert.Greater(merciless.ElementEdgeBonus, measured.ElementEdgeBonus);
            Assert.IsFalse(measured.AttackWithNewCards);
            Assert.IsTrue(merciless.AttackWithNewCards);
        }

        [Test]
        public void PlanTurn_BuysTwoEfficientCardsInsteadOfOneExpensiveBody()
        {
            BattleModel model = RivalTurn(4);
            model.Opponent.Hand.Clear();
            CardInstance gorgon = Card("gorgon", ElementKind.Earth, 5, 4, 3);
            CardInstance first = Card("goblin1", ElementKind.Air, 3, 3, 2);
            CardInstance second = Card("goblin2", ElementKind.Air, 3, 3, 2);
            model.Opponent.Hand.Add(gorgon);
            model.Opponent.Hand.Add(first);
            model.Opponent.Hand.Add(second);

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            List<BattleAction> plays = Plays(ai.PlanTurn());

            Assert.AreEqual(2, plays.Count, "Four mana buys two two-cost cards, which is a wider board.");
            Assert.IsTrue(plays.Exists(p => p.CardUid == first.Uid));
            Assert.IsTrue(plays.Exists(p => p.CardUid == second.Uid));
        }

        [Test]
        public void PlanTurn_AttacksWithCardsPlayedThisTurn_ForAnAggressiveOpening()
        {
            BattleModel model = RivalTurn();
            CardInstance goblin = Card("goblin", ElementKind.Air, 3, 3, 2);
            model.Opponent.Hand.Clear();
            model.Opponent.Field.Clear();
            model.Opponent.Hand.Add(goblin);
            model.Player.Field.Add(Card("wisp", ElementKind.Water, 0, 1, 1));

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            List<BattleAction> actions = ai.PlanTurn();

            Assert.IsTrue(actions.Exists(a => a.Kind == BattleActionKind.Play && a.CardUid == goblin.Uid));
            Assert.IsTrue(actions.Exists(a => a.Kind == BattleActionKind.AttackCard && a.CardUid == goblin.Uid),
                "A card played this turn can still swing, so the rival should use it.");
        }

        [Test]
        public void PlanTurn_SendsLeftoverAttackersAtTheHero_AfterTheBoardIsCleared()
        {
            BattleModel model = RivalTurn();
            CardInstance hitter = Card("hitter", ElementKind.Fire, 3, 6, 2);
            CardInstance finisher = Card("finisher", ElementKind.Fire, 2, 2, 2);
            model.Opponent.Field.Clear();
            model.Opponent.Field.Add(hitter);
            model.Opponent.Field.Add(finisher);
            model.Player.Field.Add(Card("wisp", ElementKind.Water, 0, 1, 1));

            var ai = new OpponentAI(model, new SeededRandom(1), AiTuning.From(AiDifficulty.Merciless));
            List<BattleAction> actions = ai.PlanTurn();

            Assert.IsTrue(actions.Exists(a => a.Kind == BattleActionKind.AttackCard && a.CardUid == hitter.Uid));
            Assert.IsTrue(actions.Exists(a => a.Kind == BattleActionKind.AttackHero && a.CardUid == finisher.Uid),
                "Once the last blocker is dead the remaining attackers must go face.");
        }

        [Test]
        public void PlanTurn_ReportsIdleSummary_WhenItIsNotTheRivalsTurn()
        {
            BattleModel model = Create();
            model.Start();
            var ai = new OpponentAI(model, new SeededRandom(1));
            Assert.AreEqual(0, ai.PlanTurn().Count);
            Assert.AreEqual("idle", ai.LastPlanSummary);
        }
    }
}
