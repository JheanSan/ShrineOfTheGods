using Shrine.Audio;
using Shrine.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Shrine.UI
{
    /// <summary>Delayed, screen-clamped tooltip with rich text rules.</summary>
    public sealed class TooltipOverlay : ShrineScreen
    {
        public static TooltipOverlay Active { get; private set; }

        private RectTransform _panel;
        private TextMeshProUGUI _text;
        private Coroutine _hide;

        protected override void Build()
        {
            _panel = UiKit.Centered(transform, "TooltipPanel", new Vector2(400, 180), Vector2.zero);
            var image = _panel.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = ProceduralArt.ButtonFrame(false);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.96f);
            image.raycastTarget = false;
            _text = UiKit.Text(_panel, "TooltipText", string.Empty, 20, ShrinePalette.BoneParchment);
            _text.margin = new Vector4(18, 12, 18, 12);
            Group.alpha = 1f;
            gameObject.SetActive(true);
            Active = this;
            Hide();
        }

        private void OnDestroy()
        {
            if (Active == this) Active = null;
        }

        public void Show(string message, Vector2 screenPosition)
        {
            if (_hide != null) StopCoroutine(_hide);
            _text.text = message;
            _text.textWrappingMode = TextWrappingModes.Normal;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,
                screenPosition, null, out local);
            float x = Mathf.Clamp(local.x + 28f, -760f, 360f);
            float y = Mathf.Clamp(local.y - 20f, -420f, 420f);
            _panel.anchoredPosition = new Vector2(x, y);
            _panel.gameObject.SetActive(true);
            _panel.DOKill();
            _panel.localScale = Vector3.one * 0.94f;
            _panel.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack, 1.4f);
            CanvasGroup group = _panel.GetComponent<CanvasGroup>();
            if (group == null) group = _panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;
        }

        public void Hide()
        {
            if (_panel == null) return;
            _panel.gameObject.SetActive(false);
        }
    }
}
