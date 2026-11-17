using Shrine.Audio;
using DG.Tweening;
using Shrine.Data;
using Shrine.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>
    /// One card. Layout is expressed as fractions of the card size so the same view
    /// reads correctly in hand, on the battlefield and in the compact covenant grid.
    /// </summary>
    public sealed class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private const float HoverLift = 34f;

        private CanvasGroup _group;
        private Image _frame;
        private Image _art;
        private Image _sigilGlow;
        private Image _glow;
        private Image _flash;
        private Image _costGem;
        private Image _attackGem;
        private Image _healthGem;
        private Image _ribbon;
        private Image _ribbonEdge;
        private Image _ribbonGlyph;
        private Image _elementMark;
        private TextMeshProUGUI _name;
        private TextMeshProUGUI _cost;
        private TextMeshProUGUI _attack;
        private TextMeshProUGUI _health;
        private TextMeshProUGUI _ribbonName;
        private TextMeshProUGUI _advantage;

        private CardInstance _card;
        private Vector2 _homePosition;
        private float _homeScale = 1f;
        private Transform _dragParent;
        private bool _dragging;
        private bool _attacking;
        private bool _interactive;
        private bool _ready;
        private bool _hovered;
        private Tween _breath;
        private int _artUid = -1;

        public CardInstance Card => _card;
        public bool Interactive => _interactive;
        public bool IsBusy => _dragging || _hovered || _attacking;
        public RectTransform Rect => (RectTransform)transform;

        public System.Action<CardView> Clicked;
        public System.Action<CardView> Hovered;
        public System.Action<CardView> DragStarted;
        public System.Action<CardView, Vector2> Dragging;
        public System.Action<CardView, Vector2> DragEnded;
        public System.Func<CardView, bool> DragAllowed;

        public void Initialize(Transform parent, Vector2 size, bool compact)
        {
            var rect = gameObject.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            _group = gameObject.AddComponent<CanvasGroup>();

            float w = size.x;
            float h = size.y;

            AddImage(rect, "Shadow", ProceduralArt.CardShadow(), new Color(1f, 1f, 1f, 0.55f),
                new Vector2(w * 1.14f, h * 1.10f), new Vector2(0f, -h * 0.04f));

            _glow = AddImage(rect, "ReadyGlow", ProceduralArt.RadialGlow(ShrinePalette.SpiritCyan),
                ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f), size * 1.5f, Vector2.zero);

            _frame = AddImage(rect, "Frame", ProceduralArt.CardFrame(), Color.white, size, Vector2.zero);
            _frame.type = Image.Type.Sliced;
            _frame.raycastTarget = true;

            float artWidth = w * 0.88f;
            float artHeight = h * 0.36f;
            float artY = h * 0.14f;
            _sigilGlow = AddImage(rect, "SigilGlow", ProceduralArt.RadialGlow(ShrinePalette.EmberGold),
                ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.32f),
                new Vector2(artWidth * 0.95f, artHeight * 1.10f), new Vector2(0f, artY));

            AddImage(rect, "ArtWell", ProceduralArt.ArtWell(), Color.white,
                new Vector2(artWidth, artHeight), new Vector2(0f, artY));
            _art = AddImage(rect, "Art", null, Color.white,
                new Vector2(artWidth - 6f, artHeight - 6f), new Vector2(0f, artY));
            _art.preserveAspect = true;

            // Element watermark over the art: the hue and the glyph repeat the ribbon's
            // message so a glance across the hand is enough to read the four elements.
            _elementMark = AddImage(rect, "ElementMark", ProceduralArt.ElementSigil(ElementKind.Fire),
                ShrinePalette.WithAlpha(ShrinePalette.PaleGale, 0.30f),
                Vector2.one * (artWidth * 0.42f), new Vector2(w * 0.26f, artY + artHeight * 0.20f));

            Image artShade = AddImage(rect, "ArtShade", ProceduralArt.Vignette(),
                ShrinePalette.WithAlpha(Color.white, 0.42f),
                new Vector2(artWidth, artHeight), new Vector2(0f, artY));
            artShade.raycastTarget = false;

            AddImage(rect, "NamePlate", null,
                ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.80f),
                new Vector2(w * 0.92f, h * 0.092f), new Vector2(0f, h * 0.40f));

            _name = AddText(rect, "Name", string.Empty, 0.092f * w, ShrinePalette.Candlelight,
                new Vector2(w * 0.62f, h * 0.086f), new Vector2(w * 0.06f, h * 0.40f),
                TextAlignmentOptions.Center, true);
            _name.characterSpacing = 2f;

            // Element ribbon: colour, glyph and the element it defeats, all in one band.
            _ribbon = AddImage(rect, "ElementRibbon", null, ShrinePalette.EmberOrange,
                new Vector2(w * 0.94f, h * 0.105f), new Vector2(0f, -h * 0.062f));
            _ribbonEdge = AddImage(rect, "RibbonEdge", null,
                ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.7f),
                new Vector2(w * 0.94f, 1.5f), new Vector2(0f, -h * 0.115f));
            _ribbonGlyph = AddImage(rect, "RibbonGlyph", ProceduralArt.ElementSigil(ElementKind.Fire),
                Color.white, Vector2.one * (w * 0.135f), new Vector2(-w * 0.395f, -h * 0.062f));
            _ribbonName = AddText(rect, "RibbonName", string.Empty, 0.065f * w, ShrinePalette.MidnightInk,
                new Vector2(w * 0.42f, h * 0.08f), new Vector2(-w * 0.10f, -h * 0.062f),
                TextAlignmentOptions.Left, true);
            _ribbonName.characterSpacing = 6f;
            _advantage = AddText(rect, "Advantage", string.Empty, 10.5f,
                ShrinePalette.MidnightInk,
                new Vector2(w * 0.30f, h * 0.08f), new Vector2(w * 0.285f, -h * 0.062f),
                TextAlignmentOptions.Right, false);
            _advantage.enableAutoSizing = false;

            AddText(rect, "AttackLabel", "ATK", 10f, ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.9f),
                new Vector2(w * 0.3f, 11f), new Vector2(-w * 0.315f, -h * 0.202f),
                TextAlignmentOptions.Center, false);
            AddText(rect, "HealthLabel", "LIFE", 10f, ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.9f),
                new Vector2(w * 0.3f, 11f), new Vector2(w * 0.315f, -h * 0.202f),
                TextAlignmentOptions.Center, false);

            AddImage(rect, "TypeRule", null, ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.5f),
                new Vector2(w * 0.66f, 1.5f), new Vector2(0f, -h * 0.163f));

            float gem = w * 0.22f;
            _costGem = AddImage(rect, "CostGem", ProceduralArt.Gem(ShrinePalette.EmberGold),
                Color.white, Vector2.one * gem,
                new Vector2(-w * 0.345f, h * 0.372f));
            _cost = AddText(rect, "Cost", string.Empty, 0.115f * w, ShrinePalette.Candlelight,
                Vector2.one * gem, new Vector2(-w * 0.345f, h * 0.372f), TextAlignmentOptions.Center, true);

            _attackGem = AddImage(rect, "AttackGem", ProceduralArt.Gem(ShrinePalette.BloodEmber),
                Color.white, Vector2.one * gem,
                new Vector2(-w * 0.315f, -h * 0.33f));
            _attack = AddText(rect, "Attack", string.Empty, 0.112f * w, ShrinePalette.Candlelight,
                Vector2.one * gem, new Vector2(-w * 0.315f, -h * 0.33f), TextAlignmentOptions.Center, true);

            _healthGem = AddImage(rect, "HealthGem", ProceduralArt.Gem(ShrinePalette.MossDevotion),
                Color.white, Vector2.one * gem,
                new Vector2(w * 0.315f, -h * 0.33f));
            _health = AddText(rect, "Health", string.Empty, 0.112f * w, ShrinePalette.Candlelight,
                Vector2.one * gem, new Vector2(w * 0.315f, -h * 0.33f), TextAlignmentOptions.Center, true);

            _flash = AddImage(rect, "HitFlash", null, new Color(1f, 1f, 1f, 0f), size, Vector2.zero);
        }

        public void Bind(CardInstance card, bool interactive, bool ready)
        {
            _card = card;
            _interactive = interactive;
            Refresh(ready);
        }

        public void Refresh(bool ready, bool affordable = true)
        {
            if (_card == null) return;
            _ready = ready;
            CardData data = _card.Data;
            _name.text = data.DisplayName;
            _ribbon.color = ShrinePalette.WithAlpha(ShrinePalette.Element(data.Element), 0.94f);
            _ribbonGlyph.sprite = ProceduralArt.ElementSigil(data.Element);
            _ribbonGlyph.color = ShrinePalette.ElementInk(data.Element);
            _ribbonName.text = data.Element.ToString().ToUpperInvariant();
            _ribbonName.color = ShrinePalette.ElementInk(data.Element);
            _advantage.text = "> " + ElementRules.Beats(data.Element).ToString().ToUpperInvariant();
            _advantage.color = ShrinePalette.WithAlpha(ShrinePalette.ElementInk(data.Element), 0.82f);
            _elementMark.sprite = ProceduralArt.ElementSigil(data.Element);
            _elementMark.color = ShrinePalette.WithAlpha(ShrinePalette.ElementBright(data.Element), 0.38f);
            _cost.text = data.ManaCost.ToString();
            _cost.color = affordable ? ShrinePalette.Candlelight
                : ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.7f);
            _costGem.color = affordable ? Color.white : ShrinePalette.WithAlpha(Color.white, 0.4f);
            _attack.text = data.Attack.ToString();
            _health.text = _card.CurrentDefense.ToString();

            bool damaged = _card.CurrentDefense < _card.MaxDefense;
            _healthGem.sprite = ProceduralArt.Gem(damaged
                ? ShrinePalette.BloodEmber
                : ShrinePalette.MossDevotion);
            _health.color = damaged ? ShrinePalette.Candlelight : ShrinePalette.BoneParchment;

            UpdateGlow();

            Sprite art = string.IsNullOrEmpty(data.ArtResourcePath)
                ? null
                : UiKit.LoadSprite(data.ArtResourcePath);
            bool painted = art != null && art.texture.width > 128;
            if (_artUid != _card.Uid)
            {
                _artUid = _card.Uid;
                _art.sprite = painted ? art : ProceduralArt.CardScene(data.Element, data.Id);
                _art.preserveAspect = true;
            }
            _art.color = painted
                ? ShrinePalette.WithAlpha(Color.white, 0.96f)
                : Color.white;
            _sigilGlow.color = ShrinePalette.WithAlpha(ShrinePalette.Element(data.Element),
                painted ? 0.08f : 0.14f);

            _frame.sprite = ProceduralArt.CardFrame(data.Element);
            _frame.color = _interactive ? Color.white : ShrinePalette.WithAlpha(Color.white, 0.62f);
            _frame.raycastTarget = _interactive;
        }

        public void SetHome(Vector2 position, float scale = 1f)
        {
            _homePosition = position;
            _homeScale = scale;
        }

        public void SnapHome()
        {
            Rect.DOKill();
            Rect.anchoredPosition = _homePosition;
            Rect.localScale = Vector3.one * _homeScale;
            Rect.localRotation = Quaternion.identity;
        }

        public void AnimateToHome(float duration = 0.3f)
        {
            Rect.DOAnchorPos(_homePosition, duration).SetEase(Ease.OutCubic);
            Rect.DORotate(Vector3.zero, duration).SetEase(Ease.OutCubic);
            Rect.DOScale(Vector3.one * _homeScale, duration).SetEase(Ease.OutBack, 1.2f);
        }

        public void PlayEnter(float delay)
        {
            Rect.anchoredPosition = _homePosition + new Vector2(0f, 180f);
            Rect.localScale = Vector3.one * (_homeScale * 0.8f);
            _group.alpha = 0f;
            _group.DOFade(1f, 0.24f).SetDelay(delay);
            Rect.DOAnchorPos(_homePosition, 0.42f).SetDelay(delay).SetEase(Ease.OutCubic);
            Rect.DOScale(Vector3.one * _homeScale, 0.42f).SetDelay(delay).SetEase(Ease.OutBack, 1.3f);
        }

        /// <summary>Anticipation, charge, impact callback, then a weighty return.</summary>
        public void PlayAttack(Vector2 targetLocalPosition, System.Action onImpact)
        {
            Rect.DOKill();
            _group.DOKill();
            _group.alpha = 1f;
            Rect.SetAsLastSibling();
            Vector2 home = _homePosition;
            Vector2 delta = targetLocalPosition - home;
            float distance = delta.magnitude;
            if (distance < 1f)
            {
                _attacking = false;
                onImpact?.Invoke();
                return;
            }
            Vector2 direction = delta / distance;
            Vector2 windUp = home - direction * 26f;
            Vector2 contact = targetLocalPosition - direction * Mathf.Min(96f, distance * 0.62f);

            _attacking = true;
            _breath?.Kill();
            _breath = null;
            _glow.DOKill();
            _glow.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.55f);
            _glow.DOFade(0f, 0.45f).SetEase(Ease.OutQuad);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(Rect.DOAnchorPos(windUp, 0.10f).SetEase(Ease.OutQuad));
            sequence.Join(Rect.DOScale(_homeScale * new Vector2(0.95f, 1.06f), 0.10f));
            sequence.Append(Rect.DOAnchorPos(contact, 0.16f).SetEase(Ease.InCubic));
            sequence.Join(Rect.DOScale(_homeScale * new Vector2(1.10f, 0.94f), 0.16f));
            sequence.AppendCallback(() => onImpact?.Invoke());
            sequence.AppendInterval(0.04f);
            sequence.Append(Rect.DOAnchorPos(home, 0.24f).SetEase(Ease.OutCubic));
            sequence.Join(Rect.DOScale(Vector3.one * _homeScale, 0.24f).SetEase(Ease.OutBack, 1.2f));
            sequence.OnComplete(() =>
            {
                _attacking = false;
                UpdateGlow();
            });
        }

        public void PlayHit(Color? tint = null)
        {
            Rect.DOKill();
            Rect.localScale = _homeScale * new Vector3(1.10f, 0.90f);
            Rect.DOScale(Vector3.one * _homeScale, 0.26f).SetEase(Ease.OutBack, 1.5f);
            _flash.DOKill();
            Color glow = tint ?? ShrinePalette.Candlelight;
            _flash.color = ShrinePalette.WithAlpha(glow, 0.78f);
            _flash.DOFade(0f, 0.22f).SetEase(Ease.OutQuad);
        }

        public void PlayDeath(float delay, System.Action onDone)
        {
            Rect.DOKill();
            _group.DOKill();
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(delay);
            sequence.Append(Rect.DOScale(_homeScale * new Vector3(1.12f, 0.94f), 0.10f).SetEase(Ease.OutQuad));
            sequence.Append(Rect.DOScale(_homeScale * new Vector3(0.35f, 0.08f), 0.26f).SetEase(Ease.InCubic));
            sequence.Join(Rect.DORotate(new Vector3(0f, 0f, -12f), 0.26f));
            sequence.Join(_group.DOFade(0f, 0.26f));
            sequence.OnComplete(() => onDone?.Invoke());
        }

        /// <summary>Card is absorbed by the shrine: it spirals into the altar light.</summary>
        public void PlayOffer(Vector2 canvasLocalPosition, System.Action onDone)
        {
            Rect.DOKill();
            _group.DOKill();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(Rect.DOAnchorPos(canvasLocalPosition, 0.34f).SetEase(Ease.InCubic));
            sequence.Join(Rect.DOScale(Vector3.one * 0.14f, 0.34f).SetEase(Ease.InCubic));
            sequence.Join(Rect.DORotate(new Vector3(0f, 0f, 150f), 0.34f).SetEase(Ease.InQuad));
            sequence.Join(_group.DOFade(0f, 0.30f).SetDelay(0.05f));
            sequence.OnComplete(() => onDone?.Invoke());
        }

        /// <summary>Rejected action: the card refuses with a short red shudder.</summary>
        public void PlayDeny()
        {
            Rect.DOKill();
            Rect.DOShakeAnchorPos(0.22f, 9f, 22, 90f, false, true);
            _glow.color = ShrinePalette.WithAlpha(ShrinePalette.BloodEmber, 0.55f);
            _glow.DOKill();
            _glow.DOFade(0f, 0.35f).SetDelay(0.12f)
                .OnComplete(() => _glow.color = _ready
                    ? ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.30f)
                    : ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f));
        }

        public void SetTargetable(bool targetable)
        {
            if (targetable)
            {
                _glow.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.40f);
                Rect.DOScale(Vector3.one * (_homeScale * 1.04f), 0.12f).SetEase(Ease.OutQuad);
            }
            else
            {
                _glow.color = _ready
                    ? ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.30f)
                    : ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f);
                Rect.DOScale(Vector3.one * _homeScale, 0.12f).SetEase(Ease.OutQuad);
            }
        }

        private static Image AddImage(RectTransform parent, string name, Sprite sprite, Color color,
            Vector2 size, Vector2 position)
        {
            RectTransform rect = UiKit.Centered(parent, name, size, position);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = false;
            return image;
        }

        private static TextMeshProUGUI AddText(RectTransform parent, string name, string content,
            float size, Color color, Vector2 rectSize, Vector2 position,
            TextAlignmentOptions alignment, bool display)
        {
            RectTransform rect = UiKit.Centered(parent, name, rectSize, position);
            TextMeshProUGUI text = UiKit.Text(rect, "Label", content, size, color, display, alignment);
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(7f, size * 0.62f);
            text.fontSizeMax = size;
            // Small labels are laid out in rects barely taller than the glyphs; without
            // this the ascenders of the element tag and the stat labels are clipped.
            text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactive) return;
            _hovered = true;
            _breath?.Kill();
            _breath = null;
            _glow.DOKill();
            _glow.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.30f);
            Rect.DOKill();
            Rect.SetAsLastSibling();
            Rect.DOAnchorPos(_homePosition + new Vector2(0f, HoverLift), 0.18f).SetEase(Ease.OutCubic);
            Rect.DOScale(Vector3.one * (_homeScale * 1.16f), 0.18f).SetEase(Ease.OutQuad);
            Rect.DOLocalRotate(Vector3.zero, 0.18f);
            Hovered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipOverlay.Active?.Hide();
            _hovered = false;
            if (_dragging) return;
            ReturnHome();
            UpdateGlow();
        }

        /// <summary>Playable cards breathe; everything else stays quiet.</summary>
        private void UpdateGlow()
        {
            _breath?.Kill();
            _breath = null;
            _glow.DOKill();
            if (!_ready || _hovered || _dragging || !_interactive)
            {
                _glow.color = ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f);
                return;
            }
            _glow.color = ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.18f);
            _breath = _glow.DOFade(0.44f, 1.05f).SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void ReturnHome()
        {
            Rect.DOKill();
            Rect.DOAnchorPos(_homePosition, 0.24f).SetEase(Ease.OutCubic);
            Rect.DOScale(Vector3.one * _homeScale, 0.24f).SetEase(Ease.OutCubic);
            Rect.DOLocalRotate(Vector3.zero, 0.24f).SetEase(Ease.OutCubic);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_interactive) Clicked?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_interactive || DragAllowed != null && !DragAllowed(this)) return;
            _dragging = true;
            _breath?.Kill();
            _breath = null;
            _glow.DOKill();
            _glow.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.5f);
            _dragParent = Rect.parent;
            Rect.SetParent(Rect.parent.parent, true);
            Rect.SetAsLastSibling();
            Rect.DOKill();
            Rect.localScale = Vector3.one * 1.06f;
            DragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)Rect.parent, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                Rect.anchoredPosition = local;
            }
            Rect.localRotation = Quaternion.Euler(0f, 0f, 3f);
            Dragging?.Invoke(this, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging) return;
            _dragging = false;
            if (_dragParent != null) Rect.SetParent(_dragParent, true);
            DragEnded?.Invoke(this, eventData.position);
            UpdateGlow();
        }

        private void OnDisable()
        {
            Rect?.DOKill();
            _group?.DOKill();
            if (_group != null) _group.alpha = 1f;
            _dragging = false;
            _hovered = false;
        }
    }
}
