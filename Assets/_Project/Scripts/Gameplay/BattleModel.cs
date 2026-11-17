using System;
using System.Collections.Generic;
using Shrine.Data;
using UnityEngine;

namespace Shrine.Gameplay
{
    /// <summary>Snapshot of one participant, safe for UI binding.</summary>
    public sealed class Participant
    {
        public Side Side { get; }
        public string DisplayName { get; }
        public int Health { get; internal set; }
        public int MaxHealth { get; }
        public int Mana { get; internal set; }
        public int MaxMana { get; internal set; }
        public int Shield { get; internal set; }
        public int Favour { get; internal set; }
        public PatronData Patron { get; }
        public readonly List<CardInstance> Hand = new List<CardInstance>();
        public readonly List<CardInstance> Field = new List<CardInstance>();
        internal readonly List<CardData> DrawPile = new List<CardData>();

        public Participant(Side side, string name, int health, PatronData patron)
        {
            Side = side;
            DisplayName = name;
            Health = health;
            MaxHealth = health;
            Patron = patron;
        }

        public CardInstance FindHand(int uid) => Hand.Find(c => c.Uid == uid);

        public CardInstance FindField(int uid) => Field.Find(c => c.Uid == uid);
    }

    /// <summary>
    /// Pure rules engine for a battle. Holds no Unity component references; the
    /// presentation layer listens to events and animates the resulting changes.
    /// </summary>
    public sealed class BattleModel
    {
        private readonly BalanceConfig _balance;
        private readonly IReadOnlyList<CardData> _catalog;
        private readonly IReadOnlyList<OmenData> _omens;
        private readonly IRandom _random;
        private readonly Participant _player;
        private readonly Participant _opponent;

        private int _round;
        private OmenData _currentOmen;
        private bool _started;
        private bool _over;

        public event Action<Side> TurnChanged;
        public event Action<OmenData> OmenChanged;
        public event Action<CardInstance, Side> CardDrawn;
        public event Action<CardInstance, Side> CardPlayed;
        public event Action<CardInstance, Side, CardInstance, Side, int, int> CombatResolved;
        public event Action<CardInstance, Side, int> HeroDamaged;
        public event Action<CardInstance, Side> CardDestroyed;
        public event Action<Side, int, int> ManaChanged;
        public event Action<Side, int, int> HealthChanged;
        public event Action<int> FavourChanged;
        public event Action<string> BlessingInvoked;
        public event Action<bool> BattleEnded;
        public event Action<string> Log;

        public Participant Player => _player;
        public Participant Opponent => _opponent;
        public Side CurrentSide { get; private set; } = Side.Player;
        public int Round => _round;
        public OmenData CurrentOmen => _currentOmen;
        public bool IsOver => _over;
        public bool IsStarted => _started;

        public BattleModel(BalanceConfig balance, IReadOnlyList<CardData> catalog,
            IReadOnlyList<OmenData> omens, IReadOnlyList<CardData> playerDeck,
            PatronData playerPatron, PatronData opponentPatron, IRandom random,
            string playerName = "Pilgrim", string opponentName = "The Rival Shrine")
        {
            _balance = balance;
            _catalog = catalog;
            _omens = omens;
            _random = random;
            _player = new Participant(Side.Player, playerName, balance.StartingHealth, playerPatron);
            _opponent = new Participant(Side.Opponent, opponentName, balance.StartingHealth, opponentPatron);

            foreach (CardData card in playerDeck)
            {
                for (int i = 0; i < balance.CopiesPerCard; i++)
                    _player.DrawPile.Add(card);
            }
            for (int i = 0; i < balance.DeckSize * balance.CopiesPerCard; i++)
            {
                _opponent.DrawPile.Add(catalog[_random.Next(0, catalog.Count)]);
            }
        }

