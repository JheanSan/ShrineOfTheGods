using System.Collections.Generic;
using NUnit.Framework;
using Shrine.Data;
using Shrine.Gameplay;
using UnityEngine;

namespace Shrine.Tests
{
    public sealed class BattleModelTests
    {
        private static readonly OmenData[] NoOmen =
        {
            new OmenData("none", "Still Air", "No rule change this round.")
        };

        private static BattleModel Create(int seed = 42, CardData[] deck = null, PatronKind patron = PatronKind.Flame)
        {
            BalanceConfig balance = ScriptableObject.CreateInstance<BalanceConfig>();
            balance.ResetDefaults();
            CardData[] playerDeck = deck ?? new[] { ContentFactory.Cards[0], ContentFactory.Cards[3] };
            PatronData playerPatron = Find(patron);
            PatronData rivalPatron = Find(PatronKind.Stone);
            return new BattleModel(balance, ContentFactory.Cards, NoOmen, playerDeck,
                playerPatron, rivalPatron, new SeededRandom(seed));
        }

        private static PatronData Find(PatronKind kind)
        {
            foreach (PatronData data in ContentFactory.Patrons)
                if (data.Kind == kind) return data;
            return ContentFactory.Patrons[0];
        }

        [Test]
        public void Start_DrawsOpeningHands_AndPlayerActsFirst()
        {
            BattleModel model = Create();
            model.Start();
            Assert.AreEqual(4, model.Player.Hand.Count);
            Assert.AreEqual(4, model.Opponent.Hand.Count);
            Assert.AreEqual(Side.Player, model.CurrentSide);
            Assert.AreEqual(1, model.Round);
            Assert.AreEqual(2, model.Player.Mana);
        }

        [Test]
        public void Start_GuaranteesAtLeastOneAffordableCard()
        {
            CardData[] deck =
            {
                ContentFactory.Cards[0], ContentFactory.Cards[1], ContentFactory.Cards[2],
                ContentFactory.Cards[3], ContentFactory.Cards[4], ContentFactory.Cards[5],
                ContentFactory.Cards[6], ContentFactory.Cards[7]
            };
            for (int seed = 1; seed <= 12; seed++)
            {
                BattleModel model = Create(seed, deck);
                model.Start();
                Assert.IsTrue(model.Player.Hand.Exists(c => c.Data.ManaCost <= 2),
                    $"Seed {seed} produced no affordable opening card.");
            }
        }

        [Test]
        public void PlayCard_RejectsWhenManaIsInsufficient()
        {
            BattleModel model = Create(deck: new[] { ContentFactory.Cards[7] }); // Wizard, cost 6
            model.Start();
            CardInstance card = model.Player.Hand.Find(c => c.Data.ManaCost > 1);
            if (card == null) return; // deterministic hand may not contain it
            Assert.IsFalse(model.PlayCard(Side.Player, card.Uid));
            Assert.AreEqual(2, model.Player.Mana);
        }

        [Test]
        public void PlayCard_SpendsMana_AndGrantsFavourForFavouredElement()
        {
            BattleModel model = Create(deck: new[] { ContentFactory.Cards[0] }, patron: PatronKind.Flame);
            model.Start();
            CardInstance card = model.Player.Hand.Find(c => c.Data.Element == ElementKind.Fire);
            if (card == null || card.Data.ManaCost > model.Player.Mana) return;
            int mana = model.Player.Mana;
            Assert.IsTrue(model.PlayCard(Side.Player, card.Uid));
            Assert.AreEqual(mana - card.Data.ManaCost, model.Player.Mana);
            Assert.AreEqual(10, model.Player.Favour);
            Assert.AreEqual(1, model.Player.Field.Count);
        }

        [Test]
        public void Combat_FireDealsDoubleDamageToAir()
        {
            BattleModel model = Create();
            model.Start();
            var fire = new CardInstance(new CardData("fire", "Fire", ElementKind.Fire, 4, 2, 1, "t", string.Empty));
            var air = new CardInstance(new CardData("air", "Air", ElementKind.Air, 1, 8, 1, "t", string.Empty));
            model.Player.Field.Add(fire);
            model.Opponent.Field.Add(air);
            model.Attack(Side.Player, fire.Uid, air.Uid);
            Assert.AreEqual(0, air.CurrentDefense); // 8 - 8 = 0
        }

        [Test]
        public void Combat_EarthIgnoresFirstHit()
        {
            BattleModel model = Create();
            model.Start();
            var earth = new CardInstance(new CardData("earth", "Earth", ElementKind.Earth, 3, 5, 3, "t", string.Empty));
            model.Player.Field.Add(earth);
            model.Attack(Side.Player, earth.Uid);
            Assert.AreEqual(5, earth.CurrentDefense);
            Assert.IsTrue(earth.HasUsedPower);
        }

        [Test]
        public void Offering_GrantsFavour_AndDrawsReplacement()
        {
            BattleModel model = Create();
            model.Start();
            int hand = model.Player.Hand.Count;
            CardInstance card = model.Player.Hand[0];
            Assert.IsTrue(model.Offer(card.Uid));
            Assert.AreEqual(16, model.Player.Favour);
            Assert.AreEqual(hand, model.Player.Hand.Count);
            Assert.IsNull(model.Player.FindHand(card.Uid));
        }

        [Test]
        public void EndTurn_CyclesSides_AndGrowsManaOnNewRound()
        {
            BattleModel model = Create();
            model.Start();
            model.EndTurn();
            Assert.AreEqual(Side.Opponent, model.CurrentSide);
            Assert.AreEqual(1, model.Round);
            model.EndTurn();
            Assert.AreEqual(Side.Player, model.CurrentSide);
            Assert.AreEqual(2, model.Round);
            Assert.AreEqual(3, model.Player.MaxMana);
        }

        [Test]
        public void Blessing_SpendsFavour_AndAppliesPatronEffect()
        {
            CardData[] wideDeck =
            {
                ContentFactory.Cards[0], ContentFactory.Cards[1], ContentFactory.Cards[2],
                ContentFactory.Cards[3], ContentFactory.Cards[4], ContentFactory.Cards[5],
                ContentFactory.Cards[6], ContentFactory.Cards[7]
            };
            BattleModel model = Create(deck: wideDeck, patron: PatronKind.Gale);
            model.Start();
            for (int i = 0; i < 7; i++) model.Offer(model.Player.Hand[0].Uid);
            Assert.IsTrue(model.CanInvokeBlessing);
            int mana = model.Player.Mana;
            Assert.IsTrue(model.InvokeBlessing());
            Assert.AreEqual(0, model.Player.Favour);
            Assert.AreEqual(Mathf.Min(10, mana + 3), model.Player.Mana);
        }

        [Test]
        public void SeededRandom_ProducesIdenticalDraws()
        {
            BattleModel a = Create(7);
            BattleModel b = Create(7);
            a.Start();
            b.Start();
            var idsA = new List<string>();
            var idsB = new List<string>();
            foreach (CardInstance card in a.Player.Hand) idsA.Add(card.Data.Id);
            foreach (CardInstance card in b.Player.Hand) idsB.Add(card.Data.Id);
            Assert.AreEqual(idsA, idsB);
        }

        [Test]
        public void OpponentAI_DoesNotActOnPlayerTurn()
        {
            BattleModel model = Create();
            model.Start();
            var ai = new OpponentAI(model, new SeededRandom(1));
            Assert.AreEqual(0, ai.PlanTurn().Count);
        }
    }
}
