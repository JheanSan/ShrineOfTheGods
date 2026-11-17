using Shrine.Audio;
using Shrine.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>Shared hover/press/focus behaviour for every button in the game.</summary>
    public sealed class ShrineButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private Button _button;
        private Image _image;
        private TextMeshProUGUI _label;
        private bool _primary;
        private Color _baseColor;
        private Color _baseText;
        private Vector3 _baseScale = Vector3.one;
        private bool _hovered;
        private bool _selected;
        private bool _attentionOn;
        private Tween _attention;

        public void Initialize(Button button, Image image, TextMeshProUGUI label, bool primary)
        {
            _button = button;
            _image = image;
            _label = label;
            _primary = primary;
            _baseColor = image.color;
            _baseText = label.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            _hovered = true;
            Animate(1.035f, _primary ? ShrinePalette.Candlelight : ShrinePalette.EmberGold, 0.16f);
            UiAudioBus.PlayHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            if (!_selected) Animate(1f, _baseColor, 0.22f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            transform.DOScale(_baseScale * 0.965f, 0.08f).SetEase(Ease.InQuad);
            UiAudioBus.PlayClick();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            transform.DOScale(_baseScale * 1.01f, 0.24f).SetEase(Ease.OutBack, 1.6f)
                .OnComplete(() => Animate(_hovered || _selected ? 1.035f : 1f,
                    _hovered ? ShrinePalette.Candlelight : _baseColor, 0.12f));
        }

        public void OnSelect(BaseEventData eventData)
        {
            _selected = true;
            if (!_hovered) Animate(1.03f, ShrinePalette.SpiritCyan, 0.16f);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _selected = false;
            if (!_hovered) Animate(1f, _baseColor, 0.22f);
        }

        public void SetInteractable(bool value)
        {
            _button.interactable = value;
            _image.color = value ? _baseColor : ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.35f);
            _label.color = value ? _baseText : ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.5f);
            if (!value) SetAttention(false);
        }

        /// <summary>Soft loop used when the button is the obvious next action.</summary>
        public void SetAttention(bool attention)
        {
            if (attention == _attentionOn) return;
            _attentionOn = attention;
            _attention?.Kill();
            _attention = null;
            transform.DOKill();
            _image.DOKill();
            if (!attention)
            {
                transform.localScale = _baseScale;
                if (!_hovered && !_selected) _image.color = _baseColor;
                return;
            }
            if (!_button.interactable) return;
            _image.DOColor(ShrinePalette.Candlelight, 0.22f);
            _attention = transform.DOScale(_baseScale * 1.035f, 0.62f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        public void SetLabel(string text)
        {
            if (_label != null) _label.text = text;
        }

        private void Animate(float scale, Color color, float duration)
        {
            transform.DOKill();
            _image.DOKill();
            _label.DOKill();
            transform.DOScale(_baseScale * scale, duration).SetEase(Ease.OutCubic);
            _image.DOColor(color, duration).SetEase(Ease.OutQuad);
            _label.DOColor(scale > 1f ? ShrinePalette.Candlelight : _baseText, duration);
        }

        private void OnDisable()
        {
            transform.DOKill();
            _image?.DOKill();
            _label?.DOKill();
            _attention?.Kill();
            _attention = null;
            _attentionOn = false;
        }
    }
}
