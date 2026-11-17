using Shrine.Audio;
using Shrine.Core;
using System.Collections.Generic;
using DG.Tweening;
using Shrine.Data;
using Shrine.Gameplay;
using TMPro;
using UnityEngine;

namespace Shrine.UI
{
    public sealed class CovenantScreen : ShrineScreen
    {
        private readonly List<CardView> _views = new List<CardView>();
        private readonly List<RectTransform> _slots = new List<RectTransform>();
        private readonly List<TextMeshProUGUI> _slotNames = new List<TextMeshProUGUI>();
        private readonly Dictionary<PatronKind, UnityEngine.UI.Image> _patronMarks = new Dictionary<PatronKind, UnityEngine.UI.Image>();
        private TextMeshProUGUI _counter;
        private ShrineButton _seal;
        private ElementKind? _filter;

        protected override void Build()
        {
            RectTransform background = UiKit.Stretch(transform, "MythicBackground");
            var image = background.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = UiKit.LoadSprite("FotosManageDeckScene/fondomitico");
            image.color = ShrinePalette.WithAlpha(Color.white, 0.42f);
            image.raycastTarget = false;
            background.DOScale(1.06f, 16f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            UiKit.Image(transform, "Vignette", ProceduralArt.Vignette(), ShrinePalette.WithAlpha(Color.white, 0.85f));

            UiKit.TextAt(transform, "Heading", "THE COVENANT", 56, ShrinePalette.Candlelight,
                new Vector2(600, 80), new Vector2(-560, 470), true, TMPro.TextAlignmentOptions.MidlineLeft);
            _counter = UiKit.TextAt(transform, "Counter", string.Empty, 30, ShrinePalette.BoneParchment,
                new Vector2(420, 60), new Vector2(620, 470), false, TMPro.TextAlignmentOptions.MidlineRight);

            for (int i = 0; i < 8; i++)
            {
                RectTransform slot = UiKit.Centered(transform, "DeckSlot" + i, new Vector2(124, 178),
                    new Vector2(-700 + i * 160, 350));
                UnityEngine.UI.Image slotImage = slot.gameObject.AddComponent<UnityEngine.UI.Image>();
                slotImage.sprite = UiKit.LoadSprite("FotosManageDeckScene/Fondo de carta");
                slotImage.color = ShrinePalette.WithAlpha(Color.white, 0.5f);
                slotImage.raycastTarget = false;
                TextMeshProUGUI name = UiKit.Text(slot, "SlotName", "—", 18, ShrinePalette.MoonSilver, false);
                _slots.Add(slot);
                _slotNames.Add(name);
            }

            string[] filters = { "ALL", "FIRE", "WATER", "EARTH", "AIR" };
            for (int i = 0; i < filters.Length; i++)
            {
                int index = i;
                UiKit.Button(transform, "Filter" + filters[i], filters[i], new Vector2(150, 46),
                    new Vector2(-720 + i * 170, 210), () => SetFilter(index), false);
            }

            IReadOnlyList<CardDefinition> catalog = ShrineDatabase.Cards;
            for (int i = 0; i < catalog.Count; i++)
            {
                CardView view = CreateView(catalog[i].Data, i);
                _views.Add(view);
            }

            PatronData[] patrons = PatronCatalog();
            for (int i = 0; i < patrons.Length; i++)
            {
                PatronKind kind = patrons[i].Kind;
                ShrineButton button = UiKit.Button(transform, "Patron" + kind, patrons[i].DisplayName.Split(',')[0],
                    new Vector2(220, 54), new Vector2(-600 + i * 250, -460),
                    () => SelectPatron(kind), false);
                UnityEngine.UI.Image mark = UiKit.ImageAt(transform, "PatronMark" + kind,
                    ProceduralArt.RadialGlow(ShrinePalette.EmberGold),
                    ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0f), new Vector2(80, 80),
                    new Vector2(-600 + i * 250, -460));
                _patronMarks[kind] = mark;
                _ = button;
            }

            _seal = UiKit.Button(transform, "SealButton", "SEAL THE COVENANT", new Vector2(420, 74),
                new Vector2(670, -455), App.StartBattle);
            UiKit.Button(transform, "BackButton", "BACK", new Vector2(180, 54),
                new Vector2(-810, -500), () => App.Router.Back(), false);
        }

        private CardView CreateView(CardData data, int index)
        {
            int column = index % 6;
            int row = index / 6;
            var holder = UiKit.Centered(transform, "CardHolder_" + data.Id, new Vector2(140, 200),
                new Vector2(-650 + column * 260, 74 - row * 226));
            var view = new GameObject("CardView_" + data.Id).AddComponent<CardView>();
            view.Initialize(holder, new Vector2(128, 184), true);
            view.Bind(new CardInstance(data), true, false);
            view.Clicked += ToggleCard;
            return view;
        }

        private void ToggleCard(CardView view)
        {
            string id = view.Card.Data.Id;
            List<string> deck = App.Profile.Deck;
            if (deck.Contains(id))
            {
                deck.Remove(id);
                UiAudioBus.PlayBack();
            }
            else if (deck.Count < ShrineDatabase.Balance.DeckSize)
            {
                deck.Add(id);
                UiAudioBus.PlayClick();
            }
            else
            {
                UiAudioBus.PlayDeny();
                view.Rect.DOShakePosition(0.18f, 6f, 14, 90f, false, true);
            }
            App.SaveProfile();
            Refresh();
        }

        private void SetFilter(int index)
        {
            _filter = index == 0 ? (ElementKind?)null : (ElementKind)(index - 1);
            Refresh();
        }

        private void SelectPatron(PatronKind kind)
        {
            App.Profile.Patron = kind;
            App.SaveProfile();
            Refresh();
        }

        private static PatronData[] PatronCatalog()
        {
            var list = new List<PatronData>();
            foreach (PatronDefinition definition in ShrineDatabase.Patrons)
                list.Add(definition.Data);
            return list.ToArray();
        }

        public override void OnEnter()
        {
            Refresh();
        }

        public override void OnExit()
        {
            transform.DOKill();
        }

        private void Refresh()
        {
            List<string> deck = App.Profile.Deck;
            _counter.text = $"{deck.Count} / {ShrineDatabase.Balance.DeckSize} BOUND";
            for (int i = 0; i < _slots.Count; i++)
            {
                string name = i < deck.Count ? deck[i] : "—";
                _slotNames[i].text = char.ToUpperInvariant(name[0]) + name.Substring(1);
                _slotNames[i].color = i < deck.Count ? ShrinePalette.Candlelight : ShrinePalette.MoonSilver;
            }
            foreach (CardView view in _views)
            {
                bool visible = !_filter.HasValue || view.Card.Data.Element == _filter.Value;
                view.Rect.parent.gameObject.SetActive(visible);
                bool selected = deck.Contains(view.Card.Data.Id);
                view.Rect.localScale = selected ? Vector3.one * 1.05f : Vector3.one;
            }
            foreach (KeyValuePair<PatronKind, UnityEngine.UI.Image> pair in _patronMarks)
                pair.Value.color = pair.Key == App.Profile.Patron
                    ? ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.55f)
                    : ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0f);
            _seal.SetInteractable(deck.Count == ShrineDatabase.Balance.DeckSize);
        }
    }
}