        public void Start()
        {
            if (_started) return;
            _started = true;
            _round = 1;
            _player.MaxMana = _balance.StartingMana;
            _opponent.MaxMana = _balance.StartingMana;
            _player.Mana = _player.MaxMana;
            _opponent.Mana = _opponent.MaxMana;
            ManaChanged?.Invoke(Side.Player, _player.Mana, _player.MaxMana);
            ManaChanged?.Invoke(Side.Opponent, _opponent.Mana, _opponent.MaxMana);

            for (int i = 0; i < _balance.StartingHandSize; i++)
            {
                Draw(Side.Player);
                Draw(Side.Opponent);
            }
            EnsurePlayableOpeningHand(_player);
            EnsurePlayableOpeningHand(_opponent);
            RollOmen();
            CurrentSide = Side.Player;
            TurnChanged?.Invoke(CurrentSide);
            Log?.Invoke("The shrine opens. The pilgrim acts first.");
        }

        public Participant ParticipantFor(Side side) => side == Side.Player ? _player : _opponent;

        /// <summary>Avoids a dead opening hand where every card costs more than starting mana.</summary>
        private void EnsurePlayableOpeningHand(Participant participant)
        {
            CardInstance cheapest = null;
            foreach (CardInstance card in participant.Hand)
                if (cheapest == null || card.Data.ManaCost < cheapest.Data.ManaCost)
                    cheapest = card;
            if (cheapest == null || cheapest.Data.ManaCost <= _balance.StartingMana) return;
            if (participant.DrawPile.Count == 0) return;

            int cheapestIndex = 0;
            for (int i = 1; i < participant.DrawPile.Count; i++)
                if (participant.DrawPile[i].ManaCost < participant.DrawPile[cheapestIndex].ManaCost)
                    cheapestIndex = i;
            CardData affordable = participant.DrawPile[cheapestIndex];
            participant.DrawPile[cheapestIndex] = cheapest.Data;
            participant.Hand.Remove(cheapest);
            var replacement = new CardInstance(affordable);
            if (_currentOmen.DefenseBonus > 0) replacement.ModifyDefense(_currentOmen.DefenseBonus);
            participant.Hand.Add(replacement);
        }

        public bool CanPlay(Side side, int uid)
        {
            if (_over || CurrentSide != side) return false;
            Participant p = ParticipantFor(side);
            CardInstance card = p.FindHand(uid);
            if (card == null || p.Field.Count >= _balance.MaxFieldSize) return false;
            return p.Mana >= EffectiveCost(card, side);
        }

        public bool PlayCard(Side side, int uid)
        {
            if (!CanPlay(side, uid)) return false;
            Participant p = ParticipantFor(side);
            CardInstance card = p.FindHand(uid);
            p.Hand.Remove(card);
            p.Field.Add(card);
            p.Mana -= EffectiveCost(card, side);

            if (_currentOmen.DefenseBonus > 0)
                card.ModifyDefense(_currentOmen.DefenseBonus);
            if (card.Data.Element == ElementKind.Air && !card.HasUsedPower)
                card.MarkPowerUsed();
            if (card.Data.Element == ElementKind.Water && _currentOmen.DefenseBonus > 0)
                card.ModifyDefense(1);

            if (side == Side.Player)
            {
                if (card.Data.Element == p.Patron.FavouredElement)
                    AddFavour(_balance.FavourPerFavouredCard * (_currentOmen.FavourBonus ? 2 : 1));
                Log?.Invoke($"You play {card.Data.DisplayName}.");
            }
            else
            {
                Log?.Invoke($"The rival plays {card.Data.DisplayName}.");
            }

            ManaChanged?.Invoke(side, p.Mana, p.MaxMana);
            CardPlayed?.Invoke(card, side);
            return true;
        }

        public bool CanAttack(Side side, int uid)
        {
            if (_over || CurrentSide != side) return false;
            Participant p = ParticipantFor(side);
            CardInstance card = p.FindField(uid);
            return card != null && !card.HasAttacked && card.CurrentDefense > 0;
        }

