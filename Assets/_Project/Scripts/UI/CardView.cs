using Shrine.Audio;
using Shrine.Core;
using DG.Tweening;
using Shrine.Data;
using Shrine.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>Visual representation of one card. Views are pooled and reused.</summary>
    public sealed class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private Image _frame;
        private Image _art;
        private Image _glow;
        private TextMeshProUGUI _name;
        private TextMeshProUGUI _cost;
        private TextMeshProUGUI _stats;
        private TextMeshProUGUI _element;
        private CardInstance _card;
        private Vector2 _homePosition;
        private Transform _dragParent;
        private bool _dragging;
        private bool _interactive;

        public CardInstance Card => _card;
        public bool Interactive => _interactive;
        public RectTransform Rect => (RectTransform)transform;

        public System.Action<CardView> Clicked;
        public System.Action<CardView> Hovered;
        public System.Action<CardView, Vector2> DragEnded;
        public System.Func<CardView, bool> DragAllowed;

        public void Initialize(Transform parent, Vector2 size, bool compact)
        {
            var rect = gameObject.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);

            _glow = AddImage(rect, "ReadyGlow", ProceduralArt.RadialGlow(ShrinePalette.SpiritCyan),
                ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f), size * 1.45f, Vector2.zero);

            _frame = AddImage(rect, "Frame", ProceduralArt.CardFrame(),
                Color.white, size, Vector2.zero);
            _frame.type = Image.Type.Sliced;
            _frame.raycastTarget = true;

            _art = AddImage(rect, "Art", null, Color.white,
                new Vector2(size.x - 34f, size.y * 0.46f), new Vector2(0f, size.y * 0.05f));

            float top = size.y * 0.5f - 24f;
            float bottom = -size.y * 0.5f + 24f;
            _name = AddText(rect, "Name", string.Empty, compact ? 13 : 17, ShrinePalette.Candlelight,
                new Vector2(size.x - 48f, 28f), new Vector2(14f, top), true);
            _element = AddText(rect, "Element", string.Empty, compact ? 11 : 13, ShrinePalette.BoneParchment,
                new Vector2(size.x - 48f, 20f), new Vector2(14f, top - 24f), false);
            _stats = AddText(rect, "Stats", string.Empty, compact ? 16 : 20, ShrinePalette.Candlelight,
                new Vector2(size.x - 16f, 34f), new Vector2(0f, bottom), true);
            _cost = AddText(rect, "Cost", string.Empty, compact ? 20 : 24, ShrinePalette.Candlelight,
                new Vector2(38f, 38f), new Vector2(-size.x * 0.5f + 20f, top), true);
        }

        public void Bind(CardInstance card, bool interactive, bool ready)
        {
            _card = card;
            _interactive = interactive;
            Refresh(ready);
        }

        public void Refresh(bool ready)
        {
            if (_card == null) return;
            CardData data = _card.Data;
            _name.text = data.DisplayName;
            _element.text = data.Element.ToString().ToUpperInvariant();
            _element.color = ShrinePalette.Element(data.Element);
            _cost.text = data.ManaCost.ToString();
            _stats.text = $"{data.Attack}  ·  {_card.CurrentDefense}/{_card.MaxDefense}";
            _glow.color = ready
                ? ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.28f)
                : ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f);
            Sprite art = string.IsNullOrEmpty(data.ArtResourcePath)
                ? ProceduralArt.ElementSigil(data.Element)
                : UiKit.LoadSprite(data.ArtResourcePath);
            if (art == null) art = ProceduralArt.ElementSigil(data.Element);
            _art.sprite = art;
            _art.color = art.texture.width <= 128 ? ShrinePalette.WithAlpha(ShrinePalette.Element(data.Element), 0.9f)
                : Color.white;
            _frame.color = interactiveTint();
        }

        public void SetHome(Vector2 position)
        {
            _homePosition = position;
            Rect.anchoredPosition = position;
        }

        public void AnimateToHome(float duration = 0.3f)
        {
            Rect.DOAnchorPos(_homePosition, duration).SetEase(Ease.OutCubic);
            Rect.DORotate(Vector3.zero, duration).SetEase(Ease.OutCubic);
            Rect.DOScale(Vector3.one, duration).SetEase(Ease.OutBack, 1.2f);
        }

        public void PlayEnter(float delay)
        {
            Rect.anchoredPosition = _homePosition + new Vector2(0f, 180f);
            Rect.localScale = Vector3.one * 0.8f;
            Rect.DOAnchorPos(_homePosition, 0.42f).SetDelay(delay).SetEase(Ease.OutCubic);
            Rect.DOScale(Vector3.one, 0.42f).SetDelay(delay).SetEase(Ease.OutBack, 1.3f);
        }

        private Color interactiveTint() => _interactive ? Color.white : ShrinePalette.WithAlpha(Color.white, 0.72f);

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
            float size, Color color, Vector2 rectSize, Vector2 position, bool display)
        {
            RectTransform rect = UiKit.Centered(parent, name, rectSize, position);
            TextMeshProUGUI text = UiKit.Text(rect, "Label", content, size, color, display);
            return text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactive) return;
            Rect.DOScale(Vector3.one * 1.06f, 0.16f).SetEase(Ease.OutQuad);
            Rect.DOLocalRotate(new Vector3(0f, 0f, -2f), 0.16f).SetEase(Ease.OutQuad);
            Hovered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipOverlay.Active?.Hide();
            if (_dragging) return;
            Rect.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutCubic);
            Rect.DOLocalRotate(Vector3.zero, 0.22f).SetEase(Ease.OutCubic);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_interactive) Clicked?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_interactive || DragAllowed != null && !DragAllowed(this)) return;
            _dragging = true;
            _dragParent = Rect.parent;
            Rect.SetParent(Rect.parent.parent, true);
            Rect.SetAsLastSibling();
            Rect.DOKill();
            Rect.localScale = Vector3.one * 1.08f;
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
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging) return;
            _dragging = false;
            if (_dragParent != null) Rect.SetParent(_dragParent, true);
            DragEnded?.Invoke(this, eventData.position);
        }

        private void OnDisable()
        {
            Rect?.DOKill();
        }
    }
}
