using Shrine.Audio;
using Shrine.Core;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shrine.Data;
using Shrine.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>Battle presentation: binds <see cref="BattleModel"/> events to the UI.</summary>
    public sealed class BattleScreen : ShrineScreen
    {
        private readonly Dictionary<int, CardView> _views = new Dictionary<int, CardView>();
        private readonly List<RectTransform> _favourRays = new List<RectTransform>();
        private BattleModel _model;
        private OpponentAI _ai;
        private Transform _handRoot;
        private Transform _playerFieldRoot;
        private Transform _opponentFieldRoot;
        private RectTransform _opponentAvatarRect;
        private RectTransform _altarRect;
        private RectTransform _playerAvatarRect;
        private Image _playerAvatar;
        private Image _opponentAvatar;
        private TextMeshProUGUI _playerHealth;
        private TextMeshProUGUI _opponentHealth;
        private TextMeshProUGUI _manaText;
        private TextMeshProUGUI _roundText;
        private TextMeshProUGUI _omenTitle;
        private TextMeshProUGUI _omenDescription;
        private TextMeshProUGUI _favourText;
        private TextMeshProUGUI _feedback;
        private RectTransform _omenBanner;
        private ShrineButton _endTurn;
        private ShrineButton _blessing;
        private Coroutine _enemyRoutine;
        private bool _bound;

        public void Bind(BattleModel model, OpponentAI ai)
        {
            _model = model;
            _ai = ai;
            if (_bound) return;
            _bound = true;
            model.CardDrawn += OnCardDrawn;
            model.CardPlayed += OnCardPlayed;
            model.CardDestroyed += OnCardDestroyed;
            model.CombatResolved += OnCombat;
            model.HeroDamaged += OnHeroDamaged;
            model.HealthChanged += OnHealthChanged;
            model.ManaChanged += OnManaChanged;
            model.TurnChanged += OnTurnChanged;
            model.OmenChanged += OnOmenChanged;
            model.FavourChanged += OnFavourChanged;
            model.BattleEnded += OnBattleEnded;
            model.BlessingInvoked += OnBlessing;
            model.Log += OnLog;
        }

        protected override void Build()
        {
            RectTransform background = UiKit.Stretch(transform, "BattleBackground");
            var backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.sprite = UiKit.LoadSprite("FotosManageDeckScene/fondomitico");
            backgroundImage.color = ShrinePalette.WithAlpha(Color.white, 0.5f);
            backgroundImage.raycastTarget = false;
            background.DOScale(1.04f, 18f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            UiKit.Image(transform, "Vignette", ProceduralArt.Vignette(), ShrinePalette.WithAlpha(Color.white, 0.9f));

            RectTransform top = UiKit.Stretch(transform, "TopBar", 60, 0, 60, 940);
            var topImage = top.gameObject.AddComponent<Image>();
            topImage.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.72f);
            topImage.raycastTarget = false;
            _roundText = UiKit.TextAt(transform, "Round", string.Empty, 30, ShrinePalette.Candlelight,
                new Vector2(280, 50), new Vector2(-730, 480), true);

            _omenBanner = UiKit.Centered(transform, "OmenBanner", new Vector2(620, 92), new Vector2(0, 474));
            var omenImage = _omenBanner.gameObject.AddComponent<Image>();
            omenImage.sprite = UiKit.LoadSprite("FotosMainMenu/MainMenuBoton");
            omenImage.type = Image.Type.Sliced;
            omenImage.color = ShrinePalette.WithAlpha(ShrinePalette.TwilightIndigo, 0.9f);
            omenImage.raycastTarget = false;
            _omenTitle = UiKit.Text(_omenBanner, "OmenTitle", string.Empty, 26, ShrinePalette.Candlelight, true);
            _omenTitle.rectTransform.sizeDelta = new Vector2(600, 38);
            _omenTitle.rectTransform.anchoredPosition = new Vector2(0, 16);
            _omenDescription = UiKit.Text(_omenBanner, "OmenDescription", string.Empty, 18, ShrinePalette.BoneParchment);
            _omenDescription.rectTransform.sizeDelta = new Vector2(600, 34);
            _omenDescription.rectTransform.anchoredPosition = new Vector2(0, -18);

            UiKit.Button(transform, "Pause", "II", new Vector2(70, 56), new Vector2(850, 480),
                App.TogglePause, false);

            _opponentAvatarRect = UiKit.Centered(transform, "OpponentPortrait", new Vector2(150, 180),
                new Vector2(720, 350));
            _opponentAvatar = _opponentAvatarRect.gameObject.AddComponent<Image>();
            _opponentAvatar.sprite = UiKit.LoadSprite("FotosMenuInicial/JeanGenius");
            _opponentAvatar.preserveAspect = true;
            _opponentAvatar.raycastTarget = true;
            _opponentHealth = UiKit.TextAt(transform, "OpponentHealth", string.Empty, 28, ShrinePalette.BloodEmber,
                new Vector2(300, 46), new Vector2(500, 232), true);
            UiKit.TextAt(transform, "OpponentName", "THE RIVAL SHRINE", 22, ShrinePalette.BoneParchment,
                new Vector2(360, 40), new Vector2(720, 232), true);

            RectTransform opponentField = UiKit.Centered(transform, "OpponentFieldArea", new Vector2(900, 220),
                new Vector2(0, 150));
            _opponentFieldRoot = opponentField;

            _altarRect = UiKit.Centered(transform, "ShrineAltar", new Vector2(260, 260), new Vector2(0, -30));
            Image altarImage = _altarRect.gameObject.AddComponent<Image>();
            altarImage.sprite = ProceduralArt.Altar();
            altarImage.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.7f);
            altarImage.raycastTarget = true;
            Image altarGlow = UiKit.ImageAt(_altarRect, "AltarGlow", ProceduralArt.RadialGlow(ShrinePalette.SpiritCyan),
                ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.16f), new Vector2(340, 340), Vector2.zero);
            altarGlow.DOColor(ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.32f), 2.4f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI / 6f;
                RectTransform ray = UiKit.ImageAt(transform, "FavourRay" + i, ProceduralArt.Mote(),
                    ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0f), new Vector2(16, 16),
                    new Vector2(Mathf.Cos(angle) * 150f, -30f + Mathf.Sin(angle) * 150f))
                    .rectTransform;
                _favourRays.Add(ray);
            }
            _favourText = UiKit.TextAt(transform, "FavourText", string.Empty, 24, ShrinePalette.Candlelight,
                new Vector2(260, 40), new Vector2(0, -30), true);
            _blessing = UiKit.Button(transform, "BlessingButton", "INVOKE BLESSING", new Vector2(280, 60),
                new Vector2(0, -160), () => _model.InvokeBlessing());
            _blessing.SetInteractable(false);

            UiKit.Button(transform, "OfferingButton", "OFFER", new Vector2(150, 48), new Vector2(220, -30), OfferHint, false);

            RectTransform playerField = UiKit.Centered(transform, "PlayerFieldArea", new Vector2(900, 220),
                new Vector2(0, -230));
            _playerFieldRoot = playerField;

            _playerAvatarRect = UiKit.Centered(transform, "PlayerPortrait", new Vector2(150, 180),
                new Vector2(-720, 350));
            _playerAvatar = _playerAvatarRect.gameObject.AddComponent<Image>();
            _playerAvatar.sprite = UiKit.LoadSprite("FotosMenuInicial/AleGod");
            _playerAvatar.preserveAspect = true;
            _playerAvatar.raycastTarget = false;

            _playerHealth = UiKit.TextAt(transform, "PlayerHealth", string.Empty, 28, ShrinePalette.MossDevotion,
                new Vector2(300, 46), new Vector2(-500, 240), true);
            UiKit.TextAt(transform, "PlayerName", "PILGRIM", 22, ShrinePalette.BoneParchment,
                new Vector2(360, 40), new Vector2(-720, 240), true);

            RectTransform manaRow = UiKit.Centered(transform, "ManaRow", new Vector2(360, 46), new Vector2(-620, -170));
            _manaText = UiKit.Text(manaRow, "ManaLabel", string.Empty, 26, ShrinePalette.SpiritCyan, true);

            RectTransform hand = UiKit.Centered(transform, "HandArea", new Vector2(1100, 220), new Vector2(0, -430));
            _handRoot = hand;

            _endTurn = UiKit.Button(transform, "EndTurnButton", "END TURN", new Vector2(300, 72),
                new Vector2(760, -430), EndPlayerTurn);

            _feedback = UiKit.TextAt(transform, "Feedback", string.Empty, 26, ShrinePalette.Candlelight,
                new Vector2(900, 48), new Vector2(0, -140), false);

            UiKit.Button(transform, "BackToTitle", "LEAVE BATTLE", new Vector2(180, 46),
                new Vector2(-800, -500), App.LeaveBattle, false);
        }

        private void EndPlayerTurn()
        {
            if (_model == null || _model.IsOver || _model.CurrentSide != Side.Player) return;
            _model.EndTurn();
        }

        private void OfferHint()
        {
            if (_model == null || _model.CurrentSide != Side.Player) return;
            App.Tooltip.Show("Drag a card from your hand onto the altar to offer it. "
                + "An offering grants divine favour and draws a replacement.", Input.mousePosition);
        }

        public override void OnEnter()
        {
            if (_model == null || !_model.IsStarted) return;
            RefreshAll();
        }

        public override void OnExit()
        {
            if (_enemyRoutine != null) StopCoroutine(_enemyRoutine);
            _enemyRoutine = null;
            backgroundStop();
        }

        private void backgroundStop()
        {
            transform.DOKill(true);
        }

        private void OnCardDrawn(CardInstance card, Side side)
        {
            CardView view = GetView(card, side == Side.Player ? _handRoot : _opponentFieldRoot);
            view.Rect.SetParent(side == Side.Player ? _handRoot : _opponentFieldRoot, false);
            Layout();
            view.PlayEnter(0.05f * (_model.ParticipantFor(side).Hand.Count % 6));
            UiAudioBus.PlayCardDraw();
        }

        private void OnCardPlayed(CardInstance card, Side side)
        {
            CardView view = GetView(card, side == Side.Player ? _playerFieldRoot : _opponentFieldRoot);
            view.Rect.SetParent(side == Side.Player ? _playerFieldRoot : _opponentFieldRoot, false);
            Layout();
            view.Refresh(side == Side.Player && _model.CanAttack(side, card.Uid));
            UiAudioBus.PlayCardPlay();
            App.Vfx.Burst(_altarRect, Vector2.zero, ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.8f), 8);
        }

        private void OnCardDestroyed(CardInstance card, Side side)
        {
            if (!_views.TryGetValue(card.Uid, out CardView view)) return;
            _views.Remove(card.Uid);
            Vector2 position = view.Rect.anchoredPosition;
            App.Vfx.Burst(transform, position, ShrinePalette.BloodEmber, 12);
            view.Rect.DOScale(Vector3.zero, 0.24f).SetEase(Ease.InCubic)
                .OnComplete(() => Destroy(view.gameObject));
            Layout();
        }

        private void OnCombat(CardInstance attacker, Side attackerSide, CardInstance target,
            Side targetSide, int damage, int retaliation)
        {
            App.Vfx.HitStop(false);
            App.Vfx.Shake(6f, 0.2f);
            UiAudioBus.PlayImpact();
            if (_views.TryGetValue(target.Uid, out CardView targetView))
                HitFlash(targetView);
            if (_views.TryGetValue(attacker.Uid, out CardView attackerView))
                HitFlash(attackerView);
            Vector2 targetPosition = targetView != null ? targetView.Rect.anchoredPosition : Vector2.zero;
            App.Vfx.DamageNumber(transform, targetPosition, damage, damage >= 5);
            RefreshAll();
        }

        private void OnHeroDamaged(CardInstance attacker, Side targetSide, int damage)
        {
            App.Vfx.HitStop(damage >= 5);
            App.Vfx.Shake(damage >= 5 ? 11f : 5f, damage >= 5 ? 0.32f : 0.16f);
            UiAudioBus.PlayImpact();
            RectTransform portrait = targetSide == Side.Player ? _playerAvatarRect : _opponentAvatarRect;
            Image image = targetSide == Side.Player ? _playerAvatar : _opponentAvatar;
            image.DOColor(ShrinePalette.BloodEmber, 0.06f).SetEase(Ease.InQuad)
                .OnComplete(() => image.DOColor(Color.white, 0.35f));
            portrait.DOScale(new Vector3(1.05f, 0.95f, 1f), 0.08f)
                .OnComplete(() => portrait.DOScale(Vector3.one, 0.28f).SetEase(Ease.OutBack, 1.5f));
            App.Vfx.DamageNumber(transform, portrait.anchoredPosition, damage, damage >= 5);
        }

        private void HitFlash(CardView view)
        {
            view.Rect.DOScale(new Vector3(1.08f, 0.92f, 1f), 0.06f)
                .OnComplete(() => view.Rect.DOScale(Vector3.one, 0.26f).SetEase(Ease.OutBack, 1.4f));
        }

        private void OnHealthChanged(Side side, int health, int max)
        {
            RefreshAll();
        }

        private void OnManaChanged(Side side, int mana, int max)
        {
            RefreshAll();
        }

        private void OnTurnChanged(Side side)
        {
            _endTurn.SetInteractable(side == Side.Player && !_model.IsOver);
            _feedback.text = side == Side.Player ? "Your turn. The shrine listens."
                : "The rival shrine is acting…";
            _feedback.alpha = 1f;
            _feedback.DOFade(0f, 0.4f).SetDelay(1.6f);
            if (side == Side.Opponent && _enemyRoutine == null)
                _enemyRoutine = StartCoroutine(OpponentTurn());
            RefreshAll();
        }

        private IEnumerator OpponentTurn()
        {
            yield return new WaitForSeconds(0.55f);
            _model.Draw(Side.Opponent);
            yield return new WaitForSeconds(0.45f);
            List<BattleAction> actions = _ai.PlanTurn();
            foreach (BattleAction action in actions)
            {
                if (_model.IsOver) break;
                switch (action.Kind)
                {
                    case BattleActionKind.Play:
                        _model.PlayCard(Side.Opponent, action.CardUid);
                        break;
                    case BattleActionKind.AttackCard:
                        _model.Attack(Side.Opponent, action.CardUid, action.TargetUid);
                        break;
                    case BattleActionKind.AttackHero:
                        _model.Attack(Side.Opponent, action.CardUid);
                        break;
                }
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.35f);
            if (!_model.IsOver && _model.CurrentSide == Side.Opponent) _model.EndTurn();
            _enemyRoutine = null;
        }

        private void OnOmenChanged(OmenData omen)
        {
            _omenTitle.text = omen.Title.ToUpperInvariant();
            _omenDescription.text = omen.Description;
            _omenBanner.DOScale(1.06f, 0.22f).SetEase(Ease.OutBack, 1.4f)
                .OnComplete(() => _omenBanner.DOScale(1f, 0.3f).SetEase(Ease.OutCubic));
        }

        private void OnFavourChanged(int favour)
        {
            int lit = Mathf.RoundToInt(favour / (float)Mathf.Max(1, _favourRays.Count));
            for (int i = 0; i < _favourRays.Count; i++)
                _favourRays[i].GetComponent<Image>().color = i < lit
                    ? ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.95f)
                    : ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.08f);
            _favourText.text = $"DIVINE FAVOUR  {favour}%";
            _blessing.SetInteractable(favour >= ShrineDatabase.Balance.FavourMax);
        }

        private void OnBlessing(string name)
        {
            _feedback.text = name.ToUpperInvariant() + "!";
            _feedback.alpha = 1f;
            App.Vfx.Shake(12f, 0.35f);
            App.Vfx.HitStop(true);
            App.Vfx.Burst(_altarRect, Vector2.zero, ShrinePalette.SpiritCyan, 24);
            UiAudioBus.PlayBlessing();
        }

        private void OnLog(string message)
        {
            _feedback.text = message;
            _feedback.alpha = 0.9f;
            _feedback.DOFade(0f, 0.4f).SetDelay(2f);
        }

        private void OnBattleEnded(bool won)
        {
            _endTurn.SetInteractable(false);
            UiAudioBus.PlaySound(won ? UiSound.Victory : UiSound.Defeat);
            App.ShowGameOver(won);
        }

        private CardView GetView(CardInstance card, Transform parent)
        {
            if (_views.TryGetValue(card.Uid, out CardView existing)) return existing;
            var view = new GameObject("Card_" + card.Data.Id).AddComponent<CardView>();
            view.Initialize(parent, new Vector2(150, 214), false);
            view.Clicked += OnCardClicked;
            view.Hovered += OnCardHovered;
            view.DragEnded += OnCardDragEnded;
            view.DragAllowed += CanDrag;
            view.Bind(card, true, false);
            _views[card.Uid] = view;
            return view;
        }

        private bool CanDrag(CardView view)
        {
            return _model != null && !_model.IsOver && _model.CurrentSide == Side.Player
                && (_model.Player.Hand.Contains(view.Card) || _model.Player.Field.Contains(view.Card));
        }

        private void OnCardClicked(CardView view)
        {
            if (_model == null || _model.CurrentSide != Side.Player) return;
            if (_model.Player.Hand.Contains(view.Card))
            {
                if (!_model.PlayCard(Side.Player, view.Card.Uid)) UiAudioBus.PlayDeny();
                RefreshAll();
            }
        }

        private void OnCardHovered(CardView view)
        {
            CardData data = view.Card.Data;
            string rule = data.Element switch
            {
                ElementKind.Fire => "Fire: +1 damage in Emberfall; double vs Air.",
                ElementKind.Water => "Water: restores 2 defence on its first strike.",
                ElementKind.Earth => "Earth: ignores the first damage it would take.",
                _ => "Air: ignores the first mana cost of its kind."
            };
            App.Tooltip.Show($"<b>{data.DisplayName}</b>\n{data.Lore}\n\n"
                + $"<color=#D9B36C>ATK {data.Attack}  DEF {view.Card.CurrentDefense}/{view.Card.MaxDefense}  COST {data.ManaCost}</color>\n{rule}",
                Input.mousePosition);
        }

        private void OnCardDragEnded(CardView view, Vector2 screenPosition)
        {
            if (_model == null || _model.CurrentSide != Side.Player) return;
            bool offered = RectTransformUtility.RectangleContainsScreenPoint(_altarRect, screenPosition);
            if (offered && _model.Player.Hand.Contains(view.Card))
            {
                _model.Offer(view.Card.Uid);
                RefreshAll();
                return;
            }
            if (_model.Player.Field.Contains(view.Card))
            {
                foreach (KeyValuePair<int, CardView> pair in _views)
                {
                    if (pair.Value == view || !_model.Opponent.Field.Contains(pair.Value.Card)) continue;
                    if (RectTransformUtility.RectangleContainsScreenPoint(pair.Value.Rect, screenPosition))
                    {
                        _model.Attack(Side.Player, view.Card.Uid, pair.Key);
                        RefreshAll();
                        return;
                    }
                }
                if (_model.Opponent.Field.Count == 0 &&
                    RectTransformUtility.RectangleContainsScreenPoint(_opponentAvatarRect, screenPosition))
                {
                    _model.Attack(Side.Player, view.Card.Uid);
                }
            }
            Layout();
        }

        private void RefreshAll()
        {
            if (_model == null) return;
            Participant player = _model.Player;
            Participant opponent = _model.Opponent;
            _roundText.text = $"ROUND {_model.Round}";
            _playerHealth.text = $"{player.Health} / {player.MaxHealth}";
            _opponentHealth.text = $"{opponent.Health} / {opponent.MaxHealth}";
            _manaText.text = $"MANA  {player.Mana} / {player.MaxMana}";
            _favourText.text = $"DIVINE FAVOUR  {player.Favour}%";
            _playerAvatar.sprite = UiKit.LoadSprite(player.Patron.AvatarResourcePath);
            _opponentAvatar.sprite = UiKit.LoadSprite(opponent.Patron.AvatarResourcePath);
            foreach (CardView view in _views.Values)
            {
                if (view.Card == null) continue;
                bool inHand = player.Hand.Contains(view.Card);
                bool ready = inHand ? _model.CanPlay(Side.Player, view.Card.Uid)
                    : _model.CanAttack(Side.Player, view.Card.Uid);
                view.Refresh(ready);
            }
            Layout();
        }

        private void Layout()
        {
            if (_model == null) return;
            LayoutRow(_model.Player.Hand, _handRoot, 136f, -8f);
            LayoutRow(_model.Player.Field, _playerFieldRoot, 168f, 0f);
            LayoutRow(_model.Opponent.Field, _opponentFieldRoot, 168f, 0f);
        }

        private void LayoutRow(IReadOnlyList<CardInstance> cards, Transform root, float spacing, float arc)
        {
            int count = cards.Count;
            float width = (count - 1) * spacing;
            for (int i = 0; i < count; i++)
            {
                if (!_views.TryGetValue(cards[i].Uid, out CardView view)) continue;
                if (view.Rect.parent != root) view.Rect.SetParent(root, false);
                float x = -width * 0.5f + i * spacing;
                float y = count > 1 ? -Mathf.Abs(i - (count - 1) * 0.5f) * arc : 0f;
                view.SetHome(new Vector2(x, y));
                float tilt = count > 1 ? (i - (count - 1) * 0.5f) * -3f : 0f;
                view.Rect.DOAnchorPos(new Vector2(x, y), 0.32f).SetEase(Ease.OutCubic);
                view.Rect.DOLocalRotate(new Vector3(0f, 0f, tilt), 0.32f).SetEase(Ease.OutCubic);
                view.Rect.SetAsLastSibling();
            }
        }

        private void OnDestroy()
        {
            if (!_bound || _model == null) return;
            _model.CardDrawn -= OnCardDrawn;
            _model.CardPlayed -= OnCardPlayed;
            _model.CardDestroyed -= OnCardDestroyed;
            _model.CombatResolved -= OnCombat;
            _model.HeroDamaged -= OnHeroDamaged;
            _model.HealthChanged -= OnHealthChanged;
            _model.ManaChanged -= OnManaChanged;
            _model.TurnChanged -= OnTurnChanged;
            _model.OmenChanged -= OnOmenChanged;
            _model.FavourChanged -= OnFavourChanged;
            _model.BattleEnded -= OnBattleEnded;
            _model.BlessingInvoked -= OnBlessing;
            _model.Log -= OnLog;
        }
    }
}