        /// <summary>Attacks a card when targetUid is set, otherwise the opposing hero.</summary>
        public bool Attack(Side side, int uid, int? targetUid = null)
        {
            if (!CanAttack(side, uid)) return false;
            Participant attacker = ParticipantFor(side);
            Participant defender = ParticipantFor(Other(side));
            CardInstance card = attacker.FindField(uid);
            card.MarkAttacked();

            if (targetUid.HasValue)
            {
                CardInstance target = defender.FindField(targetUid.Value);
                if (target == null) return false;

                int damage = EffectiveAttack(card, target, side);
                bool died = target.TakeDamage(damage);

                int retaliation = target.Data.Attack;
                if (target.Data.Element == ElementKind.Fire && card.Data.Element == ElementKind.Air)
                    retaliation *= 2;
                if (card.Data.Element == ElementKind.Earth && !card.HasUsedPower)
                {
                    card.MarkPowerUsed();
                    card.ModifyDefense(2);
                    retaliation = 0;
                }
                bool attackerDied = card.TakeDamage(retaliation);

                if (card.Data.Element == ElementKind.Water && !card.HasUsedPower)
                {
                    card.MarkPowerUsed();
                    card.ModifyDefense(2);
                }

                CombatResolved?.Invoke(card, side, target, Other(side), damage, retaliation);
                Log?.Invoke($"{card.Data.DisplayName} strikes {target.Data.DisplayName} for {damage}.");
                if (died) DestroyCard(target, Other(side));
                if (attackerDied) DestroyCard(card, side);
            }
            else
            {
                int damage = card.Data.Attack + (card.Data.Element == ElementKind.Fire ? _currentOmen.AttackBonus : 0)
                    + _currentOmen.DirectDamageBonus;
                if (card.Data.Element == ElementKind.Earth && !card.HasUsedPower)
                {
                    card.MarkPowerUsed();
                    card.ModifyDefense(2);
                    damage = 0;
                }
                ApplyHeroDamage(defender, damage);
                HeroDamaged?.Invoke(card, Other(side), damage);
                Log?.Invoke($"{card.Data.DisplayName} strikes the shrine for {damage}.");
                if (card.Data.Element == ElementKind.Water && !card.HasUsedPower)
                {
                    card.MarkPowerUsed();
                    card.ModifyDefense(2);
                }
            }

            if (side == Side.Player && _currentOmen.FavourBonus)
                AddFavour(2);
            CheckEnd();
            return true;
        }

        public bool Offer(int uid)
        {
            if (_over || CurrentSide != Side.Player) return false;
            CardInstance card = _player.FindHand(uid);
            if (card == null) return false;
            _player.Hand.Remove(card);
            AddFavour(_balance.FavourPerOffering);
            Draw(Side.Player);
            Log?.Invoke($"You offer {card.Data.DisplayName}. The shrine answers.");
            return true;
        }

        public bool CanInvokeBlessing => !_over && _player.Favour >= _balance.FavourMax;

        public bool InvokeBlessing()
        {
            if (!CanInvokeBlessing) return false;
            PatronData patron = _player.Patron;
            switch (patron.Kind)
            {
                case PatronKind.Flame:
                    foreach (CardInstance card in new List<CardInstance>(_opponent.Field))
                        if (card.TakeDamage(4)) DestroyCard(card, Side.Opponent);
                    ApplyHeroDamage(_opponent, 2);
                    break;
                case PatronKind.Tide:
                    HealHero(_player, 5);
                    Draw(Side.Player);
                    break;
                case PatronKind.Stone:
                    foreach (CardInstance card in _player.Field) card.ModifyDefense(2);
                    _player.Shield += 3;
                    break;
                case PatronKind.Gale:
                    _player.Mana = Mathf.Min(_balance.MaxMana, _player.Mana + 3);
                    ManaChanged?.Invoke(Side.Player, _player.Mana, _player.MaxMana);
                    foreach (CardInstance card in _player.Field) card.ResetAttack();
                    break;
            }
            _player.Favour = 0;
            FavourChanged?.Invoke(_player.Favour);
            BlessingInvoked?.Invoke(patron.BlessingName);
            Log?.Invoke($"Divine favour: {patron.BlessingName}!");
            CheckEnd();
            return true;
        }

