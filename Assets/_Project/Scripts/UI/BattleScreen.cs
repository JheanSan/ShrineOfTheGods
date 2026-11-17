using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shrine.Audio;
using Shrine.Core;
using Shrine.Data;
using Shrine.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>
    /// Battle presentation. The layout answers three questions at a glance: how much
    /// life each shrine has, what is on the board, and what the current player may do.
    /// </summary>
    public sealed class BattleScreen : ShrineScreen
    {
        private const float CardWidth = 172f;
        private const float CardHeight = 230f;
        private const float FieldSpacing = 186f;
        private const float MaxHandSpacing = 186f;
        private const float HandWidth = 1040f;
        private const float FieldCardScale = 0.98f;
        private const float HandCardScale = 0.92f;
        private const float RivalFieldY = 302f;
        private const float PlayerFieldY = -150f;
        private const float AltarY = 65f;

        private readonly Dictionary<int, CardView> _views = new Dictionary<int, CardView>();
        private readonly Stack<CardView> _pool = new Stack<CardView>();
        private readonly List<RectTransform> _favourRays = new List<RectTransform>();
        private readonly List<Image> _playerSlots = new List<Image>();

        private BattleModel _model;
        private OpponentAI _ai;

        private RectTransform _background;
        private RectTransform _handRoot;
        private RectTransform _playerFieldRoot;
        private RectTransform _opponentFieldRoot;
        private RectTransform _altarRect;
        private RectTransform _arrow;
        private Image _arrowImage;
        private Image _altarGlow;
        private HeroPlate _playerPlate;
        private HeroPlate _rivalPlate;
        private ManaBar _manaBar;

        private TextMeshProUGUI _roundText;
        private TextMeshProUGUI _turnPhase;
        private TextMeshProUGUI _deckText;
        private TextMeshProUGUI _omenTitle;
        private TextMeshProUGUI _omenDescription;
        private TextMeshProUGUI _favourText;
        private TextMeshProUGUI _feedback;

        private RectTransform _omenBanner;
        private Image _playerBoard;
        private RectTransform _turnBanner;
        private CanvasGroup _turnBannerGroup;
        private TextMeshProUGUI _turnBannerText;
        private ShrineButton _endTurn;
        private ShrineButton _blessing;
        private Image _blessingGlow;
        private RectTransform _forecast;
        private CanvasGroup _forecastGroup;
        private TextMeshProUGUI _forecastTitle;
        private TextMeshProUGUI _forecastBody;
        private Coroutine _enemyRoutine;
        private CardView _dragging;
        private bool _bound;

        public void Bind(BattleModel model, OpponentAI ai)
        {
            if (_model != model) ResetViews();
            _model = model;
            _ai = ai;
            OnFavourChanged(model.Player.Favour);
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
            _background = UiKit.Stretch(transform, "BattleBackground");
            var backgroundImage = _background.gameObject.AddComponent<Image>();
            backgroundImage.sprite = UiKit.LoadSprite("FotosManageDeckScene/fondomitico");
            backgroundImage.color = ShrinePalette.WithAlpha(Color.white, 0.62f);
            backgroundImage.raycastTarget = false;
            _background.DOScale(1.04f, 18f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            UiKit.Image(transform, "VerticalShade", ProceduralArt.VerticalShade(), Color.white);
            UiKit.Image(transform, "Vignette", ProceduralArt.Vignette(),
                ShrinePalette.WithAlpha(Color.white, 0.75f));

            BuildTopBar();
            BuildBoards();
            BuildHeroes();
            BuildAltar();
            BuildRightRail();

            _handRoot = UiKit.Centered(transform, "HandArea", new Vector2(1100, 244),
                new Vector2(40f, -390f));
            _playerFieldRoot = UiKit.Centered(transform, "PlayerFieldArea", new Vector2(1020, 240),
                new Vector2(40f, PlayerFieldY));
            _opponentFieldRoot = UiKit.Centered(transform, "OpponentFieldArea", new Vector2(1020, 240),
                new Vector2(40f, RivalFieldY));

            BuildArrow();
            BuildTurnBanner();
            BuildElementLegend();
            BuildForecast();
            App.Vfx.AmbientEmbers(transform, new Vector2(0f, -430f), 14);
        }

        /// <summary>Wide stone band that sweeps in each turn so the handover is unmistakable.</summary>
        private void BuildTurnBanner()
        {
            _turnBanner = UiKit.Centered(transform, "TurnBanner", new Vector2(620, 84),
                new Vector2(0f, 146f));
            _turnBannerGroup = _turnBanner.gameObject.AddComponent<CanvasGroup>();
            _turnBannerGroup.blocksRaycasts = false;
            _turnBannerGroup.alpha = 0f;

            Image plate = UiKit.Image(_turnBanner, "Plate", ProceduralArt.StonePlate(),
                ShrinePalette.WithAlpha(Color.white, 0.97f));
            plate.type = Image.Type.Sliced;
            plate.raycastTarget = false;

            UiKit.ImageAt(_turnBanner, "TopRule", null,
                ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.75f),
                new Vector2(560, 2f), new Vector2(0f, 30f));
            UiKit.ImageAt(_turnBanner, "BottomRule", null,
                ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.75f),
                new Vector2(560, 2f), new Vector2(0f, -30f));

            _turnBannerText = UiKit.Text(_turnBanner, "Label", string.Empty, 32,
                ShrinePalette.Candlelight, true);
            _turnBannerText.fontStyle = FontStyles.Bold;
        }

        private void AnnounceTurn(bool playerTurn)
        {
            if (_turnBanner == null) return;
            _turnBannerText.text = playerTurn ? "YOUR TURN" : "RIVAL'S TURN";
            _turnBannerText.color = playerTurn ? ShrinePalette.Candlelight : ShrinePalette.MoonSilver;
            _turnBanner.DOKill();
            _turnBannerGroup.DOKill();
            _turnBannerGroup.alpha = 0f;
            _turnBanner.anchoredPosition = new Vector2(-120f, 146f);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_turnBannerGroup.DOFade(1f, 0.14f));
            sequence.Join(_turnBanner.DOAnchorPos(new Vector2(0f, 146f), 0.40f).SetEase(Ease.OutCubic));
            sequence.AppendInterval(0.55f);
            sequence.Append(_turnBannerGroup.DOFade(0f, 0.34f));
            sequence.Join(_turnBanner.DOAnchorPos(new Vector2(120f, 146f), 0.34f)
                .SetEase(Ease.InCubic));
        }

        private void BuildTopBar()
        {
            RectTransform bar = UiKit.Centered(transform, "TopBar", new Vector2(1920, 76),
                new Vector2(0f, 502f));
            var barImage = bar.gameObject.AddComponent<Image>();
            barImage.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.62f);
            barImage.raycastTarget = false;
            var rule = UiKit.ImageAt(bar, "Rule", null,
                ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.30f),
                new Vector2(1920, 1.5f), new Vector2(0f, -37f));
            rule.raycastTarget = false;

            _roundText = UiKit.TextAt(bar, "Round", string.Empty, 22, ShrinePalette.EmberGold,
                new Vector2(280, 40), new Vector2(-790, 0), true, TextAlignmentOptions.Left);

            _omenBanner = UiKit.Centered(bar, "OmenBanner", new Vector2(720, 70), Vector2.zero);
            var omenImage = _omenBanner.gameObject.AddComponent<Image>();
            omenImage.sprite = ProceduralArt.StonePlate();
            omenImage.type = Image.Type.Sliced;
            omenImage.color = ShrinePalette.WithAlpha(Color.white, 0.92f);
            omenImage.raycastTarget = false;
            _omenTitle = UiKit.TextAt(_omenBanner, "OmenTitle", string.Empty, 21,
                ShrinePalette.Candlelight, new Vector2(680, 28), new Vector2(0f, 14f), true);
            _omenDescription = UiKit.TextAt(_omenBanner, "OmenDescription", string.Empty, 15,
                ShrinePalette.BoneParchment, new Vector2(680, 24), new Vector2(0f, -14f), false);

            UiKit.Button(bar, "Pause", "II", new Vector2(70, 48), new Vector2(880f, 0f),
                App.TogglePause, false);
        }

        private void BuildBoards()
        {
            AddBoardPanel("RivalBoard", RivalFieldY);
            _playerBoard = AddBoardPanel("PilgrimBoard", PlayerFieldY);
            AddSlots("RivalSlots", RivalFieldY, null);
            AddSlots("PilgrimSlots", PlayerFieldY, _playerSlots);
        }

        private Image AddBoardPanel(string name, float y)
        {
            Image panel = UiKit.ImageAt(transform, name, ProceduralArt.BoardPanel(),
                Color.white, new Vector2(1020, 240), new Vector2(40f, y));
            panel.type = Image.Type.Sliced;
            panel.raycastTarget = false;
            return panel;
        }

        private void AddSlots(string name, float y, List<Image> sink)
        {
            RectTransform row = UiKit.Centered(transform, name, new Vector2(1020, 240), new Vector2(40f, y));
            float width = (5 - 1) * FieldSpacing;
            for (int i = 0; i < 5; i++)
            {
                Image slot = UiKit.ImageAt(row, "Slot" + i, ProceduralArt.SlotFrame(),
                    ShrinePalette.WithAlpha(Color.white, 0.55f), new Vector2(CardWidth, CardHeight),
                    new Vector2(-width * 0.5f + i * FieldSpacing, 0f));
                slot.type = Image.Type.Sliced;
                slot.raycastTarget = false;
                sink?.Add(slot);
            }
        }

        /// <summary>Light the exact slot a dragged hand card would fill.</summary>
        private void HighlightSlots(bool on)
        {
            if (_model == null) return;
            int next = _model.Player.Field.Count;
            for (int i = 0; i < _playerSlots.Count; i++)
            {
                Image slot = _playerSlots[i];
                slot.DOKill();
                bool target = on && i == next;
                slot.DOColor(target
                        ? ShrinePalette.WithAlpha(ShrinePalette.Candlelight, 1f)
                        : ShrinePalette.WithAlpha(Color.white, on ? 0.28f : 0.55f), 0.16f)
                    .SetEase(Ease.OutQuad);
                if (target)
                {
                    slot.rectTransform.DOKill();
                    slot.rectTransform.DOScale(Vector3.one * 1.03f, 0.18f)
                        .SetEase(Ease.OutBack, 1.4f);
                }
                else
                {
                    slot.rectTransform.DOKill();
                    slot.rectTransform.DOScale(Vector3.one, 0.16f);
                }
            }
        }

        private void BuildHeroes()
        {
            _rivalPlate = new GameObject("RivalPlate").AddComponent<HeroPlate>();
            _rivalPlate.Initialize(transform, true, new Vector2(340, 200), new Vector2(-700f, 320f));
            _rivalPlate.SetHandCount(0);

            _playerPlate = new GameObject("PilgrimPlate").AddComponent<HeroPlate>();
            _playerPlate.Initialize(transform, false, new Vector2(340, 200), new Vector2(-700f, -250f));

            RectTransform plaque = UiKit.Centered(transform, "TurnPlaque", new Vector2(320, 130),
                new Vector2(-700f, 20f));
            var plaqueImage = plaque.gameObject.AddComponent<Image>();
            plaqueImage.sprite = ProceduralArt.StonePlate();
            plaqueImage.type = Image.Type.Sliced;
            plaqueImage.color = ShrinePalette.WithAlpha(Color.white, 0.95f);
            plaqueImage.raycastTarget = false;
            _turnPhase = UiKit.TextAt(plaque, "Phase", string.Empty, 36, ShrinePalette.Candlelight,
                new Vector2(280, 44), new Vector2(0f, 24f), true);
            UiKit.ImageAt(plaque, "Rule", null,
                ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.35f),
                new Vector2(240, 1.5f), new Vector2(0f, 4f));
            _deckText = UiKit.TextAt(plaque, "Deck", string.Empty, 15,
                ShrinePalette.WithAlpha(ShrinePalette.BoneParchment, 0.85f),
                new Vector2(280, 26), new Vector2(0f, -24f), false);
        }

        private void BuildAltar()
        {
            _altarRect = UiKit.Centered(transform, "ShrineAltar", new Vector2(200, 200),
                new Vector2(0f, AltarY));
            var altarImage = _altarRect.gameObject.AddComponent<Image>();
            altarImage.sprite = ProceduralArt.Altar();
            altarImage.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.80f);
            altarImage.raycastTarget = false;

            _altarGlow = UiKit.ImageAt(_altarRect, "AltarGlow",
                ProceduralArt.RadialGlow(ShrinePalette.SpiritCyan),
                ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.16f),
                new Vector2(300, 300), Vector2.zero);
            _altarGlow.DOColor(ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.30f), 2.4f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI / 6f;
                RectTransform ray = UiKit.ImageAt(_altarRect, "FavourRay" + i, ProceduralArt.Mote(),
                    ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0f), new Vector2(11, 11),
                    new Vector2(Mathf.Cos(angle) * 78f, Mathf.Sin(angle) * 78f)).rectTransform;
                _favourRays.Add(ray);
            }

            UiKit.TextAt(_altarRect, "FavourLabel", "FAVOUR", 11, ShrinePalette.MoonSilver,
                new Vector2(120, 18), new Vector2(0f, 44f), true);
            _favourText = UiKit.TextAt(_altarRect, "FavourValue", "0%", 34, ShrinePalette.Candlelight,
                new Vector2(120, 42), new Vector2(0f, 6f), true);
            UiKit.TextAt(_altarRect, "OfferHint", "DRAG TO OFFER", 10, ShrinePalette.MoonSilver,
                new Vector2(150, 16), new Vector2(0f, -40f), false);
        }

        private void BuildRightRail()
        {
            _blessingGlow = UiKit.ImageAt(transform, "BlessingGlow",
                ProceduralArt.RadialGlow(ShrinePalette.SpiritCyan),
                ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f),
                new Vector2(340, 140), new Vector2(790f, 64f));
            _blessing = UiKit.Button(transform, "BlessingButton", "BLESSING", new Vector2(300, 64),
                new Vector2(790f, 64f), () => _model.InvokeBlessing(), false);
            _blessing.SetInteractable(false);

            _manaBar = new GameObject("ManaBar").AddComponent<ManaBar>();
            _manaBar.Initialize(transform, new Vector2(752f, -232f));

            _endTurn = UiKit.Button(transform, "EndTurnButton", "END TURN", new Vector2(260, 84),
                new Vector2(790f, -110f), EndPlayerTurn);

            _feedback = UiKit.TextAt(transform, "Feedback", string.Empty, 16, ShrinePalette.BoneParchment,
                new Vector2(330, 110), new Vector2(775f, -392f), false,
                TextAlignmentOptions.TopRight);
        }

        private void BuildArrow()
        {
            _arrow = UiKit.Centered(transform, "TargetArrow", new Vector2(200, 52), Vector2.zero);
            _arrow.pivot = new Vector2(0f, 0.5f);
            _arrowImage = _arrow.gameObject.AddComponent<Image>();
            _arrowImage.sprite = ProceduralArt.Arrow();
            _arrowImage.raycastTarget = false;
            _arrowImage.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.85f);
            _arrow.gameObject.SetActive(false);
        }

        /// <summary>
        /// Compact cheat sheet of the elemental cycle, pinned under the rival plate.
        /// Every fight is decided by these four arrows, so they stay on screen.
        /// </summary>
        private void BuildElementLegend()
        {
            RectTransform panel = UiKit.Centered(transform, "ElementLegend", new Vector2(380, 126),
                new Vector2(-676f, 146f));
            var background = panel.gameObject.AddComponent<Image>();
            background.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.68f);
            background.raycastTarget = false;
            UiKit.ImageAt(panel, "Rule", null, ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.35f),
                new Vector2(344, 1.5f), new Vector2(0f, 54f));
            UiKit.TextAt(panel, "Title", "ELEMENT CYCLE", 12, ShrinePalette.MoonSilver,
                new Vector2(344, 16), new Vector2(0f, 43f), true).characterSpacing = 4f;

            ElementKind[] order =
            {
                ElementKind.Fire, ElementKind.Air, ElementKind.Earth, ElementKind.Water
            };
            for (int i = 0; i < order.Length; i++)
            {
                ElementKind from = order[i];
                ElementKind to = ElementRules.Beats(from);
                float y = 20f - i * 22f;
                Image glyph = UiKit.ImageAt(panel, "Glyph" + i, ProceduralArt.ElementSigil(from),
                    ShrinePalette.Element(from), Vector2.one * 18f, new Vector2(-164f, y));
                glyph.raycastTarget = false;
                string row = $"<color=#{ShrinePalette.Rgb(ShrinePalette.Element(from))}>"
                    + $"{from.ToString().ToUpperInvariant()}</color> <color=#8A6538>></color> "
                    + $"<color=#{ShrinePalette.Rgb(ShrinePalette.Element(to))}>"
                    + $"{to.ToString().ToUpperInvariant()}</color>"
                    + "  <size=72%><color=#B9C2D6>DOUBLE</color></size>";
                TextMeshProUGUI label = UiKit.TextAt(panel, "Row" + i, row, 16, ShrinePalette.BoneParchment,
                    new Vector2(326, 20), new Vector2(14f, y), false, TextAlignmentOptions.Left);
                label.characterSpacing = 1f;
            }
        }

        /// <summary>Reads out the maths of a strike before the card leaves the hand.</summary>
        private void BuildForecast()
        {
            _forecast = UiKit.Centered(transform, "CombatForecast", new Vector2(320, 108),
                new Vector2(760f, 214f));
            var background = _forecast.gameObject.AddComponent<Image>();
            background.sprite = ProceduralArt.StonePlate();
            background.type = Image.Type.Sliced;
            background.color = ShrinePalette.WithAlpha(Color.white, 0.97f);
            background.raycastTarget = false;
            _forecastGroup = _forecast.gameObject.AddComponent<CanvasGroup>();
            _forecastGroup.blocksRaycasts = false;
            _forecastGroup.alpha = 0f;
            _forecastTitle = UiKit.TextAt(_forecast, "Title", string.Empty, 16, ShrinePalette.Candlelight,
                new Vector2(288, 24), new Vector2(0f, 36f), true);
            _forecastBody = UiKit.TextAt(_forecast, "Body", string.Empty, 14, ShrinePalette.BoneParchment,
                new Vector2(288, 62), new Vector2(0f, -10f), false, TextAlignmentOptions.Top);
        }

        private void EndPlayerTurn()
        {
            if (_model == null || _model.IsOver || _model.CurrentSide != Side.Player) return;
            _model.EndTurn();
        }

        public override void OnEnter()
        {
            if (_model == null) return;
            RefreshAll();
            StartCoroutine(RefreshAfterStart());
        }

        /// <summary>
        /// The model starts one frame after the router enters this screen, so the first
        /// refresh lands too early. This second pass is the one that dresses the board.
        /// </summary>
        private IEnumerator RefreshAfterStart()
        {
            yield return null;
            if (_model == null || !_model.IsStarted) yield break;
            RefreshAll();
            UiAudioBus.PlayTurn();
        }

        public override void OnExit()
        {
            if (_enemyRoutine != null) StopCoroutine(_enemyRoutine);
            _enemyRoutine = null;
            _background.DOKill();
            _background.localScale = Vector3.one;
            HideTargeting();
        }

        /// <summary>The live battle, exposed for automated recording and diagnostics.</summary>
        public BattleModel Model => _model;

        public bool IsPlayerTurn => _model != null && !_model.IsOver && _model.CurrentSide == Side.Player;

        public bool AutoPlay(int uid)
        {
            if (_model == null || !_model.PlayCard(Side.Player, uid)) return false;
            RefreshAll();
            return true;
        }

        public bool AutoAttack(int attackerUid, int targetUid)
        {
            if (_model == null || !_model.Attack(Side.Player, attackerUid, targetUid)) return false;
            RefreshAll();
            return true;
        }

        public bool AutoAttackHero(int attackerUid) =>
            _model != null && _model.Attack(Side.Player, attackerUid);

        public void AutoEndTurn() => EndPlayerTurn();

        private void OnCardDrawn(CardInstance card, Side side)
        {
            if (side == Side.Opponent)
            {
                _rivalPlate.SetHandCount(_model.Opponent.Hand.Count);
                return;
            }
            CardView view = GetView(card, _handRoot);
            view.Rect.SetParent(_handRoot, false);
            Layout();
            view.PlayEnter(0.04f * (_model.Player.Hand.Count % 6));
            UiAudioBus.PlayCardDraw();
        }

        private void OnCardPlayed(CardInstance card, Side side)
        {
            RectTransform root = side == Side.Player ? _playerFieldRoot : _opponentFieldRoot;
            CardView view = GetView(card, root);
            view.Rect.SetParent(root, false);
            if (side == Side.Opponent)
            {
                _rivalPlate.SetHandCount(_model.Opponent.Hand.Count);
                Vector2 from = root.InverseTransformPoint(_rivalPlate.Rect.position);
                view.Rect.anchoredPosition = from;
                view.Rect.localScale = Vector3.one * 0.55f;
                view.Rect.DOScale(Vector3.one, 0.34f).SetEase(Ease.OutBack, 1.7f);
            }
            Layout();
            view.Refresh(side == Side.Player && _model.CanAttack(side, card.Uid),
                side != Side.Player || _model.Player.Mana >= card.Data.ManaCost);
            UiAudioBus.PlayCardPlay();
            // Parent the flourish to the card itself: the view is still gliding from the
            // hand to its slot, so a canvas-space ring is left behind on an empty slot.
            App.Vfx.Burst(view.Rect, Vector2.zero,
                ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.85f), 8);
            App.Vfx.Ring(view.Rect, Vector2.zero, ShrinePalette.EmberGold, 0.7f);
        }

        private void OnCardDestroyed(CardInstance card, Side side)
        {
            if (!_views.TryGetValue(card.Uid, out CardView view)) return;
            _views.Remove(card.Uid);
            Vector2 position = ToCanvas(view.Rect);
            App.Vfx.Burst(transform, position, ShrinePalette.BloodEmber, 14);
            App.Vfx.Burst(transform, position, ShrinePalette.WithAlpha(ShrinePalette.AmethystHaze, 0.9f), 8);
            App.Vfx.Ash(transform, position, 18);
            UiAudioBus.PlayDeath();
            view.PlayDeath(0.34f, () => Recycle(view));
            Layout();
        }

        private void OnCombat(CardInstance attacker, Side attackerSide, CardInstance target,
            Side targetSide, int damage, int retaliation)
        {
            if (!_views.TryGetValue(attacker.Uid, out CardView attackerView) ||
                !_views.TryGetValue(target.Uid, out CardView targetView))
            {
                RefreshAll();
                return;
            }

            Color accent = ShrinePalette.ElementBright(attacker.Data.Element);
            Color reply = ShrinePalette.ElementBright(target.Data.Element);
            bool edge = ElementRules.HasAdvantage(attacker.Data.Element, target.Data.Element);
            RectTransform parent = (RectTransform)attackerView.Rect.parent;
            Vector2 targetLocal = parent.InverseTransformPoint(targetView.Rect.position);
            Vector2 from = ToCanvas(attackerView.Rect);
            Vector2 to = ToCanvas(targetView.Rect);
            App.Vfx.Trail(transform, from, to, accent);
            UiAudioBus.PlayAttack();
            attackerView.PlayAttack(targetLocal, () =>
            {
                App.Vfx.Impact(transform, to, damage, damage >= 5, accent);
                targetView.PlayHit(accent);
                if (edge)
                    App.Vfx.Callout(transform, to, "ELEMENTAL EDGE", accent, 19f);
                UiAudioBus.PlayImpact();
                if (retaliation > 0)
                {
                    DOVirtual.DelayedCall(0.22f, () =>
                    {
                        if (!_views.ContainsKey(attacker.Uid) || !attackerView.gameObject.activeSelf) return;
                        attackerView.PlayHit(reply);
                        App.Vfx.Impact(transform, ToCanvas(attackerView.Rect), retaliation, false, reply);
                        UiAudioBus.PlayBlock();
                    });
                }
                RefreshAll();
            });
        }

        private void OnHeroDamaged(CardInstance attacker, Side targetSide, int damage)
        {
            HeroPlate plate = targetSide == Side.Player ? _playerPlate : _rivalPlate;
            Color accent = attacker != null
                ? ShrinePalette.ElementBright(attacker.Data.Element)
                : ShrinePalette.BloodEmber;
            if (attacker != null && _views.TryGetValue(attacker.Uid, out CardView view))
            {
                RectTransform parent = (RectTransform)view.Rect.parent;
                Vector2 targetLocal = parent.InverseTransformPoint(plate.Rect.position);
                view.PlayAttack(targetLocal, () => HeroImpact(plate, damage, accent));
            }
            else
            {
                HeroImpact(plate, damage, accent);
            }
        }

        private void HeroImpact(HeroPlate plate, int damage, Color? accent = null)
        {
            Color color = accent ?? ShrinePalette.BloodEmber;
            plate.PlayHit();
            Vector2 position = ToCanvas(plate.Rect);
            App.Vfx.Trail(transform, position + new Vector2(0f, -120f), position, color);
            App.Vfx.Impact(transform, position, damage, damage >= 5, color);
            App.Vfx.ScreenFlash(color, 0.24f, 0.40f);
            UiAudioBus.PlayWound();
        }

        private void OnHealthChanged(Side side, int health, int max)
        {
            RefreshAll();
        }

        private void OnManaChanged(Side side, int mana, int max)
        {
            if (side == Side.Player) UiAudioBus.PlayMana();
            RefreshAll();
        }

        private void OnTurnChanged(Side side)
        {
            bool playerTurn = side == Side.Player;
            _endTurn.SetInteractable(playerTurn && !_model.IsOver);
            _endTurn.SetLabel(playerTurn ? "END TURN" : "RIVAL'S TURN");
            _deckText.text = "CARDS REMAIN";
            _feedback.text = playerTurn ? "Your turn. The shrine listens."
                : "The rival shrine is acting.";
            _feedback.alpha = 1f;
            _feedback.DOFade(0f, 0.5f).SetDelay(2.4f);
            AnnounceTurn(playerTurn);
            UiAudioBus.PlayTurn();
            if (!playerTurn && _enemyRoutine == null && !_model.IsOver)
                _enemyRoutine = StartCoroutine(OpponentTurn());
            RefreshAll();
        }

        private IEnumerator OpponentTurn()
        {
            yield return new WaitForSeconds(0.7f);
            yield return new WaitForSeconds(0.55f);
            List<BattleAction> actions = _ai.PlanTurn();
            Debug.Log($"[AI] round={_model.Round} side={_model.CurrentSide} "
                + $"mana={_model.Opponent.Mana}/{_model.Opponent.MaxMana} "
                + $"hand={_model.Opponent.Hand.Count} field={_model.Opponent.Field.Count} "
                + $"actions={actions.Count} {_ai.LastPlanSummary}");
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
                yield return new WaitForSeconds(0.85f);
            }
            yield return new WaitForSeconds(0.45f);
            Debug.Log($"[AI] done field={_model.Opponent.Field.Count} mana={_model.Opponent.Mana}");
            if (!_model.IsOver && _model.CurrentSide == Side.Opponent) _model.EndTurn();
            _enemyRoutine = null;
        }

        private void OnOmenChanged(OmenData omen)
        {
            _omenTitle.text = omen.Title.ToUpperInvariant();
            _omenDescription.text = omen.Description;
            _omenBanner.DOScale(1.05f, 0.22f).SetEase(Ease.OutBack, 1.4f)
                .OnComplete(() => _omenBanner.DOScale(1f, 0.3f).SetEase(Ease.OutCubic));
        }

        private void OnFavourChanged(int favour)
        {
            int lit = Mathf.RoundToInt(favour / 100f * _favourRays.Count);
            for (int i = 0; i < _favourRays.Count; i++)
                _favourRays[i].GetComponent<Image>().color = i < lit
                    ? ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.95f)
                    : ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.08f);
            _favourText.text = favour + "%";
            bool ready = favour >= ShrineDatabase.Balance.FavourMax;
            _blessing.SetInteractable(ready);
            _blessing.SetLabel(ready ? "CLAIM BLESSING" : $"BLESSING  {favour}%");
            _blessingGlow.color = ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, ready ? 0.22f : 0f);
        }

        private void OnBlessing(string name)
        {
            _feedback.text = name.ToUpperInvariant() + "!";
            _feedback.alpha = 1f;
            App.Vfx.Shake(12f, 0.35f);
            App.Vfx.HitStop(true);
            App.Vfx.ScreenFlash(ShrinePalette.SpiritCyan, 0.22f, 0.5f);
            Vector2 altar = ToCanvas(_altarRect);
            App.Vfx.Beam(transform, altar, ShrinePalette.SpiritCyan);
            App.Vfx.Ring(transform, altar, ShrinePalette.SpiritCyan, 1.4f);
            App.Vfx.Burst(transform, altar, ShrinePalette.SpiritCyan, 26);
            UiAudioBus.PlayBlessing();
        }

        private void OnLog(string message)
        {
            _feedback.text = message;
            _feedback.alpha = 0.92f;
            _feedback.DOFade(0f, 0.5f).SetDelay(2.6f);
        }

        private void OnBattleEnded(bool won)
        {
            _endTurn.SetInteractable(false);
            _endTurn.SetAttention(false);
            _turnPhase.text = won ? "VICTORY" : "DEFEAT";
            _turnPhase.color = won ? ShrinePalette.Candlelight : ShrinePalette.BloodEmber;
            _deckText.text = "BATTLE OVER";
            UiAudioBus.PlaySound(won ? UiSound.Victory : UiSound.Defeat);
            App.Vfx.ScreenFlash(won ? ShrinePalette.Candlelight : ShrinePalette.BloodEmber,
                won ? 0.30f : 0.34f, 0.9f);
            App.Vfx.Shake(won ? 9f : 14f, 0.5f);
            if (won) App.Vfx.Burst(transform, ToCanvas(_rivalPlate.Rect),
                ShrinePalette.Candlelight, 26);
            else App.Vfx.Ash(transform, ToCanvas(_playerPlate.Rect), 24);
            App.ShowGameOver(won);
        }

        private CardView GetView(CardInstance card, Transform parent)
        {
            if (_views.TryGetValue(card.Uid, out CardView existing)) return existing;
            CardView view;
            if (_pool.Count > 0)
            {
                view = _pool.Pop();
                view.gameObject.SetActive(true);
                view.Rect.localScale = Vector3.one;
                view.Rect.localRotation = Quaternion.identity;
                view.Rect.DOKill();
            }
            else
            {
                view = new GameObject("CardView").AddComponent<CardView>();
                view.Initialize(parent, new Vector2(CardWidth, CardHeight), false);
                view.Clicked += OnCardClicked;
                view.Hovered += OnCardHovered;
                view.DragStarted += OnCardDragStarted;
                view.Dragging += OnCardDragging;
                view.DragEnded += OnCardDragEnded;
                view.DragAllowed += CanDrag;
            }
            view.Bind(card, true, false);
            _views[card.Uid] = view;
            return view;
        }

        private void Recycle(CardView view)
        {
            view.Rect.DOKill();
            view.gameObject.SetActive(false);
            _pool.Push(view);
        }

        private void ResetViews()
        {
            foreach (CardView view in _views.Values) Recycle(view);
            _views.Clear();
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
                if (!_model.PlayCard(Side.Player, view.Card.Uid))
                {
                    view.PlayDeny();
                    UiAudioBus.PlayDeny();
                }
                RefreshAll();
            }
        }

        private void OnCardHovered(CardView view)
        {
            CardData data = view.Card.Data;
            ElementKind beats = ElementRules.Beats(data.Element);
            ElementKind weak = ElementRules.WeaknessOf(data.Element);
            App.Tooltip.Show($"<b>{data.DisplayName}</b>\n{data.Lore}\n\n"
                + $"<color=#{ShrinePalette.Rgb(ShrinePalette.Element(data.Element))}><b>"
                + $"{data.Element.ToString().ToUpperInvariant()}</b></color>"
                + $"  <color=#D9B36C>ATK {data.Attack}  DEF {view.Card.CurrentDefense}/{view.Card.MaxDefense}"
                + $"  COST {data.ManaCost}</color>\n"
                + $"<color=#{ShrinePalette.Rgb(ShrinePalette.Element(beats))}>x2 vs {beats}</color>"
                + $"   <color=#C4523C>weak to {weak}</color>\n"
                + ElementRules.Description(data.Element),
                Input.mousePosition);
        }

        private void OnCardDragStarted(CardView view)
        {
            _dragging = view;
            bool fromHand = _model.Player.Hand.Contains(view.Card);
            _arrow.gameObject.SetActive(true);
            _arrowImage.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.85f);
            if (fromHand)
            {
                PulseTarget(_altarRect, true);
                HighlightBoard(true);
                HighlightSlots(true);
                return;
            }
            foreach (CardView other in _views.Values)
                if (other != view && _model.Opponent.Field.Contains(other.Card))
                    other.SetTargetable(true);
            if (_model.Opponent.Field.Count == 0) _rivalPlate.SetTargetable(true);
        }

        private void OnCardDragging(CardView view, Vector2 screenPosition)
        {
            if (_dragging != view) return;
            Vector2 origin = ToCanvas(view.Rect);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, screenPosition, null, out Vector2 cursor);
            Vector2 delta = cursor - origin;
            _arrow.anchoredPosition = origin;
            _arrow.sizeDelta = new Vector2(Mathf.Max(24f, delta.magnitude), 52f);
            _arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            _arrow.SetAsLastSibling();

            bool valid = IsValidDrop(view, screenPosition);
            _arrowImage.color = valid
                ? ShrinePalette.WithAlpha(ShrinePalette.Candlelight, 0.95f)
                : ShrinePalette.WithAlpha(ShrinePalette.BloodEmber, 0.55f);

            UpdateForecast(view, screenPosition);
        }

        /// <summary>Shows the exact trade the release would cause, or hides the panel.</summary>
        private void UpdateForecast(CardView attacker, Vector2 screenPosition)
        {
            if (_forecast == null || _model.Player.Hand.Contains(attacker.Card))
            {
                HideForecast();
                return;
            }

            foreach (KeyValuePair<int, CardView> pair in _views)
            {
                CardView target = pair.Value;
                if (target == attacker || !_model.Opponent.Field.Contains(target.Card)) continue;
                if (!RectTransformUtility.RectangleContainsScreenPoint(target.Rect, screenPosition)) continue;
                PresentCardForecast(attacker.Card, target.Card);
                return;
            }

            if (_model.Opponent.Field.Count == 0 &&
                RectTransformUtility.RectangleContainsScreenPoint(_rivalPlate.Rect, screenPosition))
            {
                PresentHeroForecast(attacker.Card);
                return;
            }

            HideForecast();
        }

        private void PresentCardForecast(CardInstance attacker, CardInstance target)
        {
            int damage = _model.EffectiveAttack(attacker, target, Side.Player);
            int retaliation = _model.RetaliationDamage(attacker, target);
            bool kill = damage >= target.CurrentDefense;
            bool dies = retaliation >= attacker.CurrentDefense;
            bool edge = ElementRules.HasAdvantage(attacker.Data.Element, target.Data.Element);
            bool weak = ElementRules.HasAdvantage(target.Data.Element, attacker.Data.Element);

            _forecastTitle.text = $"{attacker.Data.DisplayName.ToUpperInvariant()}  >  "
                + target.Data.DisplayName.ToUpperInvariant();
            _forecastTitle.color = ShrinePalette.ElementBright(attacker.Data.Element);

            string body = $"<size=24><b>{damage}</b></size> <color=#B9C2D6>damage</color>"
                + (kill ? "   <color=#D9B36C><b>KILL</b></color>" : string.Empty)
                + $"\n<color=#B9C2D6>reply</color> <b>{retaliation}</b>"
                + (dies
                    ? "   <color=#C4523C><b>CARD DIES</b></color>"
                    : "   <color=#719B7A>card survives</color>");
            if (edge)
                body += $"\n<color=#{ShrinePalette.Rgb(ShrinePalette.Element(attacker.Data.Element))}>"
                    + $"{attacker.Data.Element} > {target.Data.Element}: DOUBLE DAMAGE</color>";
            else if (weak)
                body += $"\n<color=#{ShrinePalette.Rgb(ShrinePalette.BloodEmber)}>"
                    + $"their element answers x2</color>";
            _forecastBody.text = body;
            RevealForecast();
        }

        private void PresentHeroForecast(CardInstance attacker)
        {
            int damage = _model.HeroDamage(attacker);
            _forecastTitle.text = "STRIKE THE RIVAL SHRINE";
            _forecastTitle.color = ShrinePalette.ElementBright(attacker.Data.Element);
            _forecastBody.text = $"<size=24><b>{damage}</b></size> <color=#B9C2D6>damage to the shrine</color>"
                + "\n<color=#719B7A>no retaliation</color>";
            RevealForecast();
        }

        private void RevealForecast()
        {
            _forecast.SetAsLastSibling();
            _forecastGroup.DOKill();
            _forecastGroup.DOFade(1f, 0.10f);
        }

        private void HideForecast()
        {
            if (_forecastGroup == null) return;
            _forecastGroup.DOKill();
            _forecastGroup.DOFade(0f, 0.12f);
        }

        private bool IsValidDrop(CardView view, Vector2 screenPosition)
        {
            if (_model.Player.Hand.Contains(view.Card))
                return RectTransformUtility.RectangleContainsScreenPoint(_altarRect, screenPosition)
                    || RectTransformUtility.RectangleContainsScreenPoint(_playerFieldRoot, screenPosition)
                    || RectTransformUtility.RectangleContainsScreenPoint(_playerBoard.rectTransform, screenPosition);
            foreach (KeyValuePair<int, CardView> pair in _views)
            {
                if (pair.Value == view || !_model.Opponent.Field.Contains(pair.Value.Card)) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(pair.Value.Rect, screenPosition))
                    return true;
            }
            return _model.Opponent.Field.Count == 0 &&
                   RectTransformUtility.RectangleContainsScreenPoint(_rivalPlate.Rect, screenPosition);
        }

        private void OnCardDragEnded(CardView view, Vector2 screenPosition)
        {
            HideTargeting();
            if (_model == null || _model.CurrentSide != Side.Player) return;
            bool fromHand = _model.Player.Hand.Contains(view.Card);
            if (fromHand)
            {
                bool offered = RectTransformUtility.RectangleContainsScreenPoint(_altarRect, screenPosition);
                if (offered)
                {
                    if (_model.Offer(view.Card.Uid))
                    {
                        Vector2 altar = ToCanvas(_altarRect);
                        App.Vfx.Beam(transform, altar, ShrinePalette.Candlelight);
                        App.Vfx.Burst(transform, altar, ShrinePalette.Candlelight, 22);
                        App.Vfx.Burst(transform, altar, ShrinePalette.EmberGold, 10);
                        App.Vfx.Ring(transform, altar, ShrinePalette.Candlelight, 1.1f);
                        UiAudioBus.PlayOffer();
                        RefreshAll();
                        return;
                    }
                    UiAudioBus.PlayDeny();
                }
                bool onBoard = RectTransformUtility.RectangleContainsScreenPoint(_playerFieldRoot, screenPosition)
                    || RectTransformUtility.RectangleContainsScreenPoint(_playerBoard.rectTransform, screenPosition);
                if (onBoard)
                {
                    if (!_model.PlayCard(Side.Player, view.Card.Uid))
                    {
                        view.PlayDeny();
                        UiAudioBus.PlayDeny();
                    }
                    RefreshAll();
                    return;
                }
            }
            if (_model.Player.Field.Contains(view.Card))
            {
                foreach (KeyValuePair<int, CardView> pair in _views)
                {
                    if (pair.Value == view || !_model.Opponent.Field.Contains(pair.Value.Card)) continue;
                    if (RectTransformUtility.RectangleContainsScreenPoint(pair.Value.Rect, screenPosition))
                    {
                        if (_model.Attack(Side.Player, view.Card.Uid, pair.Key)) return;
                        UiAudioBus.PlayDeny();
                    }
                }
                if (_model.Opponent.Field.Count == 0 &&
                    RectTransformUtility.RectangleContainsScreenPoint(_rivalPlate.Rect, screenPosition))
                {
                    if (_model.Attack(Side.Player, view.Card.Uid)) return;
                    UiAudioBus.PlayDeny();
                }
            }
            Layout();
        }

        private void HideTargeting()
        {
            HideForecast();
            if (_arrow != null) _arrow.gameObject.SetActive(false);
            if (_dragging != null) _dragging = null;
            _altarRect.DOKill();
            _altarRect.localScale = Vector3.one;
            HighlightBoard(false);
            HighlightSlots(false);
            _rivalPlate?.SetTargetable(false);
            foreach (CardView other in _views.Values) other.SetTargetable(false);
        }

        /// <summary>Warm the player's board while a hand card hovers over it.</summary>
        private void HighlightBoard(bool on)
        {
            if (_playerBoard == null) return;
            _playerBoard.DOKill();
            _playerBoard.rectTransform.DOKill();
            _playerBoard.DOColor(on
                    ? ShrinePalette.WithAlpha(ShrinePalette.Candlelight, 0.42f)
                    : Color.white, 0.18f).SetEase(Ease.OutQuad);
            _playerBoard.rectTransform.DOScale(on ? Vector3.one * 1.012f : Vector3.one, 0.18f)
                .SetEase(Ease.OutQuad);
        }

        private void PulseTarget(RectTransform target, bool on)
        {
            target.DOKill();
            target.DOScale(Vector3.one * (on ? 1.04f : 1f), 0.16f).SetEase(Ease.OutQuad);
        }

        private Vector2 ToCanvas(RectTransform target) =>
            (Vector2)transform.InverseTransformPoint(target.position);

        private void RefreshAll()
        {
            if (_model == null || !_model.IsStarted || _playerPlate == null) return;
            Participant player = _model.Player;
            Participant opponent = _model.Opponent;

            _roundText.text = $"ROUND {_model.Round}";
            _playerPlate.Bind(player.DisplayName, player.Patron);
            _playerPlate.SetHealth(player.Health, player.MaxHealth, true);
            _playerPlate.SetShield(player.Shield);
            _rivalPlate.Bind(opponent.DisplayName, opponent.Patron);
            _rivalPlate.SetHealth(opponent.Health, opponent.MaxHealth, true);
            _rivalPlate.SetShield(opponent.Shield);
            _rivalPlate.SetHandCount(opponent.Hand.Count);
            _turnPhase.text = player.DeckCount.ToString();
            _turnPhase.color = player.DeckCount <= 3 ? ShrinePalette.BloodEmber : ShrinePalette.Candlelight;
            _deckText.text = "CARDS REMAIN";
            _manaBar.SetMana(player.Mana, player.MaxMana, true);
            SyncViews();

            foreach (CardView view in _views.Values)
            {
                if (view.Card == null) continue;
                bool inHand = player.Hand.Contains(view.Card);
                bool ready = inHand ? _model.CanPlay(Side.Player, view.Card.Uid)
                    : _model.CanAttack(Side.Player, view.Card.Uid);
                view.Refresh(ready, !inHand || player.Mana >= view.Card.Data.ManaCost);
            }
            PruneOrphans(player);
            Layout();
            UpdateEndTurnHint();
        }

        /// <summary>
        /// Guarantees a view for every card the model holds. Event handlers animate,
        /// but they must never be the only reason a card becomes visible: a missed
        /// callback would leave a hand card invisible or unpositioned.
        /// </summary>
        private void SyncViews()
        {
            EnsureViews(_model.Player.Hand, _handRoot);
            EnsureViews(_model.Player.Field, _playerFieldRoot);
            EnsureViews(_model.Opponent.Field, _opponentFieldRoot);
        }

        private void EnsureViews(IReadOnlyList<CardInstance> cards, Transform root)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CardInstance card = cards[i];
                if (_views.TryGetValue(card.Uid, out CardView existing))
                {
                    if (!existing.gameObject.activeSelf) existing.gameObject.SetActive(true);
                    if (existing.Rect.parent != root) existing.Rect.SetParent(root, false);
                    continue;
                }
                CardView view = GetView(card, root);
                view.Rect.SetParent(root, false);
                view.PlayEnter(0f);
            }
        }

        /// <summary>Glow the end-turn button when the player has nothing affordable left.</summary>
        private void UpdateEndTurnHint()
        {
            if (_endTurn == null || _model == null) return;
            bool myTurn = _model.CurrentSide == Side.Player && !_model.IsOver;
            bool canAct = false;
            if (myTurn)
            {
                foreach (CardInstance card in _model.Player.Hand)
                    if (_model.CanPlay(Side.Player, card.Uid)) { canAct = true; break; }
                if (!canAct)
                    foreach (CardInstance card in _model.Player.Field)
                        if (_model.CanAttack(Side.Player, card.Uid)) { canAct = true; break; }
            }
            _endTurn.SetAttention(myTurn && !canAct);
        }

        /// <summary>
        /// Cards removed without a destroy event (offerings) fall into the altar.
        /// Without this sweep their views would linger on screen.
        /// </summary>
        private void PruneOrphans(Participant player)
        {
            List<CardView> orphans = null;
            foreach (KeyValuePair<int, CardView> pair in _views)
            {
                CardInstance card = pair.Value.Card;
                if (card == null) continue;
                if (player.Hand.Contains(card) || player.Field.Contains(card)
                    || _model.Opponent.Field.Contains(card)) continue;
                orphans ??= new List<CardView>();
                orphans.Add(pair.Value);
            }
            if (orphans == null) return;
            Vector2 target = ToCanvas(_altarRect);
            foreach (CardView view in orphans)
            {
                _views.Remove(view.Card.Uid);
                view.Rect.SetParent(transform, true);
                view.Rect.SetAsLastSibling();
                view.PlayOffer(target, () => Recycle(view));
            }
        }

        private void Layout()
        {
            if (_model == null) return;
            LayoutRow(_model.Player.Hand, _handRoot, true);
            LayoutRow(_model.Player.Field, _playerFieldRoot, false);
            LayoutRow(_model.Opponent.Field, _opponentFieldRoot, false);
        }

        private void LayoutRow(IReadOnlyList<CardInstance> cards, Transform root, bool hand)
        {
            int count = cards.Count;
            if (count == 0) return;
            float spacing = count <= 1 ? 0f
                : Mathf.Min(hand ? MaxHandSpacing : FieldSpacing,
                    ((hand ? HandWidth : (5 - 1) * FieldSpacing) - CardWidth) / (count - 1));
            float width = (count - 1) * spacing;
            float scale = hand ? HandCardScale : FieldCardScale;
            for (int i = 0; i < count; i++)
            {
                if (!_views.TryGetValue(cards[i].Uid, out CardView view)) continue;
                if (view.Rect.parent != root) view.Rect.SetParent(root, false);
                float offset = i - (count - 1) * 0.5f;
                float x = -width * 0.5f + i * spacing;
                float arc = hand && count > 1 ? Mathf.Abs(offset) * 5f : 0f;
                view.SetHome(new Vector2(x, arc), scale);
                if (view.IsBusy) continue;
                view.Rect.DOKill();
                view.Rect.DOAnchorPos(new Vector2(x, arc), 0.32f).SetEase(Ease.OutCubic);
                view.Rect.DOScale(Vector3.one * scale, 0.32f).SetEase(Ease.OutCubic);
                float tilt = hand && count > 1 ? offset * -2.0f : 0f;
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
