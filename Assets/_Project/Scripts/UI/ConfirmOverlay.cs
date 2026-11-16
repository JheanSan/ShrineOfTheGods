using Shrine.Audio;
using Shrine.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    public sealed class ConfirmOverlay : ShrineScreen
    {
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _message;
        private ShrineButton _confirm;
        private Action _onConfirm;

        protected override void Build()
        {
            RectTransform scrim = UiKit.Stretch(transform, "Scrim");
            var image = scrim.gameObject.AddComponent<Image>();
            image.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.9f);
            RectTransform panel = UiKit.GildedPanel(transform, "ConfirmPanel", new Vector2(680, 380), Vector2.zero);
            _title = UiKit.Text(panel, "Title", string.Empty, 40, ShrinePalette.Candlelight, true);
            _title.rectTransform.anchoredPosition = new Vector2(0, 110);
            _message = UiKit.Text(panel, "Message", string.Empty, 24, ShrinePalette.BoneParchment);
            _message.rectTransform.sizeDelta = new Vector2(540, 110);
            _message.rectTransform.anchoredPosition = new Vector2(0, 20);
            _confirm = UiKit.Button(panel, "Confirm", "CONFIRM", new Vector2(260, 60), new Vector2(-140, -110), Confirm);
            UiKit.Button(panel, "Cancel", "CANCEL", new Vector2(260, 60), new Vector2(140, -110), Close, false);
        }

        public void Open(string title, string message, Action onConfirm)
        {
            _title.text = title;
            _message.text = message;
            _onConfirm = onConfirm;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void Confirm()
        {
            Action action = _onConfirm;
            Close();
            action?.Invoke();
        }

        public void Close()
        {
            _onConfirm = null;
            gameObject.SetActive(false);
            UiAudioBus.PlayBack();
        }
    }
}