        public void EndTurn()
        {
            if (_over) return;
            CurrentSide = Other(CurrentSide);
            if (CurrentSide == Side.Player)
            {
                _round++;
                if (_player.MaxMana < _balance.MaxMana) _player.MaxMana++;
                _player.Mana = _player.MaxMana;
                _opponent.Mana = _opponent.MaxMana;
                ManaChanged?.Invoke(Side.Player, _player.Mana, _player.MaxMana);
                ManaChanged?.Invoke(Side.Opponent, _opponent.Mana, _opponent.MaxMana);
                foreach (CardInstance card in _player.Field) card.ResetAttack();
                RollOmen();
            }
            else
            {
                foreach (CardInstance card in _opponent.Field) card.ResetAttack();
            }
            TurnChanged?.Invoke(CurrentSide);
        }

        public void Draw(Side side)
        {
            Participant p = ParticipantFor(side);
            if (p.Hand.Count >= _balance.MaxHandSize) return;
            if (p.DrawPile.Count == 0)
            {
                ApplyHeroDamage(p, 2);
                HealthChanged?.Invoke(side, p.Health, p.MaxHealth);
                Log?.Invoke($"{p.DisplayName} suffers 2 fatigue damage.");
                CheckEnd();
                return;
            }
            int index = _random.Next(0, p.DrawPile.Count);
            CardData data = p.DrawPile[index];
            p.DrawPile.RemoveAt(index);
            var card = new CardInstance(data);
            if (_currentOmen.DefenseBonus > 0) card.ModifyDefense(_currentOmen.DefenseBonus);
            p.Hand.Add(card);
            CardDrawn?.Invoke(card, side);
        }

        private int EffectiveCost(CardInstance card, Side side)
        {
            int cost = card.Data.ManaCost;
            if (_currentOmen.Id == "gale_whisper" && card.Data.Element == ElementKind.Air && !card.HasUsedPower)
                cost = Mathf.Max(0, cost - 1);
            return cost;
        }

        private int EffectiveAttack(CardInstance attacker, CardInstance defender, Side side)
        {
            int damage = attacker.Data.Attack;
            if (attacker.Data.Element == ElementKind.Fire && defender.Data.Element == ElementKind.Air)
                damage *= 2;
            if (attacker.Data.Element == ElementKind.Fire)
                damage += _currentOmen.AttackBonus;
            return damage;
        }

        private void ApplyHeroDamage(Participant target, int amount)
        {
            int absorbed = Mathf.Min(target.Shield, amount);
            target.Shield -= absorbed;
            target.Health = Mathf.Max(0, target.Health - (amount - absorbed));
            HealthChanged?.Invoke(target.Side, target.Health, target.MaxHealth);
        }

        private void HealHero(Participant target, int amount)
        {
            target.Health = Mathf.Min(target.MaxHealth, target.Health + amount);
            HealthChanged?.Invoke(target.Side, target.Health, target.MaxHealth);
        }

        private void AddFavour(int amount)
        {
            _player.Favour = Mathf.Min(_balance.FavourMax, _player.Favour + amount);
            FavourChanged?.Invoke(_player.Favour);
        }

        private void DestroyCard(CardInstance card, Side side)
        {
            Participant p = ParticipantFor(side);
            p.Field.Remove(card);
            CardDestroyed?.Invoke(card, side);
            if (side == Side.Player) AddFavour(_balance.FavourPerCardKill);
        }

        private void RollOmen()
        {
            if (_omens.Count == 0) return;
            _currentOmen = _omens[_random.Next(0, _omens.Count)];
            OmenChanged?.Invoke(_currentOmen);
        }

        private void CheckEnd()
        {
            if (_over) return;
            if (_opponent.Health <= 0)
            {
                _over = true;
                BattleEnded?.Invoke(true);
            }
            else if (_player.Health <= 0)
            {
                _over = true;
                BattleEnded?.Invoke(false);
            }
        }

        private static Side Other(Side side) => side == Side.Player ? Side.Opponent : Side.Player;
    }
}
